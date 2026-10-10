using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace ADB_Connect;

internal static class UpdateHandoff
{
    internal static async Task StartAsync(string payload, bool installer, string hash, string job)
    {
        string target = Path.GetFullPath(AppContext.BaseDirectory);
        if (!installer)
        {
            string probe = UpdateReplacement.SafeChild(target, Guid.NewGuid().ToString("N") + ".write-test");
            using (File.Create(probe)) { }
            File.Delete(probe);
        }
        string runner = Path.Combine(job, "Runner");
        Directory.CreateDirectory(runner);
        string executable = Environment.ProcessPath ?? throw new IOException("Cannot locate the running application.");
        string helper = Path.Combine(runner, Path.GetFileName(executable));
        File.Copy(executable, helper, true);
        // Framework-dependent developer builds also need their assembly and runtime configuration.
        foreach (string suffix in new[] { ".dll", ".deps.json", ".runtimeconfig.json" })
        {
            string dependency = Path.ChangeExtension(executable, null) + suffix;
            if (File.Exists(dependency)) File.Copy(dependency, Path.Combine(runner, Path.GetFileName(dependency)), true);
        }
        using var parent = Process.GetCurrentProcess();
        var plan = new UpdatePlan(parent.Id, parent.StartTime.ToUniversalTime().Ticks, target, payload, installer, hash);
        string path = Path.Combine(job, "update-plan.json");
        File.WriteAllText(path, JsonSerializer.Serialize(plan));
        var start = new ProcessStartInfo(helper) { UseShellExecute = false, WorkingDirectory = runner };
        start.ArgumentList.Add("--apply-update"); start.ArgumentList.Add(path);
        using var process = Process.Start(start) ?? throw new IOException("Cannot start the update helper.");
        for (int i = 0; i < 100; i++)
        {
            if (File.Exists(path + ".ready")) return;
            if (process.HasExited) throw new IOException("The update helper could not start. The application has not been closed.");
            await Task.Delay(100);
        }
        // No ready signal: terminate only our own newly started helper before continuing.
        if (!process.HasExited) process.Kill();
        throw new IOException("The update helper did not become ready.");
    }

    internal static void Run(string path)
    {
        string job = Path.GetDirectoryName(Path.GetFullPath(path))!;
        try
        {
            var plan = JsonSerializer.Deserialize<UpdatePlan>(File.ReadAllText(path)) ?? throw new IOException("Invalid update plan.");
            if (!File.Exists(UpdateReplacement.SafeChild(plan.Target, "ADB Connect.exe")))
                throw new IOException("Application destination is missing.");
            File.WriteAllText(path + ".ready", "ready");
            UpdateReplacement.WaitForParent(plan);
            if (!UpdateReplacement.HashFile(plan.Payload).Equals(plan.Hash, StringComparison.OrdinalIgnoreCase))
                throw new IOException("The downloaded update changed. Nothing was installed.");
            if (plan.Installer)
            {
                var start = new ProcessStartInfo(plan.Payload) { UseShellExecute = true };
                start.ArgumentList.Add("/SILENT"); start.ArgumentList.Add("/NORESTART");
                start.ArgumentList.Add("/DIR=" + plan.Target.TrimEnd(Path.DirectorySeparatorChar));
                using var installer = Process.Start(start) ?? throw new IOException("Cannot start the installer.");
                installer.WaitForExit();
                if (installer.ExitCode != 0) throw new IOException("Installer did not complete successfully (exit " + installer.ExitCode + ").");
            }
            else
            {
                string source = Path.Combine(job, "Apply");
                ZipFile.ExtractToDirectory(plan.Payload, source);
                UpdateReplacement.ApplyPortable(source, plan.Target, Path.Combine(job, "Backup"));
            }
            File.WriteAllText(Path.Combine(job, "update-result.txt"), "Update completed successfully.");
            Process.Start(new ProcessStartInfo(Path.Combine(plan.Target, "ADB Connect.exe")) { UseShellExecute = true, WorkingDirectory = plan.Target });
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(job, "update-result.txt"), ex.ToString());
            AppDialog.Show(ex.Message + "\n\nDetails and any backup: " + job, "Update could not complete");
        }
    }
}

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace ADB_Connect;

internal sealed record UpdatePlan(int ParentId, long ParentStart, string Target, string Payload, bool Installer, string Hash);

internal static class UpdateReplacement
{
    internal static string HashFile(string path)
    { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }

    internal static string SafeChild(string root, string relative)
    {
        string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(Path.Combine(fullRoot, relative));
        if (Path.IsPathRooted(relative) || !full.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Update path escapes the application folder.");
        for (string? p = full; p != null; p = Path.GetDirectoryName(p))
        {
            if ((File.Exists(p) || Directory.Exists(p)) && (File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Updating through a symbolic link or junction is not supported.");
        }
        return full;
    }

    internal static void ApplyPortable(string source, string target, string backup)
    {
        source = Path.GetFullPath(source); target = Path.GetFullPath(target);
        if (!File.Exists(SafeChild(source, "ADB Connect.exe")) || !File.Exists(SafeChild(target, "ADB Connect.exe")))
            throw new IOException("The source or destination is not an ADB Connect application folder.");
        if (source.StartsWith(target.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith(source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Update source and destination must be separate.");
        // Reject links before traversing them.
        var options = new EnumerationOptions { RecurseSubdirectories = false, AttributesToSkip = 0 };
        var files = new List<string>();
        void Collect(string directory)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory, "*", options))
            {
                string relative = Path.GetRelativePath(source, entry);
                SafeChild(source, relative); SafeChild(target, relative);
                if (Directory.Exists(entry)) Collect(entry);
                else if (!File.Exists(SafeChild(target, relative)) || HashFile(entry) != HashFile(SafeChild(target, relative))) files.Add(relative);
            }
        }
        Collect(source);
        Directory.CreateDirectory(backup);
        var existed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relative in files)
        {
            string destination = SafeChild(target, relative);
            if (File.Exists(destination))
            {
                string saved = SafeChild(backup, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(saved)!);
                File.Copy(destination, saved, false);
                existed.Add(relative);
            }
        }
        var applied = new List<string>();
        try
        {
            foreach (var relative in files)
            {
                string destination = SafeChild(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".update";
                try
                {
                    File.Copy(SafeChild(source, relative), temporary);
                    // Same-volume rename prevents leaving a partly copied executable.
                    File.Move(temporary, destination, true);
                    applied.Add(relative);
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
        }
        catch (Exception original)
        {
            var errors = new List<Exception> { original };
            foreach (string relative in applied.AsEnumerable().Reverse())
            {
                try
                {
                    string destination = SafeChild(target, relative);
                    if (existed.Contains(relative)) File.Copy(SafeChild(backup, relative), destination, true);
                    else File.Delete(destination);
                }
                catch (Exception restore) { errors.Add(restore); }
            }
            if (errors.Count > 1) throw new AggregateException("Update failed; some files require recovery from " + backup, errors);
            throw new IOException("Update failed. Previous files were restored. Backup: " + backup, original);
        }
    }

    internal static void WaitForParent(UpdatePlan plan)
    {
        Process parent;
        try { parent = Process.GetProcessById(plan.ParentId); }
        catch (ArgumentException) { return; }
        using (parent)
        {
            if (parent.StartTime.ToUniversalTime().Ticks != plan.ParentStart) return;
            if (!parent.WaitForExit(60000)) throw new IOException("ADB Connect did not close. No update was applied.");
        }
    }
}

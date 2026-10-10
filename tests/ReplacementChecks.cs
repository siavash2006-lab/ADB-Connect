using ADB_Connect;

internal static class ReplacementChecks
{
    internal static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "adb-replacement-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string source = Path.Combine(root, "source"), target = Path.Combine(root, "target"), backup = Path.Combine(root, "backup");
            Directory.CreateDirectory(source); Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(source, "ADB Connect.exe"), "new");
            File.WriteAllText(Path.Combine(source, "new.dll"), "new dependency");
            File.WriteAllText(Path.Combine(target, "ADB Connect.exe"), "old");
            File.WriteAllText(Path.Combine(target, "user.log"), "keep me");
            UpdateReplacement.ApplyPortable(source, target, backup);
            if (File.ReadAllText(Path.Combine(target, "ADB Connect.exe")) != "new" ||
                File.ReadAllText(Path.Combine(backup, "ADB Connect.exe")) != "old" ||
                File.ReadAllText(Path.Combine(target, "user.log")) != "keep me") throw new Exception("Replacement/preservation failed");
            Console.WriteLine("PASS: portable replacement, backup and unrelated file preservation");
            using (var sameFile = new FileStream(Path.Combine(target,"ADB Connect.exe"),FileMode.Open,FileAccess.Read,FileShare.Read))
                UpdateReplacement.ApplyPortable(source,target,Path.Combine(root,"unchanged-backup"));
            Console.WriteLine("PASS: unchanged locked files are left intact");
            bool rejected = false;
            try { UpdateReplacement.SafeChild(target, "../outside"); } catch (IOException) { rejected = true; }
            if (!rejected) throw new Exception("Path traversal accepted");
            Console.WriteLine("PASS: update paths cannot escape destination");

            // Lock the last file so preceding changes must be rolled back.
            string source2 = Path.Combine(root,"source2"), target2 = Path.Combine(root,"target2");
            Directory.CreateDirectory(source2); Directory.CreateDirectory(target2);
            File.WriteAllText(Path.Combine(source2,"ADB Connect.exe"),"new");
            File.WriteAllText(Path.Combine(source2,"first.dll"),"added");
            File.WriteAllText(Path.Combine(source2,"locked.dll"),"new locked");
            File.WriteAllText(Path.Combine(target2,"ADB Connect.exe"),"old");
            File.WriteAllText(Path.Combine(target2,"locked.dll"),"old locked");
            using (var locked = new FileStream(Path.Combine(target2,"locked.dll"),FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                bool failed = false;
                try { UpdateReplacement.ApplyPortable(source2,target2,Path.Combine(root,"backup2")); } catch (IOException) { failed=true; }
                if (!failed || File.ReadAllText(Path.Combine(target2,"ADB Connect.exe")) != "old" ||
                    File.Exists(Path.Combine(target2,"first.dll"))) throw new Exception("Rollback failed");
            }
            Console.WriteLine("PASS: locked destination causes rollback without keeping newly added files");
        }
        finally { Directory.Delete(root, true); }
    }
}

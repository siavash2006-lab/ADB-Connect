using ADB_Connect;
using System.Diagnostics;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var box = new RichTextBox { ReadOnly = true, WordWrap = false };
        _ = box.Handle;
        var watch = Stopwatch.StartNew();
        for (int batch = 0; batch < 120; batch++)
        {
            string text = string.Concat(Enumerable.Range(batch * 500, 500).Select(i => $"line-{i}\n"));
            LogViewWriter.Append(box, text, Color.White, 2000, 300000);
            if (!box.ReadOnly) throw new Exception("ReadOnly was not restored.");
            if (box.TextLength > 300000) throw new Exception("Character limit was exceeded.");
        }
        if (box.Text.Contains("line-0\n") || !box.Text.Contains("line-59999")) throw new Exception("Log trimming failed.");
        if (box.GetLineFromCharIndex(box.TextLength) > 2000) throw new Exception("Line limit was exceeded.");
        box.Select(0, 20);
        LogViewWriter.Append(box, "scrcpy: Starting...\n", Color.MediumPurple, 2000, 300000);
        if (!box.Text.EndsWith("scrcpy: Starting...\n")) throw new Exception("Append replaced the user's selection.");
        if (box.CanUndo) throw new Exception("Undo history accumulated.");
        Console.WriteLine($"PASS: 60,000 lines appended/trimmed in {watch.ElapsedMilliseconds} ms; read-only restored, limits respected, selection safe, no undo growth.");
        Console.WriteLine("No Android commands were issued. Audible behavior still requires confirmation on the user's machine.");
    }
}

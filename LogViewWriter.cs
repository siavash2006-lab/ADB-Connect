namespace ADB_Connect;

internal static class LogViewWriter
{
    // RichEdit can reject formatting/replacement in read-only mode with a system
    // notification. This synchronous scope never yields to user input.
    public static void Append(RichTextBox view, string text, Color color, int lineLimit, int characterLimit)
    {
        bool readOnly = view.ReadOnly;
        try
        {
            view.ReadOnly = false;
            view.Select(view.TextLength, 0);
            view.SelectionColor = color;
            view.AppendText(text);
            int lines = view.GetLineFromCharIndex(view.TextLength);
            int cut = lines > lineLimit ? view.GetFirstCharIndexFromLine(lines - lineLimit) : 0;
            cut = Math.Max(cut, Math.Max(0, view.TextLength - characterLimit));
            if (cut > 0)
            {
                view.Select(0, cut);
                view.SelectedText = "";
            }
            view.Select(view.TextLength, 0);
            view.SelectionColor = view.ForeColor;
            view.ClearUndo();
            if (view.Visible) view.ScrollToCaret();
        }
        finally { view.ReadOnly = readOnly; }
    }
}

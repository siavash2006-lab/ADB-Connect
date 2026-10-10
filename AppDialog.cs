namespace ADB_Connect;

// Compact plain-text messages; long output scrolls without an editor border.
internal sealed class AppDialog : Form
{
    internal AppDialog(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        Text = string.IsNullOrWhiteSpace(caption) ? "ADB Connect" : caption;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        const int width = 420;
        var measured = TextRenderer.MeasureText(text, Font, new Size(width - 52, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        int contentHeight = Math.Clamp(measured.Height + 8, 34, 260);
        ClientSize = new Size(width, contentHeight + 80);
        var body = new Panel { Bounds = new Rectangle(16, 16, width - 32, contentHeight), AutoScroll = true, Tag = "workspace" };
        var content = new Label
        {
            Text = text, UseMnemonic = false, AutoSize = false,
            Bounds = new Rectangle(0, 0, width - 52, Math.Max(contentHeight, measured.Height + 4)),
            AccessibleName = "Message"
        };
        body.Controls.Add(content);
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8),
            FlowDirection = FlowDirection.RightToLeft
        };
        Button Add(string label, DialogResult result)
        {
            var button = new Button { Text = label, DialogResult = result, Size = new Size(84, 28), Tag = result == DialogResult.OK ? "primary" : null };
            footer.Controls.Add(button); return button;
        }
        if (buttons == MessageBoxButtons.YesNo)
        {
            var no = Add("No", DialogResult.No); Add("Yes", DialogResult.Yes);
            AcceptButton = CancelButton = no;
            FormClosing += (_, _) => { if (DialogResult == DialogResult.Cancel) DialogResult = DialogResult.No; };
        }
        else if (buttons == MessageBoxButtons.OKCancel)
        { CancelButton = Add("Cancel", DialogResult.Cancel); AcceptButton = Add("OK", DialogResult.OK); }
        else { var ok = Add("OK", DialogResult.OK); AcceptButton = CancelButton = ok; }
        Controls.Add(body); Controls.Add(footer);
        // Keep long diagnostic messages copyable without presenting an editable text field.
        var menu = new ContextMenuStrip();
        menu.Items.Add("Copy message", null, (_, _) =>
        {
            try { Clipboard.SetText(text); }
            catch (System.Runtime.InteropServices.ExternalException) { }
        });
        content.ContextMenuStrip = menu;
        Disposed += (_, _) => menu.Dispose();
        AppTheme.Attach(this);
    }
    public static DialogResult Show(string text, string caption = "ADB Connect", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        => Show(Form.ActiveForm, text, caption, buttons, icon);
    public static DialogResult Show(IWin32Window? owner, string text, string caption = "ADB Connect", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        using var dialog = new AppDialog(text, caption, buttons, icon);
        return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
    }
}

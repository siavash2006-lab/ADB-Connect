using System.Drawing.Drawing2D;

namespace ADB_Connect;

internal sealed class ThemeIconButton : Button
{
    public ThemeIconButton()
    {
        AccessibleName = "Toggle light / dark theme";
        AccessibleRole = AccessibleRole.CheckButton;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Click += (_, _) => AppTheme.SetMode(AppTheme.Dark ? ThemeMode.Light : ThemeMode.Dark);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Color fill = AppTheme.SelectedSurface;
        e.Graphics.Clear(fill);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float scale = DeviceDpi / 96f;
        e.Graphics.TranslateTransform(Width / 2f, Height / 2f);
        e.Graphics.ScaleTransform(scale, scale);
        using var pen = new Pen(AppTheme.Text, 1.5f);
        if (!AppTheme.Dark)
        {
            e.Graphics.DrawEllipse(pen, -4, -4, 8, 8);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;
                e.Graphics.DrawLine(pen, (float)Math.Cos(a)*7, (float)Math.Sin(a)*7, (float)Math.Cos(a)*10, (float)Math.Sin(a)*10);
            }
        }
        else
        {
            using var moon = new SolidBrush(AppTheme.Text);
            e.Graphics.FillEllipse(moon, -8, -9, 17, 17);
            using var cutout = new SolidBrush(fill);
            e.Graphics.FillEllipse(cutout, -2, -11, 15, 15);
        }
        e.Graphics.ResetTransform();
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -3, -3), AppTheme.Text, fill);
    }
    protected override AccessibleObject CreateAccessibilityInstance() => new ThemeButtonAccessibility(this);
    private sealed class ThemeButtonAccessibility(ThemeIconButton owner) : ControlAccessibleObject(owner)
    {
        public override string DefaultAction => AppTheme.Dark ? "Switch to light theme" : "Switch to dark theme";
        public override void DoDefaultAction() => owner.PerformClick();
        public override AccessibleStates State => base.State | (AppTheme.Dark ? AccessibleStates.Checked : AccessibleStates.None);
    }
}

// Keep native tab navigation and selection, but paint without the raised Windows frame.
internal sealed class SoftTabControl : TabControl
{
    public SoftTabControl() => SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(AppTheme.Background);
        using var fill = new SolidBrush(AppTheme.Surface);
        e.Graphics.FillRectangle(fill, DisplayRectangle);
        for (int i=0; i<TabCount; i++)
            OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, GetTabRect(i), i, i == SelectedIndex ? DrawItemState.Selected : DrawItemState.None));
    }
    protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }
}

// Preserve the native dropdown, keyboard behaviour and accessibility; replace only its closed face.
internal sealed class SoftComboBox : ComboBox
{
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (DropDownStyle != ComboBoxStyle.DropDownList) return;
        if (m.Msg == 0xF) { using var graphics = CreateGraphics(); PaintFace(graphics); }
        else if ((m.Msg == 0x317 || m.Msg == 0x318) && m.WParam != IntPtr.Zero)
        { using var graphics = Graphics.FromHdc(m.WParam); PaintFace(graphics); }
    }
    private void PaintFace(Graphics graphics)
    {
        using var background = new SolidBrush(AppTheme.Surface);
        graphics.FillRectangle(background, ClientRectangle);
        using var border = new Pen(AppTheme.SoftBorder);
        graphics.DrawRectangle(border, 0, 0, Width-1, Height-1);
        Color ink = Enabled ? AppTheme.Text : AppTheme.MutedText;
        TextRenderer.DrawText(graphics, Text, Font, new Rectangle(8, 0, Math.Max(0,Width-34), Height), ink,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        using var arrow = new Pen(ink, 1.2f);
        int x=Width-14, y=Height/2;
        graphics.DrawLines(arrow, new[]{new Point(x-4,y-2),new Point(x,y+2),new Point(x+4,y-2)});
        if (Focused && !DroppedDown) ControlPaint.DrawFocusRectangle(graphics, new Rectangle(3,3,Width-26,Height-6),ink,AppTheme.Surface);
    }
    protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
}

using Microsoft.Win32;
using System.Runtime.InteropServices;
namespace ADB_Connect;
internal enum ThemeMode { System, Light, Dark }
internal static class AppTheme
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ADB Connect", "theme.txt");
    public static ThemeMode Mode { get; private set; } = Load();
    public static event Action? Changed;
    private static bool _systemDark = SystemDark();
    public static bool Dark => Mode == ThemeMode.Dark || Mode == ThemeMode.System && _systemDark;
    public static Color Background => Dark ? Color.FromArgb(20,27,35) : Color.FromArgb(239,243,247);
    public static Color Surface => Dark ? Color.FromArgb(27,36,46) : Color.FromArgb(232,238,243);
    public static Color SelectedSurface => Dark ? Color.FromArgb(35,66,72) : Color.FromArgb(205,226,230);
    public static Color SoftBorder => Dark ? Color.FromArgb(43,55,67) : Color.FromArgb(213,222,230);
    public static Color MutedText => Dark ? Color.FromArgb(137,155,171) : Color.FromArgb(110,125,140);
    public static Color Text => Dark ? Color.FromArgb(224,233,240) : Color.FromArgb(28,44,60);
    public static Color Border => Dark ? Color.FromArgb(64,80,95) : Color.FromArgb(204,216,227);
    public static Color Accent => Color.FromArgb(0,115,121);
    private static ThemeMode Load()
    {
        try { if (Enum.TryParse<ThemeMode>(File.ReadAllText(SettingsPath).Trim(), out var mode) && Enum.IsDefined(mode)) return mode; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        return ThemeMode.System;
    }
    private static bool SystemDark()
    {
        try { return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int value && value == 0; }
        catch (Exception ex) when (ex is System.Security.SecurityException or IOException or UnauthorizedAccessException) { return false; }
    }
    public static void SetMode(ThemeMode mode, bool persist = true)
    {
        Mode = mode;
        if (mode == ThemeMode.System) _systemDark = SystemDark();
        if (persist) try { Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!); File.WriteAllText(SettingsPath, mode.ToString()); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Trace.WriteLine(ex.Message); }
        Changed?.Invoke();
    }
    public static void Attach(Form form, Action? after = null)
    {
        void Refresh()
        {
            if (form.IsDisposed || form.Disposing) return;
            if (form.InvokeRequired) { if (form.IsHandleCreated) try { form.BeginInvoke((Action)Refresh); } catch (InvalidOperationException) { } return; }
            if (Mode == ThemeMode.System) _systemDark = SystemDark();
            Apply(form); after?.Invoke(); form.Invalidate(true);
        }
        void SystemChanged(object sender, UserPreferenceChangedEventArgs e)
        { if (Mode == ThemeMode.System && form.IsHandleCreated) try { form.BeginInvoke((Action)Refresh); } catch (InvalidOperationException) { } }
        Changed += Refresh; SystemEvents.UserPreferenceChanged += SystemChanged;
        form.HandleCreated += (_, _) => Refresh();
        form.Disposed += (_, _) => { Changed -= Refresh; SystemEvents.UserPreferenceChanged -= SystemChanged; };
        foreach (var button in Descendants(form).OfType<Button>())
        {
            button.EnabledChanged += (_, _) => StyleButton(button);
            button.Paint += (_, e) =>
            {
                if (button.Enabled) return;
                using var fill = new SolidBrush(Background);
                e.Graphics.FillRectangle(fill, button.ClientRectangle);
                using var edge = new Pen(Border);
                e.Graphics.DrawRectangle(edge, 0, 0, button.Width-1, button.Height-1);
                TextRenderer.DrawText(e.Graphics, button.Text, button.Font, button.ClientRectangle,
                    Dark ? Color.FromArgb(137,155,171) : Color.FromArgb(110,125,140),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            };
        }
        foreach (var combo in Descendants(form).OfType<ComboBox>())
        {
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.DrawItem += (_, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) != 0;
                using var fill = new SolidBrush(selected ? Accent : Surface);
                e.Graphics.FillRectangle(fill, e.Bounds);
                TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), e.Font, e.Bounds,
                    selected ? Color.White : Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                e.DrawFocusRectangle();
            };
        }
        Refresh();
    }
    public static IEnumerable<Control> Descendants(Control parent)
    { foreach (Control child in parent.Controls) { yield return child; foreach (var nested in Descendants(child)) yield return nested; } }
    public static void Apply(Control root)
    {
        root.BackColor = root is Form ? Background : Surface; root.ForeColor = Text;
        foreach (Control child in root.Controls) Apply(child);
        if (Equals(root.Tag,"video")) root.BackColor = Color.Black;
        if (Equals(root.Tag,"workspace")) root.BackColor = Background;
        if (root is Label or CheckBox) root.BackColor = Color.Transparent;
        if (root is TabPage page) page.UseVisualStyleBackColor = false;
        if (root is Button button) StyleButton(button);
        if (root is TextBoxBase text) { text.BackColor = Dark ? Background : Color.White; text.ForeColor = Text; text.BorderStyle = BorderStyle.FixedSingle; }
        if (root is ComboBox combo) combo.FlatStyle = FlatStyle.Flat;
        if (root is LinkLabel link) link.LinkColor = Dark ? Color.Turquoise : Accent;
        if (root is ToolStrip strip)
        { strip.Renderer = new ToolStripProfessionalRenderer(new ThemeColors()); foreach (ToolStripItem item in strip.Items) { item.ForeColor = Text; item.BackColor = Surface; } }
        if (root is Form form && form.IsHandleCreated) { int dark = Dark ? 1 : 0; _ = DwmSetWindowAttribute(form.Handle,20,ref dark,sizeof(int)); }
    }
    private static void StyleButton(Button button)
    {
        button.UseVisualStyleBackColor=false; button.FlatStyle=FlatStyle.Flat;
        button.FlatAppearance.BorderSize=1; button.FlatAppearance.BorderColor=Border;
        button.BackColor=Surface; button.ForeColor=Text;
        if (Equals(button.Tag,"primary") && button.Enabled) { button.BackColor=Accent; button.ForeColor=Color.White; }
        if (Equals(button.Tag,"danger") && button.Enabled) button.ForeColor=Dark?Color.Salmon:Color.Firebrick;
        if (!button.Enabled) button.BackColor=Background;
        button.FlatAppearance.MouseOverBackColor=Dark?Color.FromArgb(46,74,84):Color.FromArgb(207,231,234);
        button.FlatAppearance.MouseDownBackColor=Accent;
    }
    private sealed class ThemeColors : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin=>Surface;
        public override Color ToolStripGradientMiddle=>Surface;
        public override Color ToolStripGradientEnd=>Surface;
        public override Color ButtonSelectedHighlight=>Border;
        public override Color ButtonSelectedGradientBegin=>Border;
        public override Color ButtonSelectedGradientEnd=>Border;
        public override Color ToolStripBorder=>Border;
    }
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd,int attribute,ref int value,int size);
}

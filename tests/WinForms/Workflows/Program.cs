using System.Reflection;
using ADB_Connect;

internal static class Program
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static T Field<T>(object obj, string name) => (T)obj.GetType().GetField(name, Flags)!.GetValue(obj)!;
    static void Advance(PairDeviceForm form) => typeof(PairDeviceForm).GetMethod("Advance", Flags)!.Invoke(form, null);
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS: " + name); }
    static IEnumerable<Control> Walk(Control parent) { foreach (Control c in parent.Controls) { yield return c; foreach (var x in Walk(c)) yield return x; } }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr w, IntPtr l);
    [STAThread] static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var pair = new PairDeviceForm("192.168.1.10");
        Advance(pair);
        Check(!Field<bool>(pair, "_secondStep") && Field<Label>(pair, "_error").Text.Length > 0, "connection port required before next step");
        Field<TextBox>(pair, "_txtConnectPort").Text = "5555";
        Advance(pair);
        Check(Field<bool>(pair, "_secondStep") && pair.DialogResult != DialogResult.OK, "next shows pairing without submitting");
        Field<TextBox>(pair, "_txtPairPort").Text = "37123";
        Field<TextBox>(pair, "_txtPairCode").Text = "123";
        Advance(pair);
        Check(pair.DialogResult != DialogResult.OK, "incomplete pairing code rejected");
        typeof(PairDeviceForm).GetMethod("ShowStep", Flags)!.Invoke(pair, new object[] { false });
        Check(pair.ConnectionPort == 5555 && pair.PairingPort == 37123, "back preserves entered values");
        Advance(pair);
        Field<TextBox>(pair, "_txtPairCode").Text = "123456";
        Advance(pair);
        Check(pair.DialogResult == DialogResult.OK && pair.ConnectionEndpoint == "192.168.1.10:5555" && pair.PairingEndpoint == "192.168.1.10:37123", "separate endpoints submitted after validation");
        using var ipv6 = new PairDeviceForm("::1");
        Field<TextBox>(ipv6, "_txtConnectPort").Text = "5555";
        Check(ipv6.ConnectionEndpoint == "[::1]:5555", "IPv6 endpoint brackets preserved");

        var dialogType = typeof(Form1).Assembly.GetType("ADB_Connect.AppDialog")!;
        foreach (string message in new[] { "Timeout: adb did not finish.", string.Join("\n", Enumerable.Repeat("Long diagnostic output with wrapping text and package details.", 40)) })
        {
            using var dialog = (Form)Activator.CreateInstance(dialogType, Flags, null, new object[] { message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error }, null)!;
            Check(!Walk(dialog).Any(c => c is TextBoxBase), "message has no text editor");
            Check(Walk(dialog).OfType<Label>().Any(l => l.Text == message), "full message retained");
            Check(dialog.ClientSize.Height <= 340 && (message.Length > 100 || dialog.ClientSize.Height < 150), "message height is bounded and short text compact");
        }
        using var comboHost = new Form();
        using var combo = (ComboBox)Activator.CreateInstance(typeof(Form1).Assembly.GetType("ADB_Connect.SoftComboBox")!, true)!;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.AddRange(new object[] { "device-one", "device-two" });
        combo.SelectedIndex = 0; comboHost.Controls.Add(combo); comboHost.Show(); Application.DoEvents();
        combo.Focus();
        SendMessage(combo.Handle, 0x100, (IntPtr)0x28, IntPtr.Zero);
        Check(combo.SelectedIndex == 1, "custom dropdown retains keyboard selection");
        combo.DroppedDown = true; Application.DoEvents();
        Check(combo.DroppedDown, "custom dropdown opens native list");
        combo.DroppedDown = false;
        var getStyle = typeof(Control).GetMethod("GetStyle", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Check((bool)getStyle.Invoke(combo, new object[] { ControlStyles.UserPaint })!, "dropdown face uses managed paint without native paint pass");
        using var update = (Button)Activator.CreateInstance(typeof(Form1).Assembly.GetType("ADB_Connect.UpdateIconButton")!, true)!;
        var badge = update.GetType().GetProperty("HasUpdate")!;
        Check(!(bool)badge.GetValue(update)!, "update badge initially hidden");
        badge.SetValue(update, true);
        Check((bool)badge.GetValue(update)!, "update badge can signal available release");
        Console.WriteLine("Workflow checks passed. No device or installer was invoked.");
    }
}

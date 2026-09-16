namespace ADB_Connect;
public partial class Form1
{
    private Label? _connectionBadge;
    private void InitializeIndustrialTheme()
    {
        SuspendLayout(); panel4.SuspendLayout();
        ClientSize=new Size(620,690); Text="ADB Connect | Device Workbench";
        panel4.SetBounds(12,86,596,592); panel4.BorderStyle=BorderStyle.None; panel4.Tag="workspace";
        foreach(var control in AllControls(panel4))
        {
            if(control is Panel panel) panel.BorderStyle=BorderStyle.None;
            if(control is Button button) {button.Font=Font; button.Cursor=Cursors.Hand;}
        }
        label1.SetBounds(0,8,34,20); txtIp.SetBounds(34,4,184,26);
        btnConnect.SetBounds(226,0,110,30); btnDisconnect.SetBounds(344,0,110,30); btnCheckDevice.SetBounds(462,0,134,30);
        panel1.SetBounds(0,40,596,62); label2.Location=new Point(8,4); label2.Text="POWER / RECOVERY";
        var power=new[]{btnReboot,btnRecovery,btnBootloader,btnFastboot};
        for(int i=0;i<power.Length;i++) power[i].SetBounds(8+i*146,26,140,28);
        panelDeviceSelector.SetBounds(0,110,596,38);
        label8.Location=new Point(8,11); comboBox1.SetBounds(112,8,336,24); btnRefreshScrcpyDevices.SetBounds(456,5,132,28);
        tabControl1.SetBounds(0,158,596,390); tabControl1.DrawMode=TabDrawMode.OwnerDrawFixed;
        tabControl1.SizeMode=TabSizeMode.Fixed; tabControl1.ItemSize=new Size(146,32);
        tabControl1.DrawItem+=(_,e)=>
        {
            bool selected=e.Index==tabControl1.SelectedIndex;
            using var brush=new SolidBrush(selected?AppTheme.SelectedSurface:AppTheme.Background);
            e.Graphics.FillRectangle(brush,e.Bounds);
            TextRenderer.DrawText(e.Graphics,tabControl1.TabPages[e.Index].Text,e.Font,e.Bounds,AppTheme.Text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
            if(selected) {using var accent=new SolidBrush(AppTheme.Accent); e.Graphics.FillRectangle(accent,e.Bounds.X+8,e.Bounds.Bottom-2,e.Bounds.Width-16,2);}
            if(selected && tabControl1.Focused) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(e.Bounds,-4,-4));
        };
        tabPage2.Text="App Management"; tabPage3.Text="Device Logs";
        btnVendorFingerprint.Text="Vendor Fingerprint"; btnSkdVersion.Text="SDK Version"; btnSerialNo.Text="Serial Number";
        btnGetEnforce.Text="SELinux Mode"; btnPackageList.Text="Refresh Apps"; ckbSystemApps.Text="System"; ckbUserApps.Text="Third-party";
        var props=tabPage1.Controls.OfType<Button>().Where(b=>b!=btnGetProps).OrderBy(b=>b.Top).ThenBy(b=>b.Left).ToArray();
        for(int i=0;i<props.Length;i++) props[i].SetBounds(8+i%4*144,10+i/4*28,138,24);
        btnGetProps.Text="Read All Properties"; btnGetProps.SetBounds(8,210,374,28);
        InitializePropertyResults();
        var appButtons=new[]{btnInstallApk,btnUninstall,btnPackageList,btnStartApp,btnStopApp};
        for(int i=0;i<appButtons.Length;i++) appButtons[i].SetBounds(8+i*115,10,109,28);
        ckbAllApp.Location=new Point(8,46); ckbSystemApps.Location=new Point(82,46); ckbUserApps.Location=new Point(174,46);
        txbSearchPackages.SetBounds(8,72,570,25); txbSearchPackages.PlaceholderText="Search package name…";
        checkedListBox1.SetBounds(8,104,570,194); checkedListBox1.IntegralHeight=false; pbLoading.SetBounds(548,45,24,24);
        btnStartLog.SetBounds(8,10,110,28); btnStopLog.SetBounds(126,10,110,28); label3.Location=new Point(326,16); txtLineLimitation.SetBounds(444,13,134,24);
        label3.Text="Line limit:"; label4.Text="Keyword:"; label5.Text="Package:";
        label4.Location=new Point(8,51); txtFilter.SetBounds(74,47,206,24); label5.Location=new Point(292,51); txtPackageFilter.SetBounds(362,47,216,24);
        rtbLog.SetBounds(8,80,570,177); rtbLog.Font=new Font("Consolas",9F);
        var clearDevice=tabPage3.Controls.OfType<Button>().Single(b=>b.Text=="Clear Device...");
        var logButtons=new[]{btnExportLog,btnBugreport,clearDevice,btnClearLog};
        for(int i=0;i<logButtons.Length;i++) logButtons[i].SetBounds(8+i*144,265,138,28);
        label9.Location=new Point(12,18); comboBox2.SetBounds(112,14,262,24); checkBox1.Location=new Point(394,16);
        button1.SetBounds(112,48,262,30); label10.Location=new Point(12,98); label11.Location=new Point(112,98);
        label11.AutoSize=false; label11.Size=new Size(458,42); lblScrcpyHint.SetBounds(12,152,558,60);
        label6.Location=new Point(0,566); label6.Text="Version 1.6.6"; linkSpadra.Location=new Point(548,566);
        btnCancelOperation.SetBounds(210,557,176,28);
        foreach(var b in new[]{btnConnect,btnInstallApk,btnStartLog,button1,btnGetProps}) b.Tag="primary";
        btnUninstall.Tag="danger"; clearDevice.Tag="danger";
        var header=new Panel{Bounds=new Rectangle(0,0,620,72)};
        header.Controls.Add(new Label{Text="ADB CONNECT",Font=new Font("Segoe UI Semibold",17F),AutoSize=true,Location=new Point(12,6)});
        _connectionBadge=new Label{Bounds=new Rectangle(14,42,200,22)}; header.Controls.Add(_connectionBadge);
        components ??= new System.ComponentModel.Container();
        var tips=new ToolTip(components);
        var icon=new ThemeIconButton {Name="themeToggle",Bounds=new Rectangle(572,22,36,32),TabIndex=0};
        header.Controls.Add(icon);
        Controls.Add(header);
        AppTheme.Attach(this,()=>
        {
            UpdateConnectionBadge(_operationCts!=null);
            string description=AppTheme.Dark ? "Dark theme — switch to light" : "Light theme — switch to dark";
            tips.SetToolTip(icon,description);
            icon.AccessibleDescription=description;
        });
        panel4.ResumeLayout(false); ResumeLayout(false);
    }
    private void UpdateConnectionBadge(bool busy)
    {
        if(_connectionBadge==null)return;
        _connectionBadge.Text=busy?"● WORKING":_isConnected?"● CONNECTED":"○ DISCONNECTED";
        _connectionBadge.ForeColor=busy?(AppTheme.Dark?Color.Gold:Color.SaddleBrown):_isConnected?(AppTheme.Dark?Color.Turquoise:AppTheme.Accent):AppTheme.Text;
    }
}

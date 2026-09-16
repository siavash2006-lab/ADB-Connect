namespace ADB_Connect
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel4 = new Panel();
            linkSpadra = new LinkLabel();
            label6 = new Label();
            panelDeviceSelector = new Panel();
            tabControl1 = new SoftTabControl();
            tabPage1 = new TabPage();
            btnGetProps = new Button();
            btnGetEnforce = new Button();
            btnArm = new Button();
            btnTimeZone = new Button();
            btnVbmeta = new Button();
            btnBuildFingerprint = new Button();
            btnClientIdBase = new Button();
            btnHardware = new Button();
            btnValidation = new Button();
            button3 = new Button();
            btnOemKey = new Button();
            btnCpuType = new Button();
            btnBoardName = new Button();
            btnDevice = new Button();
            btnManufacturer = new Button();
            btnVendorBrand = new Button();
            btnVendorModel = new Button();
            btnVendorName = new Button();
            btnSerialNo = new Button();
            btnBuildId = new Button();
            button2 = new Button();
            btnSkdVersion = new Button();
            btnVendorBuildDate = new Button();
            btnVendorFingerprint = new Button();
            btnDisplaySize = new Button();
            btnKernelTest = new Button();
            btnVersion = new Button();
            tabPage2 = new TabPage();
            pbLoading = new PictureBox();
            btnStopApp = new Button();
            btnStartApp = new Button();
            txbSearchPackages = new TextBox();
            ckbUserApps = new CheckBox();
            ckbSystemApps = new CheckBox();
            ckbAllApp = new CheckBox();
            btnUninstall = new Button();
            checkedListBox1 = new CheckedListBox();
            btnPackageList = new Button();
            btnInstallApk = new Button();
            tabPage3 = new TabPage();
            btnBugreport = new Button();
            btnClearLog = new Button();
            btnExportLog = new Button();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            txtPackageFilter = new TextBox();
            txtFilter = new TextBox();
            txtLineLimitation = new TextBox();
            btnStopLog = new Button();
            btnStartLog = new Button();
            rtbLog = new RichTextBox();
            btnCheckDevice = new Button();
            panel1 = new Panel();
            btnFastboot = new Button();
            btnBootloader = new Button();
            btnRecovery = new Button();
            label2 = new Label();
            btnReboot = new Button();
            btnDisconnect = new Button();
            label1 = new Label();
            txtIp = new TextBox();
            btnConnect = new Button();
            tabPage4 = new TabPage();
            button1 = new Button();
            btnRefreshScrcpyDevices = new Button();
            comboBox1 = new SoftComboBox();
            label8 = new Label();
            label9 = new Label();
            comboBox2 = new SoftComboBox();
            label10 = new Label();
            label11 = new Label();
            checkBox1 = new CheckBox();
            lblScrcpyHint = new Label();
            panel4.SuspendLayout();
            panelDeviceSelector.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLoading).BeginInit();
            tabPage3.SuspendLayout();
            panel1.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(linkSpadra);
            panel4.Controls.Add(label6);
            panel4.Controls.Add(panelDeviceSelector);
            panel4.Controls.Add(tabControl1);
            panel4.Controls.Add(btnCheckDevice);
            panel4.Controls.Add(panel1);
            panel4.Controls.Add(btnDisconnect);
            panel4.Controls.Add(label1);
            panel4.Controls.Add(txtIp);
            panel4.Controls.Add(btnConnect);
            panel4.Location = new Point(12, 13);
            panel4.Name = "panel4";
            panel4.Size = new Size(416, 465);
            panel4.TabIndex = 9;
            // 
            // linkSpadra
            //
            linkSpadra.ActiveLinkColor = SystemColors.Highlight;
            linkSpadra.AutoSize = true;
            linkSpadra.Cursor = Cursors.Hand;
            linkSpadra.LinkBehavior = LinkBehavior.HoverUnderline;
            linkSpadra.LinkColor = SystemColors.ControlDarkDark;
            linkSpadra.Location = new Point(354, 440);
            linkSpadra.Name = "linkSpadra";
            linkSpadra.Size = new Size(43, 15);
            linkSpadra.TabIndex = 18;
            linkSpadra.TabStop = true;
            linkSpadra.Text = "Spadra";
            linkSpadra.VisitedLinkColor = SystemColors.ControlDarkDark;
            linkSpadra.LinkClicked += linkSpadra_LinkClicked;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = SystemColors.ControlDark;
            label6.Location = new Point(14, 440);
            label6.Name = "label6";
            label6.Size = new Size(75, 15);
            label6.TabIndex = 18;
            label6.Text = "Version: 1.6.6";
            // 
            // 
            // panelDeviceSelector
            // 
            panelDeviceSelector.BorderStyle = BorderStyle.FixedSingle;
            panelDeviceSelector.Controls.Add(btnRefreshScrcpyDevices);
            panelDeviceSelector.Controls.Add(comboBox1);
            panelDeviceSelector.Controls.Add(label8);
            panelDeviceSelector.Location = new Point(19, 102);
            panelDeviceSelector.Name = "panelDeviceSelector";
            panelDeviceSelector.Size = new Size(382, 39);
            panelDeviceSelector.TabIndex = 19;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(14, 148);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(387, 286);
            tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(btnGetProps);
            tabPage1.Controls.Add(btnGetEnforce);
            tabPage1.Controls.Add(btnArm);
            tabPage1.Controls.Add(btnTimeZone);
            tabPage1.Controls.Add(btnVbmeta);
            tabPage1.Controls.Add(btnBuildFingerprint);
            tabPage1.Controls.Add(btnClientIdBase);
            tabPage1.Controls.Add(btnHardware);
            tabPage1.Controls.Add(btnValidation);
            tabPage1.Controls.Add(button3);
            tabPage1.Controls.Add(btnOemKey);
            tabPage1.Controls.Add(btnCpuType);
            tabPage1.Controls.Add(btnBoardName);
            tabPage1.Controls.Add(btnDevice);
            tabPage1.Controls.Add(btnManufacturer);
            tabPage1.Controls.Add(btnVendorBrand);
            tabPage1.Controls.Add(btnVendorModel);
            tabPage1.Controls.Add(btnVendorName);
            tabPage1.Controls.Add(btnSerialNo);
            tabPage1.Controls.Add(btnBuildId);
            tabPage1.Controls.Add(button2);
            tabPage1.Controls.Add(btnSkdVersion);
            tabPage1.Controls.Add(btnVendorBuildDate);
            tabPage1.Controls.Add(btnVendorFingerprint);
            tabPage1.Controls.Add(btnDisplaySize);
            tabPage1.Controls.Add(btnKernelTest);
            tabPage1.Controls.Add(btnVersion);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(379, 258);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Device Properties";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnGetProps
            // 
            btnGetProps.Font = new Font("Segoe UI", 12F);
            btnGetProps.Location = new Point(112, 212);
            btnGetProps.Name = "btnGetProps";
            btnGetProps.Size = new Size(148, 39);
            btnGetProps.TabIndex = 59;
            btnGetProps.Text = "Get All Props";
            btnGetProps.UseVisualStyleBackColor = true;
            btnGetProps.Click += btnGetProps_Click;
            // 
            // btnGetEnforce
            // 
            btnGetEnforce.Location = new Point(280, 183);
            btnGetEnforce.Name = "btnGetEnforce";
            btnGetEnforce.Size = new Size(83, 23);
            btnGetEnforce.TabIndex = 58;
            btnGetEnforce.Text = "Get Enforce";
            btnGetEnforce.UseVisualStyleBackColor = true;
            btnGetEnforce.Click += btnGetEnforce_Click;
            // 
            // btnArm
            // 
            btnArm.Location = new Point(182, 183);
            btnArm.Name = "btnArm";
            btnArm.Size = new Size(83, 23);
            btnArm.TabIndex = 57;
            btnArm.Text = "ARM Variant";
            btnArm.UseVisualStyleBackColor = true;
            btnArm.Click += btnArm_Click;
            // 
            // btnTimeZone
            // 
            btnTimeZone.Location = new Point(93, 183);
            btnTimeZone.Name = "btnTimeZone";
            btnTimeZone.Size = new Size(75, 23);
            btnTimeZone.TabIndex = 56;
            btnTimeZone.Text = "Time Zone";
            btnTimeZone.UseVisualStyleBackColor = true;
            btnTimeZone.Click += btnTimeZone_Click;
            // 
            // btnVbmeta
            // 
            btnVbmeta.Location = new Point(4, 183);
            btnVbmeta.Name = "btnVbmeta";
            btnVbmeta.Size = new Size(75, 23);
            btnVbmeta.TabIndex = 55;
            btnVbmeta.Text = "VB Meta";
            btnVbmeta.UseVisualStyleBackColor = true;
            btnVbmeta.Click += btnVbmeta_Click;
            // 
            // btnBuildFingerprint
            // 
            btnBuildFingerprint.Location = new Point(260, 154);
            btnBuildFingerprint.Name = "btnBuildFingerprint";
            btnBuildFingerprint.Size = new Size(105, 23);
            btnBuildFingerprint.TabIndex = 54;
            btnBuildFingerprint.Text = "Build Fingerprint";
            btnBuildFingerprint.UseVisualStyleBackColor = true;
            btnBuildFingerprint.Click += btnBuildFingerprint_Click;
            // 
            // btnClientIdBase
            // 
            btnClientIdBase.Location = new Point(162, 154);
            btnClientIdBase.Name = "btnClientIdBase";
            btnClientIdBase.Size = new Size(94, 23);
            btnClientIdBase.TabIndex = 53;
            btnClientIdBase.Text = "Client ID Base";
            btnClientIdBase.UseVisualStyleBackColor = true;
            btnClientIdBase.Click += btnClientIdBase_Click;
            // 
            // btnHardware
            // 
            btnHardware.Location = new Point(83, 154);
            btnHardware.Name = "btnHardware";
            btnHardware.Size = new Size(75, 23);
            btnHardware.TabIndex = 52;
            btnHardware.Text = "Hardware";
            btnHardware.UseVisualStyleBackColor = true;
            btnHardware.Click += btnHardware_Click;
            // 
            // btnValidation
            // 
            btnValidation.Location = new Point(4, 154);
            btnValidation.Name = "btnValidation";
            btnValidation.Size = new Size(75, 23);
            btnValidation.TabIndex = 51;
            btnValidation.Text = "Validation";
            btnValidation.UseVisualStyleBackColor = true;
            btnValidation.Click += btnValidation_Click;
            // 
            // button3
            // 
            button3.Location = new Point(261, 125);
            button3.Name = "button3";
            button3.Size = new Size(103, 23);
            button3.TabIndex = 50;
            button3.Text = "OpenGL Version";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // btnOemKey
            // 
            btnOemKey.Location = new Point(174, 125);
            btnOemKey.Name = "btnOemKey";
            btnOemKey.Size = new Size(75, 23);
            btnOemKey.TabIndex = 49;
            btnOemKey.Text = "OEM Key";
            btnOemKey.UseVisualStyleBackColor = true;
            btnOemKey.Click += btnOemKey_Click;
            // 
            // btnCpuType
            // 
            btnCpuType.Location = new Point(89, 125);
            btnCpuType.Name = "btnCpuType";
            btnCpuType.Size = new Size(75, 23);
            btnCpuType.TabIndex = 48;
            btnCpuType.Text = "CPU Type";
            btnCpuType.UseVisualStyleBackColor = true;
            btnCpuType.Click += btnCpuType_Click;
            // 
            // btnBoardName
            // 
            btnBoardName.Location = new Point(4, 125);
            btnBoardName.Name = "btnBoardName";
            btnBoardName.Size = new Size(75, 23);
            btnBoardName.TabIndex = 47;
            btnBoardName.Text = "Board Name";
            btnBoardName.UseVisualStyleBackColor = true;
            btnBoardName.Click += btnBoardName_Click;
            // 
            // btnDevice
            // 
            btnDevice.Location = new Point(289, 96);
            btnDevice.Name = "btnDevice";
            btnDevice.Size = new Size(75, 23);
            btnDevice.TabIndex = 46;
            btnDevice.Text = "Device";
            btnDevice.UseVisualStyleBackColor = true;
            btnDevice.Click += btnDevice_Click;
            // 
            // btnManufacturer
            // 
            btnManufacturer.Location = new Point(196, 96);
            btnManufacturer.Name = "btnManufacturer";
            btnManufacturer.Size = new Size(88, 23);
            btnManufacturer.TabIndex = 45;
            btnManufacturer.Text = "Manufacturer";
            btnManufacturer.UseVisualStyleBackColor = true;
            btnManufacturer.Click += btnManufacturer_Click;
            // 
            // btnVendorBrand
            // 
            btnVendorBrand.Location = new Point(103, 96);
            btnVendorBrand.Name = "btnVendorBrand";
            btnVendorBrand.Size = new Size(88, 23);
            btnVendorBrand.TabIndex = 44;
            btnVendorBrand.Text = "Vendor Brand";
            btnVendorBrand.UseVisualStyleBackColor = true;
            btnVendorBrand.Click += btnVendorBrand_Click;
            // 
            // btnVendorModel
            // 
            btnVendorModel.Location = new Point(4, 96);
            btnVendorModel.Name = "btnVendorModel";
            btnVendorModel.Size = new Size(94, 23);
            btnVendorModel.TabIndex = 43;
            btnVendorModel.Text = "Vendor Model";
            btnVendorModel.UseVisualStyleBackColor = true;
            btnVendorModel.Click += btnVendorModel_Click;
            // 
            // btnVendorName
            // 
            btnVendorName.Location = new Point(276, 67);
            btnVendorName.Name = "btnVendorName";
            btnVendorName.Size = new Size(90, 23);
            btnVendorName.TabIndex = 42;
            btnVendorName.Text = "Vendor Name";
            btnVendorName.UseVisualStyleBackColor = true;
            btnVendorName.Click += btnVendorName_Click;
            // 
            // btnSerialNo
            // 
            btnSerialNo.Location = new Point(185, 67);
            btnSerialNo.Name = "btnSerialNo";
            btnSerialNo.Size = new Size(75, 23);
            btnSerialNo.TabIndex = 41;
            btnSerialNo.Text = "Serial NO";
            btnSerialNo.UseVisualStyleBackColor = true;
            btnSerialNo.Click += btnSerialNo_Click;
            // 
            // btnBuildId
            // 
            btnBuildId.Location = new Point(4, 67);
            btnBuildId.Name = "btnBuildId";
            btnBuildId.Size = new Size(75, 23);
            btnBuildId.TabIndex = 40;
            btnBuildId.Text = "Build ID";
            btnBuildId.UseVisualStyleBackColor = true;
            btnBuildId.Click += btnBuildId_Click;
            // 
            // button2
            // 
            button2.Location = new Point(243, 38);
            button2.Name = "button2";
            button2.Size = new Size(123, 23);
            button2.TabIndex = 39;
            button2.Text = "Software Version ID";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // btnSkdVersion
            // 
            btnSkdVersion.Location = new Point(93, 67);
            btnSkdVersion.Name = "btnSkdVersion";
            btnSkdVersion.Size = new Size(78, 23);
            btnSkdVersion.TabIndex = 38;
            btnSkdVersion.Text = "SKD Version";
            btnSkdVersion.UseVisualStyleBackColor = true;
            btnSkdVersion.Click += btnSkdVersion_Click;
            // 
            // btnVendorBuildDate
            // 
            btnVendorBuildDate.Location = new Point(4, 38);
            btnVendorBuildDate.Name = "btnVendorBuildDate";
            btnVendorBuildDate.Size = new Size(115, 23);
            btnVendorBuildDate.TabIndex = 37;
            btnVendorBuildDate.Text = "Vendor Build Date";
            btnVendorBuildDate.UseVisualStyleBackColor = true;
            btnVendorBuildDate.Click += btnVendorBuildDate_Click;
            // 
            // btnVendorFingerprint
            // 
            btnVendorFingerprint.Location = new Point(240, 9);
            btnVendorFingerprint.Name = "btnVendorFingerprint";
            btnVendorFingerprint.Size = new Size(126, 23);
            btnVendorFingerprint.TabIndex = 36;
            btnVendorFingerprint.Text = "Vendor Finger Print";
            btnVendorFingerprint.UseVisualStyleBackColor = true;
            btnVendorFingerprint.Click += btnVendorFingerprint_Click;
            // 
            // btnDisplaySize
            // 
            btnDisplaySize.Location = new Point(138, 38);
            btnDisplaySize.Name = "btnDisplaySize";
            btnDisplaySize.Size = new Size(84, 23);
            btnDisplaySize.TabIndex = 35;
            btnDisplaySize.Text = "Display Size";
            btnDisplaySize.UseVisualStyleBackColor = true;
            btnDisplaySize.Click += btnDisplaySize_Click;
            // 
            // btnKernelTest
            // 
            btnKernelTest.Location = new Point(127, 9);
            btnKernelTest.Name = "btnKernelTest";
            btnKernelTest.Size = new Size(97, 23);
            btnKernelTest.TabIndex = 34;
            btnKernelTest.Text = "Linux Kernel";
            btnKernelTest.UseVisualStyleBackColor = true;
            btnKernelTest.Click += btnKernelTest_Click;
            // 
            // btnVersion
            // 
            btnVersion.Location = new Point(4, 9);
            btnVersion.Name = "btnVersion";
            btnVersion.Size = new Size(107, 23);
            btnVersion.TabIndex = 33;
            btnVersion.Text = "Android Version";
            btnVersion.UseVisualStyleBackColor = true;
            btnVersion.Click += btnVersion_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(pbLoading);
            tabPage2.Controls.Add(btnStopApp);
            tabPage2.Controls.Add(btnStartApp);
            tabPage2.Controls.Add(txbSearchPackages);
            tabPage2.Controls.Add(ckbUserApps);
            tabPage2.Controls.Add(ckbSystemApps);
            tabPage2.Controls.Add(ckbAllApp);
            tabPage2.Controls.Add(btnUninstall);
            tabPage2.Controls.Add(checkedListBox1);
            tabPage2.Controls.Add(btnPackageList);
            tabPage2.Controls.Add(btnInstallApk);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(379, 258);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Apps Managment";
            // 
            // pbLoading
            // 
            pbLoading.BackColor = Color.White;
            pbLoading.Image = (Image)resources.GetObject("pbLoading.Image");
            pbLoading.Location = new Point(147, 133);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new Size(75, 75);
            pbLoading.SizeMode = PictureBoxSizeMode.StretchImage;
            pbLoading.TabIndex = 26;
            pbLoading.TabStop = false;
            pbLoading.Visible = false;
            // 
            // btnStopApp
            // 
            btnStopApp.Location = new Point(309, 6);
            btnStopApp.Name = "btnStopApp";
            btnStopApp.Size = new Size(55, 23);
            btnStopApp.TabIndex = 25;
            btnStopApp.Text = "Stop";
            btnStopApp.UseVisualStyleBackColor = true;
            btnStopApp.Click += btnStopApp_Click;
            // 
            // btnStartApp
            // 
            btnStartApp.Location = new Point(244, 6);
            btnStartApp.Name = "btnStartApp";
            btnStartApp.Size = new Size(55, 23);
            btnStartApp.TabIndex = 24;
            btnStartApp.Text = "Start";
            btnStartApp.UseVisualStyleBackColor = true;
            btnStartApp.Click += btnStartApp_Click;
            // 
            // txbSearchPackages
            // 
            txbSearchPackages.Location = new Point(9, 56);
            txbSearchPackages.Name = "txbSearchPackages";
            txbSearchPackages.Size = new Size(357, 23);
            txbSearchPackages.TabIndex = 22;
            txbSearchPackages.TextChanged += txbSearchPackages_TextChanged;
            // 
            // ckbUserApps
            // 
            ckbUserApps.AutoSize = true;
            ckbUserApps.Location = new Point(130, 34);
            ckbUserApps.Name = "ckbUserApps";
            ckbUserApps.Size = new Size(49, 19);
            ckbUserApps.TabIndex = 21;
            ckbUserApps.Text = "User";
            ckbUserApps.UseVisualStyleBackColor = true;
            ckbUserApps.Click += ckbUserApps_CheckedChanged;
            // 
            // ckbSystemApps
            // 
            ckbSystemApps.AutoSize = true;
            ckbSystemApps.Location = new Point(55, 34);
            ckbSystemApps.Name = "ckbSystemApps";
            ckbSystemApps.Size = new Size(69, 19);
            ckbSystemApps.TabIndex = 20;
            ckbSystemApps.Text = "Systems";
            ckbSystemApps.UseVisualStyleBackColor = true;
            ckbSystemApps.Click += ckbSystemApps_CheckedChanged;
            // 
            // ckbAllApp
            // 
            ckbAllApp.AutoSize = true;
            ckbAllApp.Checked = true;
            ckbAllApp.CheckState = CheckState.Checked;
            ckbAllApp.Location = new Point(9, 34);
            ckbAllApp.Name = "ckbAllApp";
            ckbAllApp.Size = new Size(40, 19);
            ckbAllApp.TabIndex = 19;
            ckbAllApp.Text = "All";
            ckbAllApp.UseVisualStyleBackColor = true;
            ckbAllApp.Click += ckbAllApp_CheckedChanged;
            // 
            // btnUninstall
            // 
            btnUninstall.Location = new Point(94, 5);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(75, 23);
            btnUninstall.TabIndex = 18;
            btnUninstall.Text = "Uninstall";
            btnUninstall.UseVisualStyleBackColor = true;
            btnUninstall.Click += btnUninstall_Click_1;
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new Point(9, 85);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.ScrollAlwaysVisible = true;
            checkedListBox1.Size = new Size(357, 166);
            checkedListBox1.Sorted = true;
            checkedListBox1.TabIndex = 17;
            // 
            // btnPackageList
            // 
            btnPackageList.Location = new Point(179, 5);
            btnPackageList.Name = "btnPackageList";
            btnPackageList.Size = new Size(55, 23);
            btnPackageList.TabIndex = 15;
            btnPackageList.Text = "Apps";
            btnPackageList.UseVisualStyleBackColor = true;
            btnPackageList.Click += btnPackages_Click;
            // 
            // btnInstallApk
            // 
            btnInstallApk.Location = new Point(9, 5);
            btnInstallApk.Name = "btnInstallApk";
            btnInstallApk.Size = new Size(75, 23);
            btnInstallApk.TabIndex = 16;
            btnInstallApk.Text = "Install";
            btnInstallApk.UseVisualStyleBackColor = true;
            btnInstallApk.Click += btnInstallApk_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(btnBugreport);
            tabPage3.Controls.Add(btnClearLog);
            tabPage3.Controls.Add(btnExportLog);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(label4);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(txtPackageFilter);
            tabPage3.Controls.Add(txtFilter);
            tabPage3.Controls.Add(txtLineLimitation);
            tabPage3.Controls.Add(btnStopLog);
            tabPage3.Controls.Add(btnStartLog);
            tabPage3.Controls.Add(rtbLog);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(379, 258);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Log";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnBugreport
            // 
            btnBugreport.Location = new Point(89, 221);
            btnBugreport.Name = "btnBugreport";
            btnBugreport.Size = new Size(75, 23);
            btnBugreport.TabIndex = 11;
            btnBugreport.Text = "Bug Report";
            btnBugreport.UseVisualStyleBackColor = true;
            btnBugreport.Click += btnBugreport_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(293, 221);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 23);
            btnClearLog.TabIndex = 10;
            btnClearLog.Text = "Clear Log";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnExportLog
            // 
            btnExportLog.Location = new Point(8, 221);
            btnExportLog.Name = "btnExportLog";
            btnExportLog.Size = new Size(75, 23);
            btnExportLog.TabIndex = 9;
            btnExportLog.Text = "Export Log";
            btnExportLog.UseVisualStyleBackColor = true;
            btnExportLog.Click += btnExportLog_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(173, 14);
            label5.Name = "label5";
            label5.Size = new Size(89, 15);
            label5.TabIndex = 8;
            label5.Text = "Line Limitation:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(208, 43);
            label4.Name = "label4";
            label4.Size = new Size(54, 15);
            label4.TabIndex = 7;
            label4.Text = "Package:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 43);
            label3.Name = "label3";
            label3.Size = new Size(56, 15);
            label3.TabIndex = 6;
            label3.Text = "Keyword:";
            // 
            // txtPackageFilter
            // 
            txtPackageFilter.Location = new Point(268, 40);
            txtPackageFilter.Name = "txtPackageFilter";
            txtPackageFilter.Size = new Size(100, 23);
            txtPackageFilter.TabIndex = 5;
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(74, 40);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(100, 23);
            txtFilter.TabIndex = 4;
            // 
            // txtLineLimitation
            // 
            txtLineLimitation.ForeColor = SystemColors.InactiveCaption;
            txtLineLimitation.Location = new Point(268, 11);
            txtLineLimitation.Name = "txtLineLimitation";
            txtLineLimitation.Size = new Size(100, 23);
            txtLineLimitation.TabIndex = 3;
            txtLineLimitation.Text = "2000";
            txtLineLimitation.Enter += txtLineLimitation_Enter;
            txtLineLimitation.KeyPress += txtLineLimitation_KeyPress;
            txtLineLimitation.Leave += txtLineLimitation_Leave;
            // 
            // btnStopLog
            // 
            btnStopLog.Enabled = false;
            btnStopLog.Location = new Point(89, 12);
            btnStopLog.Name = "btnStopLog";
            btnStopLog.Size = new Size(75, 23);
            btnStopLog.TabIndex = 2;
            btnStopLog.Text = "Stop Log";
            btnStopLog.UseVisualStyleBackColor = true;
            btnStopLog.Click += btnStopLog_Click;
            // 
            // btnStartLog
            // 
            btnStartLog.Location = new Point(8, 12);
            btnStartLog.Name = "btnStartLog";
            btnStartLog.Size = new Size(75, 23);
            btnStartLog.TabIndex = 1;
            btnStartLog.Text = "Start Log";
            btnStartLog.UseVisualStyleBackColor = true;
            btnStartLog.Click += btnStartLog_Click;
            // 
            // rtbLog
            // 
            rtbLog.BackColor = SystemColors.ActiveCaptionText;
            rtbLog.ForeColor = SystemColors.Window;
            rtbLog.HideSelection = false;
            rtbLog.Location = new Point(8, 69);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(360, 146);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "";
            // 
            // btnCheckDevice
            // 
            btnCheckDevice.Location = new Point(298, 6);
            btnCheckDevice.Name = "btnCheckDevice";
            btnCheckDevice.Size = new Size(103, 23);
            btnCheckDevice.TabIndex = 14;
            btnCheckDevice.Text = "Pair Device";
            btnCheckDevice.UseVisualStyleBackColor = true;
            btnCheckDevice.Click += btnPairDevice_Click;
            // 
            // panel1
            // 
            panel1.AccessibleName = "";
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(btnFastboot);
            panel1.Controls.Add(btnBootloader);
            panel1.Controls.Add(btnRecovery);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(btnReboot);
            panel1.Location = new Point(19, 39);
            panel1.Name = "panel1";
            panel1.Size = new Size(382, 58);
            panel1.TabIndex = 13;
            panel1.Tag = "";
            // 
            // btnFastboot
            // 
            btnFastboot.Location = new Point(288, 22);
            btnFastboot.Name = "btnFastboot";
            btnFastboot.Size = new Size(75, 23);
            btnFastboot.TabIndex = 9;
            btnFastboot.Text = "Fastboot";
            btnFastboot.UseVisualStyleBackColor = true;
            btnFastboot.Click += btnFastboot_Click;
            // 
            // btnBootloader
            // 
            btnBootloader.Location = new Point(193, 22);
            btnBootloader.Name = "btnBootloader";
            btnBootloader.Size = new Size(75, 23);
            btnBootloader.TabIndex = 8;
            btnBootloader.Text = "Bootloader";
            btnBootloader.UseVisualStyleBackColor = true;
            btnBootloader.Click += btnBootloader_Click;
            // 
            // btnRecovery
            // 
            btnRecovery.Location = new Point(98, 22);
            btnRecovery.Name = "btnRecovery";
            btnRecovery.Size = new Size(75, 23);
            btnRecovery.TabIndex = 7;
            btnRecovery.Text = "Recovery";
            btnRecovery.UseVisualStyleBackColor = true;
            btnRecovery.Click += btnRecovery_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(-1, 0);
            label2.Name = "label2";
            label2.Size = new Size(119, 15);
            label2.TabIndex = 6;
            label2.Text = "Reboot and Recovery";
            // 
            // btnReboot
            // 
            btnReboot.Location = new Point(3, 22);
            btnReboot.Name = "btnReboot";
            btnReboot.Size = new Size(75, 23);
            btnReboot.TabIndex = 4;
            btnReboot.Text = "Reboot";
            btnReboot.UseVisualStyleBackColor = true;
            btnReboot.Click += btnReboot_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(217, 6);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(75, 23);
            btnDisconnect.TabIndex = 12;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 10);
            label1.Name = "label1";
            label1.Size = new Size(20, 15);
            label1.TabIndex = 11;
            label1.Text = "IP:";
            // 
            // txtIp
            // 
            txtIp.Location = new Point(30, 7);
            txtIp.Name = "txtIp";
            txtIp.Size = new Size(100, 23);
            txtIp.TabIndex = 10;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(136, 7);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 9;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(lblScrcpyHint);
            tabPage4.Controls.Add(checkBox1);
            tabPage4.Controls.Add(label11);
            tabPage4.Controls.Add(label10);
            tabPage4.Controls.Add(comboBox2);
            tabPage4.Controls.Add(label9);
            tabPage4.Controls.Add(button1);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(379, 258);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "TV Control";
            tabPage4.UseVisualStyleBackColor = true;
            tabPage4.Click += tabPage4_Click;
            // 
            // button1
            // 
            button1.Location = new Point(137, 60);
            button1.Name = "button1";
            button1.Size = new Size(105, 28);
            button1.TabIndex = 0;
            button1.Text = "Start Control";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnStartScrcpy_Click;
            // 
            // btnRefreshScrcpyDevices
            // 
            btnRefreshScrcpyDevices.Location = new Point(288, 7);
            btnRefreshScrcpyDevices.Name = "btnRefreshScrcpyDevices";
            btnRefreshScrcpyDevices.Size = new Size(79, 23);
            btnRefreshScrcpyDevices.TabIndex = 8;
            btnRefreshScrcpyDevices.Text = "Refresh";
            btnRefreshScrcpyDevices.UseVisualStyleBackColor = true;
            btnRefreshScrcpyDevices.Click += btnRefreshScrcpyDevices_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(91, 7);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(191, 23);
            comboBox1.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(7, 11);
            label8.Name = "label8";
            label8.Size = new Size(78, 15);
            label8.TabIndex = 2;
            label8.Text = "Active Device:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(11, 22);
            label9.Name = "label9";
            label9.Size = new Size(71, 15);
            label9.TabIndex = 3;
            label9.Text = "Video Mode: ";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(88, 14);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(145, 23);
            comboBox2.TabIndex = 4;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(12, 105);
            label10.Name = "label10";
            label10.Size = new Size(42, 15);
            label10.TabIndex = 5;
            label10.Text = "Status:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(60, 105);
            label11.Name = "label11";
            label11.Size = new Size(26, 15);
            label11.TabIndex = 6;
            label11.Text = "Ready";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(255, 16);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(60, 19);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "Sound";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // lblScrcpyHint
            // 
            lblScrcpyHint.ForeColor = SystemColors.ControlDarkDark;
            lblScrcpyHint.Location = new Point(12, 140);
            lblScrcpyHint.Name = "lblScrcpyHint";
            lblScrcpyHint.Size = new Size(350, 45);
            lblScrcpyHint.TabIndex = 10;
            lblScrcpyHint.Text = "Start Control opens an embedded scrcpy form. Screenshot and Stop are available on the toolbar above the displayed TV image.";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 490);
            Controls.Add(panel4);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ADB Connect";
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panelDeviceSelector.ResumeLayout(false);
            panelDeviceSelector.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbLoading).EndInit();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel4;
        private Panel panelDeviceSelector;
        private Button btnCheckDevice;
        private Panel panel1;
        private Button btnFastboot;
        private Button btnBootloader;
        private Button btnRecovery;
        private Label label2;
        private Button btnReboot;
        private Button btnDisconnect;
        private Label label1;
        private TextBox txtIp;
        private Button btnConnect;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private Button btnGetProps;
        private Button btnGetEnforce;
        private Button btnArm;
        private Button btnTimeZone;
        private Button btnVbmeta;
        private Button btnBuildFingerprint;
        private Button btnClientIdBase;
        private Button btnHardware;
        private Button btnValidation;
        private Button button3;
        private Button btnOemKey;
        private Button btnCpuType;
        private Button btnBoardName;
        private Button btnDevice;
        private Button btnManufacturer;
        private Button btnVendorBrand;
        private Button btnVendorModel;
        private Button btnVendorName;
        private Button btnSerialNo;
        private Button btnBuildId;
        private Button button2;
        private Button btnSkdVersion;
        private Button btnVendorBuildDate;
        private Button btnVendorFingerprint;
        private Button btnDisplaySize;
        private Button btnKernelTest;
        private Button btnVersion;
        private TabPage tabPage2;
        private TextBox txbSearchPackages;
        private CheckBox ckbUserApps;
        private CheckBox ckbSystemApps;
        private CheckBox ckbAllApp;
        private Button btnUninstall;
        private CheckedListBox checkedListBox1;
        private Button btnPackageList;
        private Button btnInstallApk;
        private TabPage tabPage3;
        private Button btnStopLog;
        private Button btnStartLog;
        private RichTextBox rtbLog;
        private Label label4;
        private Label label3;
        private TextBox txtPackageFilter;
        private TextBox txtFilter;
        private TextBox txtLineLimitation;
        private Label label5;
        private Button btnExportLog;
        private Button btnClearLog;
        private Button btnStopApp;
        private Button btnStartApp;
        private PictureBox pbLoading;
        private Button btnBugreport;
        private Label label6;
        private LinkLabel linkSpadra;
        private TabPage tabPage4;
        private Button button1;
        private Button btnRefreshScrcpyDevices;
        private ComboBox comboBox1;
        private Label label9;
        private Label label8;
        private Label label11;
        private Label label10;
        private ComboBox comboBox2;
        private CheckBox checkBox1;
        private Label lblScrcpyHint;
    }
}

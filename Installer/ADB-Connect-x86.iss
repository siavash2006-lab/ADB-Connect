#define MyAppName "ADB Connect"
#define MyAppVersion "1.6.5"
#define MyAppPublisher "Spadra"
#define MyAppURL "https://spadra.ir"
#define MyAppExeName "ADB Connect.exe"

[Setup]
AppId={{4D458BAD-6B98-479D-AC8A-ADBFC68C2832}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL=https://github.com/siavash2006-lab/ADB-Connect/issues
AppUpdatesURL=https://github.com/siavash2006-lab/ADB-Connect/releases
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=ADB-Connect-{#MyAppVersion}-x86-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x86compatible
PrivilegesRequired=admin
SetupIconFile=..\img\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
LicenseFile=..\LICENSE
#ifdef SignedBuild
SignTool=PersonalCodeSign
SignedUninstaller=yes
#else
SignedUninstaller=no
#endif

[Files]
Source: "..\publish\v{#MyAppVersion}\x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\v{#MyAppVersion}\x86\*"; DestDir: "{app}"; Excludes: "{#MyAppExeName}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

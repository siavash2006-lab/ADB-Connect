#define MyAppName "ADB Connect"
#define MyAppVersion "1.6.3"
#define MyAppPublisher "Spadra"
#define MyAppExeName "ADB Connect.exe"

[Setup]
AppId={{764A5D86-820E-4A91-8A29-34ACF0644ADC}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=ADB-Connect-{#MyAppVersion}-x64-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern dark polar
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\img\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Files]
Source: "..\publish\x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

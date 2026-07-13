#define MyAppName "ADB Connect"
#define MyAppVersion "1.6.3"
#define MyAppPublisher "Spadra"
#define MyAppExeName "ADB Connect.exe"

[Setup]
AppId={{4D458BAD-6B98-479D-AC8A-ADBFC68C2832}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=ADB-Connect-{#MyAppVersion}-x86-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern dark polar
ArchitecturesAllowed=x86compatible
SetupIconFile=..\img\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Files]
Source: "..\publish\x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

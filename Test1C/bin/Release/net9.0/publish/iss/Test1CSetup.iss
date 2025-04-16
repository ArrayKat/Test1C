[Setup]
AppId={{9EA90569-2BEE-4348-BA4E-79A807A9043C}
AppName=Test1cProfi
AppVersion=1.0
AppPublisher=ArrayKat, Inc.
AppPublisherURL=https://github.com/ArrayKat
AppSupportURL=https://github.com/ArrayKat
AppUpdatesURL=https://github.com/ArrayKat
DefaultDirName={pf}\Test1cProfi
DisableProgramGroupPage=yes
OutputBaseFilename=Test1cProfi_Setup
Compression=lzma
SolidCompression=yes
UninstallDisplayIcon={app}\Test1C.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\Katya\source\repos\Test1C\Test1C\bin\Release\net9.0\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\Test1cProfi"; Filename: "{app}\Test1C.exe"
Name: "{commondesktop}\Test1cProfi"; Filename: "{app}\Test1C.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Test1C.exe"; Description: "{cm:LaunchProgram,Test1cProfi}"; Flags: nowait postinstall skipifsilent
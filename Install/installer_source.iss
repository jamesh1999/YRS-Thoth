[Setup]
AppName=Project Thoth
AppVersion=1.0
DefaultDirName={pf}\ProjectThoth
DefaultGroupName=Project Thoth

[Files]
Source: "..\*"; DestDir: "{app}"
Source: "..\Client\*"; DestDir: "{app}\Client"
Source: "..\WebServer\WebFixServer\bin\Release\*"; DestDir: "{app}\Server"
Source: "..\WebServer\WebFixServer\bin\Release\Filters\*"; DestDir: "{app}\Server\Filters"
Source: "..\WebServer\WebFixServer\bin\Release\Filters\Standard\*"; DestDir: "{app}\Server\Filters\Standard"
Source: "..\Install\icon.ico"; DestDir: "{app}"
Source: "..\Install\start.bat"; DestDir: "{app}"

[Tasks]
Name: desktopicon; Description: "Create a &desktop icon"
Name: starticon; Description: "Create a start menu icon"

[Icons]
Name: "{userdesktop}\Project Thoth"; Filename: "{app}\start.bat"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon
Name: "{group}\Project Thoth"; Filename: "{app}\start.bat"; IconFilename: "{app}\icon.ico"; Tasks: starticon

[Run] 
Filename: "{app}\Client\install.html"; Description: "Install the add-on (Firefox must be your default browser)"; Flags: postinstall shellexec
Filename: "{app}\README.txt"; Description: "View the README file"; Flags: postinstall shellexec

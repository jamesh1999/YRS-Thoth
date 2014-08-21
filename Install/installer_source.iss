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

[Code]
const
ErrorMSG = 'The installation of Python 3 failed. In order to use Project Thoth, Python 3 must be installed. Please install it manually.';

function InitializeSetup():boolean;
var
  ResultCode: integer;
begin

if not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Wow6432Node\Python\PythonCore\3.4') then
begin

// Launch Python setup and wait for it to terminate
 if ShellExec('', 'msiexec', ExpandConstant('/I "{src}\python.msi" /qb'),'', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then
 begin
// handle success if necessary; ResultCode contains the exit code
 end
 else begin
    MsgBox(ErrorMSG, mbError, mb_Ok);
 end;

end

   // Proceed Setup
  Result := True;

end;
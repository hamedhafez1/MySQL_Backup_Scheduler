@echo off

echo Installing MySqlBackup Service...

set service_path= %~dp0G9MySqlBackup.exe
echo Service Path Is: %service_path%
sc.exe create [G9TM][MySqlBackup] binpath= "%service_path%" start= auto

echo Start Service MySqlBackup...
sc.exe start [G9TM][MySqlBackup]

echo Installation And Running Is Successfully
TIMEOUT /T 9 /NOBREAK
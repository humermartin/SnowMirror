InstallUtil.exe /LogToConsole=true /u bin\Debug\MirrorService.exe
InstallUtil.exe /LogToConsole=true  bin\Debug\MirrorService.exe
sc query MirrorService
sc start MirrorService
sleep 3
sc query MirrorService

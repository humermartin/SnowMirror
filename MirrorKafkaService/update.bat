InstallUtil.exe /LogToConsole=true /u bin\Debug\MirrorKafkaService.exe
InstallUtil.exe /LogToConsole=true  bin\Debug\MirrorKafkaService.exe
sc query MirrorKafkaService
sc start MirrorKafkaService
sleep 3
sc query MirrorKafkaService

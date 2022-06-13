@echo off
title Ephedrine Build Tool
echo You can change config by edit `Config.json`.
pause
echo You can change internal start page by edit `index.html`.
pause
echo You can change icon by replace `drivers.ico`.
pause
echo Ensure that you have prepared anything again.
pause
cls
echo Building your Ephedrine Installer...
echo In
ping 127.0.0.1 -n 2 >nul
echo 3
ping 127.0.0.1 -n 2 >nul
echo 2
ping 127.0.0.1 -n 2 >nul
echo 1
ping 127.0.0.1 -n 2 >nul
dotnet restore
dotnet publish -p:PublishSingleFile=true --no-self-contained -r win-x64 -c Release
dotnet test --no-restore --verbosity normal
xcopy bin\Release\net6.0-windows\publish\win-x64\WebModInstaller.exe YourInstaller.exe /F /Y
echo Build Finished.
echo See `YourInstaller.exe`.
pause

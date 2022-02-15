@echo off
rmdir /s /q XIVBackup\bin\Release
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained

echo "Making Mac App Bundle"

mkdir XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"
copy /y XIVBackup\Info.plist XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Info.plist

mkdir XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Contents

mkdir XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Contents\MacOS
copy /y XIVBackup\bin\Release\net6.0\osx-x64\publish\XIVBackup XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Contents\MacOS\XIVBackup

mkdir XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Contents\Resources
copy /y XIVBackup\moogle.icns XIVBackup\bin\Release\net6.0\osx-x64\publish\"XIVBackup.app"\Contents\Resources\moogle.icns
@echo off

:: Set the paths and configurations
set "rootDir=%cd%"
set "buildsDir=%rootDir%\builds"
set "gameServerBuildsDir=%buildsDir%\gameServerBuilds"
set "masterServerBuildsDir=%buildsDir%\masterServerBuilds"

:: Clean previous build directories
if exist "%gameServerBuildsDir%" rmdir /s /q "%gameServerBuildsDir%"
if exist "%masterServerBuildsDir%" rmdir /s /q "%masterServerBuildsDir%"

:: Build GameServer for x86 platform
dotnet publish -c Release -r win-x86 --self-contained true -o "%gameServerBuildsDir%\x86" "%rootDir%\GameServer\GameServer.csproj"

:: Build GameServer for x64 platform
dotnet publish -c Release -r win-x64 --self-contained true -o "%gameServerBuildsDir%\x64" "%rootDir%\GameServer\GameServer.csproj"

:: Build MasterServer for x86 platform
dotnet publish -c Release -r win-x86 --self-contained true -o "%masterServerBuildsDir%\x86" "%rootDir%\MasterServer\MasterServer.csproj"

:: Build MasterServer for x64 platform
dotnet publish -c Release -r win-x64 --self-contained true -o "%masterServerBuildsDir%\x64" "%rootDir%\MasterServer\MasterServer.csproj"

echo Build completed successfully.
pause

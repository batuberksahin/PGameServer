@echo off

set "rootDir=%cd%"
set "buildsDir=%rootDir%\builds"
set "gameServerBuildsDir=%buildsDir%\gameServerBuilds"
set "masterServerBuildsDir=%buildsDir%\masterServerBuilds"

if exist "%gameServerBuildsDir%" rmdir /s /q "%gameServerBuildsDir%"
if exist "%masterServerBuildsDir%" rmdir /s /q "%masterServerBuildsDir%"

dotnet publish -c Release -r win-x86 --self-contained false -o "%gameServerBuildsDir%\x86" "%rootDir%\GameServer\GameServer.csproj"

dotnet publish -c Release -r win-x64 --self-contained false -o "%gameServerBuildsDir%\x64" "%rootDir%\GameServer\GameServer.csproj"

dotnet publish -c Release -r win-x86 --self-contained false -o "%masterServerBuildsDir%\x86" "%rootDir%\MasterServer\MasterServer.csproj"

dotnet publish -c Release -r win-x64 --self-contained false -o "%masterServerBuildsDir%\x64" "%rootDir%\MasterServer\MasterServer.csproj"

echo Build completed successfully.
pause

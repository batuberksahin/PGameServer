#!/bin/bash

rootDir=$(pwd)
buildsDir="$rootDir/builds"
gameServerBuildsDir="$buildsDir/gameServerBuilds"
masterServerBuildsDir="$buildsDir/masterServerBuilds"

rm -rf "$gameServerBuildsDir"
rm -rf "$masterServerBuildsDir"

dotnet publish -c Release -r linux-x64 --self-contained false -o "$gameServerBuildsDir/x64" "$rootDir/GameServer/GameServer.csproj"

dotnet publish -c Release -r linux-x64 --self-contained false -o "$masterServerBuildsDir/x64" "$rootDir/MasterServer/MasterServer.csproj"

echo "Build completed successfully."

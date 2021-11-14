@echo off
cls

:ChooseOption
echo Choose your build
echo 1. Release (default)
echo 2. Debug

set choice=
set /p choice=Option: 
if not '%choice%'=='' set choice=%choice:~0,1%
if '%choice%'=='' (
    set mode=Release
    goto Start
)
if '%choice%'=='1' (
    set mode=Release
    goto Start
)
if '%choice%'=='2' (
    set mode=Debug
    goto Start
)
echo %choice% is not valid, try again
echo.
goto ChooseOption

:Start
echo Build Mode: %mode%

rem By default, MSBuild.exe is not in PATH, so we need to find it ourselfs using vswhere.exe
rem vswhere.exe is guaranteed by Microsoft to be installed in this location:
for /F "tokens=*" %%a in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe') do SET MsBuild=%%a

rem Restore NuGet packages before build
nuget.exe restore LibSWBF2\LibSWBF2\LibSWBF2.vcxproj -PackagesDirectory LibSWBF2\LibSWBF2\packages
"%MsBuild%" LibSWBF2\LibSWBF2\LibSWBF2.vcxproj -verbosity:minimal /t:Rebuild /p:Platform="x64" /p:configuration=%mode%

"%MsBuild%" LibSWBF2\LibSWBF2.NET\LibSWBF2.NET.csproj -verbosity:minimal /t:Rebuild /p:Platform="x64" /p:configuration=%mode%
"%MsBuild%" lua5.0-swbf2-x64\mak.vs2019\lua50_dll.vcxproj -verbosity:minimal /t:Rebuild /p:Platform="x64" /p:configuration=%mode%

rem Copy built binaries to Unity project
copy "lua5.0-swbf2-x64\bin\x64\%mode%\lua50-swbf2-x64.dll" "UnityProject\Assets\Lib\lua50-swbf2-x64.dll" /Y
copy "LibSWBF2\LibSWBF2\x64\%mode%\LibSWBF2.dll" "UnityProject\Assets\Lib\LibSWBF2.dll" /Y
copy "LibSWBF2\LibSWBF2.NET\bin\x64\%mode%\LibSWBF2.NET.dll" "UnityProject\Assets\Lib\LibSWBF2.NET.dll" /Y
copy "LibSWBF2\LibSWBF2.NET\bin\x64\%mode%\LibSWBF2.NET.pdb" "UnityProject\Assets\Lib\LibSWBF2.NET.pdb" /Y

pause
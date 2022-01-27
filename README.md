# SWBF2 Phoenix Project

This project is a re-implementation of the old Star Wars Battlefront II (2005) game, utilizing the Unity game engine.<br/>
It does so by loading all assets and scripts from the original game files at runtime, providing a compatible API layer for the original, compiled Lua scripts.<br/>
<br/>
This project aims for full compatibility with the vanilla game files, and as best as possible with custom maps.<br/>
<br/>
*Click image to view Video*<br/>
[![Star Wars Battlefront II (2005) Unity Runtime - Update - Tech Demo](https://img.youtube.com/vi/hjSlM5hEfGk/0.jpg)](https://www.youtube.com/watch?v=hjSlM5hEfGk)
<br/>

# How to Build
## Windows
### Installation Requirements
* Git. Must be either included in PATH or installed with Git Bash.
* Visual Studio 2019 with the following:
    * Workloads:
        * .NET desktop development
        * Desktop development with C++
    * Components:
        * .NET Framework 4.5 targeting pack
        * .NET SDK (should be selected by default)
        * MSBuild (should be selected by default)
* CMake 3.16 or above. Must be included in PATH!
* Unity 2020.3.x with *Windows Build Support (IL2CPP)*

### Build steps
1. Clone the repository to a directory of your choice using `git clone https://github.com/Ben1138/SWBF2Phoenix --recurse-submodules`. If your forgot to clone with submodules, do `git submodule update --init --recursive`
2. Execute `BuildAndCopyLibsWin.bat` (double click)
3. Choose your build type. For now, Debug is recommended
4. Choose the number of threads used for compilation. Recommended is the number of your CPU cores.
5. Wait for the batch to complete. There should be no red text outputs! Yellow is ok. 
6. Three files should've been successfully copied to `UnityProject/Assets/Lib`:
    * `LibSWBF2.dll`
    * `LibSWBF2.NET.dll`
    * `lua50-swbf2-x64.dll`
7. Add the `UnityProject` directory to UnityHub and open it. This might take a while.
8. Open the package manager in *Windows -> Package Manager* and select the "High Definition RP" package. On the right side, expand "Samples" and import "Particle System Shader Samples"
9. Navigate to `Runtime/Scenes` and open PhxMainScene
10. In the hierarchy, select *Game* and set in the inspector:
    * `Game Path String` to your Star Wars Battlefront II installation directory. E.g.: `C:\Program Files (x86)\Steam\steamapps\common\Star Wars Battlefront II`
    * `Mission List Path` to empty!
11. Go to *File -> Build Settings*, select *PC, Max & Linux Standalone* and choose `Windows` as Target Platform and `x86_64` as Architecture.
12. Click *Build and Run* and choose the `BUILD` directory, residing in the root of this repository

## Linux
### Installation Requirements
* Install via your respective package manager (e.g. `pacman` or `apt`):
    `git gcc make cmake mono msbuild`
* Unity 2020.3.x with *Linux Build Support (IL2CPP)*

### Build steps
1. Clone the repository to a directory of your choice using `git clone https://github.com/Ben1138/SWBF2Phoenix --recurse-submodules`. If your forgot to clone with submodules, do `git submodule update --init --recursive`
2. [ARCH USERS ONLY] If you're on an Arch based system, the current mono package is not correctly installed, which will cause to `LibSWBF2.NET.dll` to not build. Run `arch_mono_4.5_fix.sh` to fix that issue
3. Execute `BuildAndCopyLibsUnix.sh` in your terminal
3. Choose your build type. For now, Debug is recommended
4. Choose the number of threads used for compilation. Recommended is the number of your CPU cores.
5. Wait for the batch to complete. There should be no red text outputs! Yellow is ok. 
6. Three files should've been successfully copied to `UnityProject/Assets/Lib`:
    * `libSWBF2.so`
    * `LibSWBF2.NET.dll`
    * `liblua50-swbf2-x64.so`
7. Add the `UnityProject` directory to UnityHub and open it. This might take a while.
8. Open the package manager in *Windows -> Package Manager* and select the "High Definition RP" package. On the right side, expand "Samples" and import "Particle System Shader Samples"
9. Navigate to `Runtime/Scenes` and open PhxMainScene
10. In the hierarchy, select *Game* and set in the inspector:
    * `Game Path String` to your Star Wars Battlefront II installation directory.
    * `Mission List Path` to empty!
11. Go to *File -> Build Settings*, select *PC, Max & Linux Standalone* and choose `Linux` as Target Platform and `x86_64` as Architecture.
12. Click *Build and Run* and choose the `BUILD` directory, residing in the root of this repository


## Known problems
The Terrain has no shader and just appears in purple.
1. In Unity, navigate to `LVLImport/LVLImport/ConversionAssets` and open SWBFTerrainHDRP in ShaderGraph
2. Select the *BlendTerrainLayers* shader node and check whether *Source* is set to `BlendTerrainLayers`. If it's `None`, drag and drop `BlendTerrainLayers.hlsl` (resides right next to `SWBFTerrainHDRP.shadergraph`) into it.
3. Navigate deeper into `LVLImport/LVLImport/ConversionAssets/Resources` and select `HDRPTerrain`
4. Make sure its shader is set to: `Shader Graphs/SWBFTerrainHDRP`


# Legal Notice

Please note that this re-implementation is neither developed by, nor endorsed by LucasArts, Lucasfilm Games or its parent company Disney.

This project does not distribute any original game files, neither full nor partial, and does not include any other Assets that might belong to the trade mark "Star Wars" in any way.

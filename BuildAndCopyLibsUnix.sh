#!/bin/bash

repopath=$(pwd)

# Build LibSWBF2 cpp lib 
cd LibSWBF2/
rm -rf build
mkdir build
cd build
cmake ..
make all -j4

# Build LibSWBF2 .NET wrapper
cd ../LibSWBF2.NET
xbuild LibSWBF2.NET.csproj

# Build Lua c lib
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
	echo "Building lua for Linux"
    cd "${repopath}/lua5.0-swbf2-x64/src"
elif [[ "$OSTYPE" == "darwin"* ]]; then
	echo "Building lua for Mac"
    cd "${repopath}/lua5.0-swbf2-x64/mak.macosx/"
else
	echo "ERROR: Platform undetected"
	exit
fi

make -f lua50_dll.make


cd "${repopath}"


# Copy built native libs
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
	echo "Copy Linux libs"
    cp lua5.0-swbf2-x64/bin/liblua50.so UnityProject/Assets/Lib/liblua50-swbf2-x64.so
	cp LibSWBF2/build/LibSWBF2/libLibSWBF2.so UnityProject/Assets/Lib/    
elif [[ "$OSTYPE" == "darwin"* ]]; then
	echo "Copying Mac libs"
	cp lua5.0-swbf2-x64/bin/liblua50.dylib UnityProject/Assets/Lib/liblua50-swbf2-x64.dylib
	cp LibSWBF2/build/LibSWBF2/libLibSWBF2.dylib UnityProject/Assets/Lib/
else
	echo "ERROR: Platform undetected"
	exit
fi

# Copy LibSWBF2 .NET wrapper
cp LibSWBF2/LibSWBF2.NET/bin/Debug/LibSWBF2.NET.dll UnityProject/Assets/Lib/

 
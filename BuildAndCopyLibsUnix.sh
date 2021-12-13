#!/bin/bash

# Release or Debug?
choice=""
while [[ "$choice" != "1" && "$choice" != "2" ]]; do
	echo "Choose your build"
	echo "1. Release (default)"
	echo "2. Debug"
	read -p "Option: " choice

	if [[ "$choice" == "" ]]; then
		choice="1"
	fi
done

if [[ "$choice" == "1" ]]; then
	mode="Release"
elif [[ "$choice" == "2" ]]; then
	mode="Debug"
fi

# Number of Threads?
numThreads=""
re='^[0-9]+$'
while [[ ! $numThreads =~ $re || $numThreads < 1 ]]; do
	read -p "How many threads should be used for compilation? (default: 4): " numThreads

	if [[ "$numThreads" == "" ]]; then
		numThreads=4
	fi
done


# Paths
repopath=$(pwd)
src_dir="${repopath}/LibSWBF2/LibSWBF2"
build_dir="${src_dir}/build/${mode}"
unity_lib_dir="${repopath}/UnityProject/Assets/Lib/"


# Build LibSWBF2 cpp lib 
rm -rf ${build_dir}

cmake -S "${src_dir}" -B "${build_dir}" -D CMAKE_BUILD_TYPE=${mode}
cmake --build "${build_dir}" --target all --parallel -- -j $numThreads


# Build LibSWBF2 .NET wrapper
msbuild "${repopath}/LibSWBF2/LibSWBF2.NET/LibSWBF2.NET.csproj" -verbosity:minimal /t:Rebuild /p:Platform="x64" /p:DefineConstants=UNIX /p:configuration=${mode}


# OS specific lua build and lib copy
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
	echo "Building lua for Linux"
	make -C "${repopath}/lua5.0-swbf2-x64/src" -f lua50_dll.make -j $numThreads --always-make

	echo "Copy Linux libs"
	cp "${build_dir}/libSWBF2.so" "${unity_lib_dir}"
    cp "${repopath}/lua5.0-swbf2-x64/bin/liblua50.so" "${unity_lib_dir}"

elif [[ "$OSTYPE" == "darwin"* ]]; then
	echo "Building lua for Mac"
	make -C "${repopath}/lua5.0-swbf2-x64/mak.macosx" -f lua50_dll.make -j $numThreads --always-make

	echo "Copying Mac libs"
	cp "${build_dir}/libSWBF2.dylib" "${unity_lib_dir}"
	cp "${repopath}/lua5.0-swbf2-x64/bin/liblua50.dylib" "${unity_lib_dir}"
else
	echo "ERROR: Unknown platform: $OSTYPE"
	exit
fi


# Copy LibSWBF2 .NET wrapper
cp "${repopath}/LibSWBF2/LibSWBF2.NET/bin/x64/Debug/LibSWBF2.NET.dll" "${repopath}/UnityProject/Assets/Lib/"

# Copy debug database when in Debug mode, delete it otherwise
if [[ ${mode} == "Debug" ]]; then
	cp "${repopath}/LibSWBF2/LibSWBF2.NET/bin/x64/Debug/LibSWBF2.NET.pdb" "${repopath}/UnityProject/Assets/Lib/"
elif [[ ${mode} == "Release" ]]; then
	rm -f "${repopath}/UnityProject/Assets/Lib/LibSWBF2.NET.pdb"
fi
# Build cpp libs
cd LibSWBF2/
rm -rf build
mkdir build
cd build
cmake ..
make all -j4

# Build .NET wrapper
cd ../LibSWBF2.NET
xbuild LibSWBF2.NET.csproj

# Build Lua
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    cd ../../lua5.0-swbf2-x64/src
elif [[ "$OSTYPE" == "darwin"* ]]; then
    cd ../../lua5.0-swbf2-x64/mak.macosx
make -f lua50_dll.make


cd ../..


# Copy built libs
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    cp lua5.0-swbf2-x64/bin/liblua50.so UnityProject/Assets/Lib/liblua50-swbf2-x64.so
	cp LibSWBF2/build/LibSWBF2/libLibSWBF2.so UnityProject/Assets/Lib/    
elif [[ "$OSTYPE" == "darwin"* ]]; then
	cp lua5.0-swbf2-x64/bin/liblua50.dylib UnityProject/Assets/Lib/liblua50-swbf2-x64.dylib
	cp LibSWBF2/build/LibSWBF2/libLibSWBF2.dylib UnityProject/Assets/Lib/

cp LibSWBF2/LibSWBF2.NET/bin/Debug/LibSWBF2.NET.dll UnityProject/Assets/Lib/

 


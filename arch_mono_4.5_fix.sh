mkdir -p /tmp/mono-deb
cd /tmp/mono-deb
FILE=$(curl --silent https://download.mono-project.com/repo/ubuntu/dists/preview-focal/main/binary-amd64/Packages | grep -Po "(?<=Filename: ).+mono-roslyn.+\\.deb")
curl https://download.mono-project.com/repo/ubuntu/${FILE} > mono.deb
ar p mono.deb data.tar.xz | tar xJf -  ./usr/lib/mono/4.5/System.Reflection.Metadata.dll --strip-components 5
sudo mv /usr/lib/mono/4.5/System.Reflection.Metadata.dll /usr/lib/mono/4.5/System.Reflection.Metadata.dll_OLD
sudo cp System.Reflection.Metadata.dll /usr/lib/mono/4.5
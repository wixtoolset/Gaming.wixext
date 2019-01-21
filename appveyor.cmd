@setlocal
@pushd %~dp0

nuget restore

msbuild -p:Configuration=Release -t:Restore

msbuild -p:Configuration=Release src\test\WixToolsetTest.Gaming\WixToolsetTest.Gaming.csproj

msbuild -p:Configuration=Release -t:Pack src\wixext\WixToolset.Gaming.wixext.csproj

@popd
@endlocal
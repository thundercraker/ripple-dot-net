$scriptpath = $MyInvocation.MyCommand.Path
$wd = Split-Path $scriptpath
$buildpath = Join-Path $wd build
$assemblies = Join-Path $buildpath lib\net45
rm -Recurse -Force $buildpath
Push-Location $wd

msbuild  /property:Configuration=Release ..\Ripple.TxSigning\Ripple.TxSigning.csproj
mkdir $assemblies
cp ..\Ripple.TxSigning\bin\Release\Ripple*.dll $assemblies
cp ..\Ripple.TxSigning\bin\Release\Chaos*.dll $assemblies
nuget pack Ripple.NET.nuspec -BasePath build

Pop-Location

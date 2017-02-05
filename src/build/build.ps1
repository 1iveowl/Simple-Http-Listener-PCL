param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild ..\main\ISimpleHttpServer\ISimpleHttpServer.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\SimpleHttpServer\SimpleHttpServer.csproj /t:Build /p:Configuration="Release"

Remove-Item NuGet -Force -Recurse

New-Item -ItemType Directory -Force -Path .\NuGet

NuGet.exe pack SimpleHttpServer.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version

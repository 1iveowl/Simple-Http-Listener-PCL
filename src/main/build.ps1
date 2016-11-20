$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild ISimpleHttpServer\ISimpleHttpServer.csproj /t:Build /p:Configuration="Release"
&$msbuild SimpleHttpServer\SimpleHttpServer.csproj /t:Build /p:Configuration="Release"

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path 'SimpleHttpServer\bin\Release\ISimpleHttpServer.dll')).Version.ToString(3)

Remove-Item SimpleHttpServer\NuGet -Force -Recurse

New-Item -ItemType Directory -Force -Path .\NuGet

NuGet.exe pack SimpleHttpServer.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version
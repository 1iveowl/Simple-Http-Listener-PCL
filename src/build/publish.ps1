$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild ..\main\ISimpleHttpServer\ISimpleHttpServer.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\SimpleHttpServer\SimpleHttpServer.csproj /t:Build /p:Configuration="Release"

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\SimpleHttpServer\bin\Release\ISimpleHttpServer.dll')).Version.ToString(3)

Remove-Item NuGet -Force -Recurse

New-Item -ItemType Directory -Force -Path .\NuGet

NuGet.exe pack SimpleHttpServer.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version
Nuget.exe push .\Nuget\SimpleHttpListener.$version.nupkg -Source https://www.nuget.org
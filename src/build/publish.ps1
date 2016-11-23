param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\SimpleHttpServer\bin\Release\ISimpleHttpServer.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\SimpleHttpServer\bin\Release\ISimpleHttpServer.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

Nuget.exe push .\Nuget\SimpleHttpListener.$version.nupkg -Source https://www.nuget.org
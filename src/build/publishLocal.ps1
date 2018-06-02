param([string]$betaver)

.\build.ps1 $version

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\ISimpleHttpServer.Netstandard\bin\Release\netstandard2.0\ISimpleHttpServer.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\ISimpleHttpServer.Netstandard\bin\Release\netstandard2.0\ISimpleHttpServer.dll')).Version.ToString(3) + "-" + $betaver
}

NuGet.exe pack SimpleHttpServer.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version

nuget.exe push -Source "1iveowlNuGetRepo" -ApiKey key .\NuGet\SimpleHttpListener.$version.nupkg

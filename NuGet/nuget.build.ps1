#param( [string] $version = $(throw "version argument is required") )
$strPath = [System.IO.Path]::GetFullPath($PSScriptRoot + "/XPression.dll");
$Assembly = [Reflection.Assembly]::Loadfile($strPath)
$AssemblyName = $Assembly.GetName()
$Assemblyversion = $AssemblyName.version

&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/XPression.nuspec -Version $Assemblyversion
&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/XPression.LinqToEntities.nuspec -Version $Assemblyversion
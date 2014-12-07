Properties {
	$release = "1.0.0.0"
	$src = (get-item "./").fullname
	$sln = "$src\Stateful.sln"
	$snk = "$src\Stateful.snk"
	$zip = "7za.exe"
}

Include ".\common.ps1"

FormatTaskName (("-"*79) + "`n`n    {0}`n`n" + ("-"*79))

Task default -depends Rebuild

Task Rebuild -depends VsVars,Clean,KeyGen,Version {
	Build-Solution $sln "Any CPU"
}

Task KeyGen -depends VsVars -precondition { return !(test-path $snk) } {
	exec { cmd /c sn -k $snk }
}

Task Version {
	Update-AssemblyVersion $src $release 'Stateful.Tests'
}

Task Clean {
	Clean-BinObj $src
}

Task VsVars {
	Set-VsVars
}

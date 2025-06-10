#!/usr/bin/env pwsh

$sandboxPath='test/Sandbox'
if (git diff --cached --name-only | Select-String $sandboxPath)
{
	Write-Host
		"Detected changes in '$sandboxPath'. If you meant to commit them, set `'SKIP`' to `'check-sandbox`' `n" +
		"bash: SKIP=`'check-sandbox`' `n" +
		"pwsh: `$env:SKIP=`'check-sandbox`'"

	exit 1
}

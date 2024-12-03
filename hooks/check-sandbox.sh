#!/usr/bin/env bash
sandbox_path='test/Sandbox'
if git diff --cached --name-only | grep -q "$sandbox_path"; then
	echo -e \
	    "Detected changes in '${sandbox_path}'. If you meant to commit them, set 'SKIP' to 'check-sandbox'\n" \
		"bash: SKIP='check-sandbox'\n" \
		"pwsh: \$env:SKIP='check-sandbox'" 
	exit 1
fi

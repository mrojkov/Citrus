@echo off
SET CMD=""Run Tangerine""
start Orange\bin\Win\Release\Orange.exe -run:bin\Win\Release\Orange.CLI.exe "-runargs:..\..\Main\LetsEat.citproj --target:Win --command:%CMD%"
@echo off
pushd %~dp0
.\nuget.exe restore
call .\packages\psake.4.4.1\tools\psake.cmd %*
popd

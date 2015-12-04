@echo off

set target=%1
if "%target%" == "" (
   set target=BuildAndTestCmd
)
set config=%2
if "%config%" == "" (
   set config=Release
)

msbuild build\build.proj /t:"%target%" /p:Configuration="%config%" /v:N /m /nr:false
@echo off

where msbuild >nul 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO setupEnv1
GOTO envGood

:setupEnv1
echo Trying VS160COMNTOOLS
IF "%VS160COMNTOOLS%" == "" (
  echo VS160COMNTOOLS not found.
  goto :setupEnv2
)
call "%VS160COMNTOOLS%\VsDevCmd.bat"
GOTO envGood

:setupEnv2
echo Trying VS160ENTCOMNTOOLS
IF "%VS160ENTCOMNTOOLS%" == "" (
  echo VS160ENTCOMNTOOLS not found.
  goto :eof
)
call "%VS160ENTCOMNTOOLS%\VsDevCmd.bat"
GOTO envGood

:envGood

msbuild Dishes.sln /p:Configuration=Release /p:Platform="Any CPU" /m
del /s /q DistPackage
mkdir DistPackage

copy bin\Release\Dishes.exe DistPackage\Dishes.exe
copy bin\Release\Dishes.exe.config DistPackage\Dishes.exe.config
copy bin\Release\LICENSE DistPackage\LICENSE
copy bin\Release\ThirdPartyNotices.html DistPackage\ThirdPartyNotices.html

del Dishes.zip
pushd DistPackage
%SYSTEMROOT%\System32\tar.exe -caf ..\Dishes.zip *
popd

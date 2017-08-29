@echo off
@REM  ---------------------------------------------------------------------------------
@REM  InstallServices.bat file
@REM
@REM  This batch file installs/uninstalls various services for the Enterprise Library 
@REM  application blocks.
@REM  
@REM  Optional arguments for this batch file:
@REM    1 - /u to unstall. Otherwise it is installed.
@REM   2 - The source folder (i.e. where the build resides.
@REM       Defaults to .\bin)
@REM  ----------------------------------------------------------------------------------

echo.
echo ==========================================================================
echo   InstallServices.bat                                                    
echo      Installs/uninstalls services for the Enterprise Library  
echo ==========================================================================
echo.

set installUtilDir=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\
set binDir=".\bin"
set action=
set pause=true

@REM  ---------------------------------------------------------------
@REM  User can override default directory containing the
@REM  the Enterprise Library assemblies by supplying 
@REM  a parameter to batch file (e.g. InstallServices C:\bin).
@REM  ---------------------------------------------------------------

if "%1"=="/?" goto HELP

if "%1"=="" goto RUN

@REM  ----------------------------------------------------
@REM  If the first parameter is /q, do not pause
@REM  at the end of execution.
@REM  ----------------------------------------------------

if /i "%1"=="/q" (
 set pause=false
 SHIFT
)

@REM  ----------------------------------------------------
@REM  If the first parameter is /u, uninstall.
@REM  ----------------------------------------------------

if /i "%1"=="/u" (
 set action=%1
 SHIFT
)

@REM  ---------------------------------------------------------------
@REM  User can override default destination directory by specifiying
@REM  a parameter to batch file (e.g. CopyAssemblies Debug c:\bin).
@REM  ---------------------------------------------------------------

if not "%1"=="" (
 set binDir=%1
 SHIFT
)

if not "%1"=="" goto HELP

:RUN

@REM  ------------------------------------------------
@REM  Shorten the command prompt for making the output
@REM  easier to read.
@REM  ------------------------------------------------
set savedPrompt=%prompt%
set prompt=*$g

@ECHO ----------------------------------------
@ECHO InstallServices.bat Started
@ECHO ----------------------------------------
@ECHO.

@REM  ----------------------------------------
@REM  Register VS.NET environment variables
@REM  (required to call installutil)
@REM  ----------------------------------------

if not Exist "%installUtilDir%" goto HELPFW
@set PATH=%PATH%;%installUtilDir%

@REM -------------------------------------------------------
@REM Change to the directory where the assemblies reside
@REM -------------------------------------------------------

if not Exist "%binDir%" goto HELP
pushd %binDir%

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Common Application Block
@ECHO -----------------------------------------------------------------
@ECHO.
if Exist Microsoft.Practices.EnterpriseLibrary.Common.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Common.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Caching Application Block
@ECHO -----------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Caching.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Caching.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Cryptography Application Block
@ECHO -----------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Security.Cryptography.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Security.Cryptography.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Data Access Application Block
@ECHO -----------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Data.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Data.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------------
@ECHO Installing Services for the Exception Handling Application Block
@ECHO -----------------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO ---------------------------------------------------------------------------------
@ECHO Installing Services for the Logging and Instrumentation Application Block
@ECHO ---------------------------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Logging.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Logging.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Security Application Block
@ECHO -----------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Security.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Security.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO -----------------------------------------------------------------
@ECHO Installing Services for the Validation Application Block
@ECHO -----------------------------------------------------------------
@ECHO.

if Exist Microsoft.Practices.EnterpriseLibrary.Validation.dll installutil %action% Microsoft.Practices.EnterpriseLibrary.Validation.dll
@if errorlevel 1 goto :error

@ECHO.
@ECHO ----------------------------------------
@ECHO InstallServices.bat Completed
@ECHO ----------------------------------------
@ECHO.

@REM  ----------------------------------------
@REM  Restore the command prompt and exit
@REM  ----------------------------------------
@goto :exit

@REM  -------------------------------------------
@REM  Handle errors
@REM
@REM  Use the following after any call to exit
@REM  and return an error code when errors occur
@REM
@REM  if errorlevel 1 goto :error	
@REM  -------------------------------------------
:error
@ECHO An error occured in InstallServices.bat - %errorLevel%
if %pause%==true PAUSE
@exit errorLevel

:HELPFW
echo Error: Unable to locate the .NET Framework v2.0
echo.
echo InstallServices.bat assumes the .NET Framework v2.0 has been installed in its default location 
echo ("%visualStudioDir%".) 
echo.
echo If you have installed the .NET Framework v2.0 to a different location, you will need 
echo to update this batch file to reflect that location.
echo.
goto exit

:HELP
echo Usage: InstallServices.bat [/q] [/u] [build folder]  
echo.
echo Examples:
echo.
echo    "InstallServices" - installs services for Enterprise Library assemblies       
echo    "InstallServices /u" - uninstalls services for Enterprise Library assemblies 
echo    "InstallServices /q" - installs services, no pause when error occurs (quiet mode)     
echo    "InstallServices bin" - installs services retrieving Enterprise Library assemblies from the "bin" folder
echo.

@REM  ----------------------------------------
@REM  The exit label
@REM  ----------------------------------------
:exit

popd
set pause=
set binDir=
set prompt=%savedPrompt%
set savedPrompt=
set action=

echo on


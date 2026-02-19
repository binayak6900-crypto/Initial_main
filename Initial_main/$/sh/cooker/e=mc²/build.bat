@echo off
REM mc² Build System for Windows
REM Supports command-line arguments via -arg flags
REM
REM ⚠️ NOTE: C++ FILES DELETED - ASSEMBLY-ONLY DEVELOPMENT ⚠️
REM The C++ bootstrap assembler has been deleted (Task 7).
REM This build system now uses the compiled mc2asm.exe for all operations.
REM All future development is assembly-only using spacetime assembly.

setlocal enabledelayedexpansion

REM Default values
set "ACTION="
set "INPUT_FILE="
set "OUTPUT_FILE="
set "VERSION="
set "VERBOSE=0"
set "INDENT_MODE=strict"
set "INDENT_SIZE=4"
set "BACKUP_VERSION="

REM Parse command-line arguments
:parse_args
if "%~1"=="" goto end_parse
if /i "%~1"=="-compile" (
    set "ACTION=compile"
    set "INPUT_FILE=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-output" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-o" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-version" (
    set "VERSION=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-self-host" (
    set "ACTION=self-host"
    shift
    goto parse_args
)
if /i "%~1"=="-backup" (
    set "ACTION=backup"
    set "BACKUP_VERSION=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-verbose" (
    set "VERBOSE=1"
    shift
    goto parse_args
)
if /i "%~1"=="-loose-indent" (
    set "INDENT_MODE=loose"
    shift
    goto parse_args
)
if /i "%~1"=="-extra-loose-indent" (
    set "INDENT_MODE=extra-loose"
    shift
    goto parse_args
)
if /i "%~1"=="-indent-size" (
    set "INDENT_SIZE=%~2"
    shift
    shift
    goto parse_args
)
if /i "%~1"=="-help" goto show_help
if /i "%~1"=="-h" goto show_help
shift
goto parse_args

:end_parse

REM Execute action
if "%ACTION%"=="compile" goto do_compile
if "%ACTION%"=="self-host" goto do_self_host
if "%ACTION%"=="backup" goto do_backup
if "%ACTION%"=="" goto show_help

:do_compile
echo [mc² Build System] Compiling %INPUT_FILE%...
if "%OUTPUT_FILE%"=="" (
    echo Error: Output file not specified. Use -output or -o flag.
    exit /b 1
)
if "%INPUT_FILE%"=="" (
    echo Error: Input file not specified.
    exit /b 1
)
REM TODO: Call mc² compiler when implemented
echo Compilation target: %INPUT_FILE% -^> %OUTPUT_FILE%
echo Indentation mode: %INDENT_MODE%
echo Indent size: %INDENT_SIZE%
if "%VERBOSE%"=="1" echo [Verbose mode enabled]
echo.
echo Note: Compiler not yet implemented. This is a placeholder.
exit /b 0

:do_self_host
echo [mc² Build System] Self-hosting build...
if "%VERSION%"=="" (
    echo.
    echo Version not specified. Please provide version information.
    echo.
    echo Format: prefix-X-Y-Z-W-V
    echo Supported prefixes: v, alpha, beta, rc, dev
    echo Examples: v-1-0-0-0-0, alpha-0-0-0-2-0, beta-0-0-1-0-0
    echo.
    set /p "VERSION_PREFIX=Enter version prefix (v/alpha/beta/rc/dev): "
    set /p "VERSION_MAJOR=Enter major version: "
    set /p "VERSION_MINOR=Enter minor version: "
    set /p "VERSION_PATCH=Enter patch version: "
    set /p "VERSION_BUILD=Enter build version: "
    set /p "VERSION_REVISION=Enter revision: "
    set "VERSION=!VERSION_PREFIX!-!VERSION_MAJOR!-!VERSION_MINOR!-!VERSION_PATCH!-!VERSION_BUILD!-!VERSION_REVISION!"
    echo.
    echo Building version: !VERSION!
    echo.
)
REM Check if version directory already exists
if exist bin\%VERSION% (
    echo.
    echo Warning: Version %VERSION% already exists!
    set /p "OVERWRITE=Overwrite existing version? (y/n): "
    if /i not "!OVERWRITE!"=="y" (
        echo Build cancelled.
        exit /b 1
    )
)

echo Building compiler version: %VERSION%
echo Target directory: bin\%VERSION%\
if not exist bin mkdir bin
if not exist bin\%VERSION% mkdir bin\%VERSION%

REM Find the latest version for auto-increment suggestion
echo.
echo Detecting existing versions...
for /f "delims=" %%d in ('dir /b /ad bin 2^>nul ^| findstr /r "^[a-z]*-[0-9]"') do (
    set "LAST_VERSION=%%d"
)
if defined LAST_VERSION (
    echo Last version found: %LAST_VERSION%
    echo Current version: %VERSION%
)

echo.
echo Copying assembler to versioned directory...
set "BINARY_NAME=mc2-%VERSION%.exe"
if exist bin\mc2asm.exe (
    copy bin\mc2asm.exe bin\%VERSION%\%BINARY_NAME% >nul
    echo Created: bin\%VERSION%\%BINARY_NAME%
) else if exist bin\v-0-0-0-1-0\mc2asm.exe (
    copy bin\v-0-0-0-1-0\mc2asm.exe bin\%VERSION%\%BINARY_NAME% >nul
    echo Created: bin\%VERSION%\%BINARY_NAME%
) else (
    echo Warning: Assembler not found. Build assembler first.
)

echo Creating version metadata...
echo Version: %VERSION% > bin\%VERSION%\VERSION.txt
echo Build Date: %date% %time% >> bin\%VERSION%\VERSION.txt
echo Build System: Windows >> bin\%VERSION%\VERSION.txt
echo.
echo Compiler version %VERSION% saved to bin\%VERSION%\
echo.
echo To use this version:
echo   bin\%VERSION%\%BINARY_NAME% input.spacetime -o output.e²
exit /b 0

:do_backup
echo [mc² Build System] Creating versioned backup...
if "%BACKUP_VERSION%"=="" (
    echo.
    echo Version not specified. Please provide version information.
    echo.
    echo Format: prefix-X-Y-Z-W-V
    echo Supported prefixes: v, alpha, beta, rc, dev
    echo Examples: v-0-0-0-1-0, alpha-0-0-0-2-0, beta-0-0-1-0-0
    echo.
    set /p "VERSION_PREFIX=Enter version prefix (v/alpha/beta/rc/dev): "
    set /p "VERSION_MAJOR=Enter major version: "
    set /p "VERSION_MINOR=Enter minor version: "
    set /p "VERSION_PATCH=Enter patch version: "
    set /p "VERSION_BUILD=Enter build version: "
    set /p "VERSION_REVISION=Enter revision: "
    set "BACKUP_VERSION=!VERSION_PREFIX!-!VERSION_MAJOR!-!VERSION_MINOR!-!VERSION_PATCH!-!VERSION_BUILD!-!VERSION_REVISION!"
    echo.
    echo Creating backup version: !BACKUP_VERSION!
    echo.
)

REM Check if version directory already exists
if exist bin\%BACKUP_VERSION% (
    echo.
    echo Warning: Version %BACKUP_VERSION% already exists!
    set /p "OVERWRITE=Overwrite existing version? (y/n): "
    if /i not "!OVERWRITE!"=="y" (
        echo Backup cancelled.
        exit /b 1
    )
)

echo Creating backup of current compiler...
echo Target directory: bin\%BACKUP_VERSION%\
if not exist bin mkdir bin
if not exist bin\%BACKUP_VERSION% mkdir bin\%BACKUP_VERSION%

REM Find the latest version for reference
echo.
echo Detecting existing versions...
for /f "delims=" %%d in ('dir /b /ad bin 2^>nul ^| findstr /r "^[a-z]*-[0-9]"') do (
    set "LAST_VERSION=%%d"
)
if defined LAST_VERSION (
    echo Last version found: %LAST_VERSION%
)
echo Creating version: %BACKUP_VERSION%

echo.
echo Copying current compiler to versioned directory...
set "BINARY_NAME=mc2-%BACKUP_VERSION%.exe"
if exist mc2asm.exe (
    copy mc2asm.exe bin\%BACKUP_VERSION%\%BINARY_NAME% >nul
    echo Created: bin\%BACKUP_VERSION%\%BINARY_NAME%
) else if exist bin\mc2asm.exe (
    copy bin\mc2asm.exe bin\%BACKUP_VERSION%\%BINARY_NAME% >nul
    echo Created: bin\%BACKUP_VERSION%\%BINARY_NAME%
) else (
    echo Error: mc2asm.exe not found in root or bin directory.
    exit /b 1
)

echo Creating version metadata...
echo Version: %BACKUP_VERSION% > bin\%BACKUP_VERSION%\VERSION.txt
echo Build Date: %date% %time% >> bin\%BACKUP_VERSION%\VERSION.txt
echo Build System: Windows >> bin\%BACKUP_VERSION%\VERSION.txt
echo Backup of: mc2asm.exe >> bin\%BACKUP_VERSION%\VERSION.txt
echo.
echo Backup complete! Compiler version %BACKUP_VERSION% saved to bin\%BACKUP_VERSION%\
echo.
echo To use this version:
echo   bin\%BACKUP_VERSION%\%BINARY_NAME% input.spacetime -o output.e²
exit /b 0

:show_help
echo mc² Build System - Windows
echo.
echo Usage:
echo   build.bat -compile ^<input^> -output ^<output^> [options]
echo   build.bat -self-host -version ^<version^> [options]
echo   build.bat -help
echo.
echo Actions:
echo   -compile ^<file^>        Compile a .mc² source file
echo   -self-host             Build compiler using previous version
echo   -backup ^<version^>      Create versioned backup of current compiler
echo   -help, -h              Show this help message
echo.
echo Options:
echo   -output, -o ^<file^>    Specify output file path
echo   -version ^<ver^>        Specify compiler version for self-hosting
echo   -verbose               Enable detailed compilation output
echo   -loose-indent          Enable loose indentation mode (warnings)
echo   -extra-loose-indent    Disable indentation validation
echo   -indent-size ^<n^>      Set indentation size (default: 4)
echo.
echo Examples:
echo   build.bat -compile src\main.mc² -output bin\program.e²
echo   build.bat -self-host -version v-1-0-0-0-0
echo   build.bat -backup v-0-0-0-1-0
echo   build.bat -compile test.mc² -o test.e² -verbose -loose-indent
echo.
exit /b 0

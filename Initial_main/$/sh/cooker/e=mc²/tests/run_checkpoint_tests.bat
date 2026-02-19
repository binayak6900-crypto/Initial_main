@echo off
echo ========================================
echo CRITICAL CHECKPOINT 6 - Assembly Testing
echo ========================================
echo.
echo This script runs all assembly tests multiple times
echo to verify consistency before C++ deletion.
echo.

set ITERATIONS=5
set FAILED=0

echo Running %ITERATIONS% iterations of all tests...
echo.

for /L %%i in (1,1,%ITERATIONS%) do (
    echo ----------------------------------------
    echo Iteration %%i of %ITERATIONS%
    echo ----------------------------------------
    
    echo [%%i] Testing test_simple.spacetime...
    ..\mc2asm.exe test_simple.spacetime test_simple.e_sub2 > nul 2>&1
    if errorlevel 1 (
        echo [FAIL] test_simple.spacetime assembly failed
        set FAILED=1
    ) else (
        ..\mc2asm.exe test_simple.e_sub2 > nul 2>&1
        if errorlevel 1 (
            echo [FAIL] test_simple.e_sub2 execution failed
            set FAILED=1
        ) else (
            echo [PASS] test_simple
        )
    )
    
    echo [%%i] Testing test_comprehensive.spacetime...
    ..\mc2asm.exe test_comprehensive.spacetime test_comprehensive.e_sub2 > nul 2>&1
    if errorlevel 1 (
        echo [FAIL] test_comprehensive.spacetime assembly failed
        set FAILED=1
    ) else (
        ..\mc2asm.exe test_comprehensive.e_sub2 > nul 2>&1
        if errorlevel 1 (
            echo [FAIL] test_comprehensive.e_sub2 execution failed
            set FAILED=1
        ) else (
            echo [PASS] test_comprehensive
        )
    )
    
    echo [%%i] Testing test_core_functions.spacetime...
    ..\mc2asm.exe test_core_functions.spacetime test_core_functions.e_sub2 > nul 2>&1
    if errorlevel 1 (
        echo [FAIL] test_core_functions.spacetime assembly failed
        set FAILED=1
    ) else (
        ..\mc2asm.exe test_core_functions.e_sub2 > nul 2>&1
        if errorlevel 1 (
            echo [FAIL] test_core_functions.e_sub2 execution failed
            set FAILED=1
        ) else (
            echo [PASS] test_core_functions
        )
    )
    
    echo.
)

echo ========================================
if %FAILED%==0 (
    echo ALL TESTS PASSED - Ready for C++ deletion
    echo ========================================
    exit /b 0
) else (
    echo SOME TESTS FAILED - DO NOT proceed to C++ deletion
    echo ========================================
    exit /b 1
)

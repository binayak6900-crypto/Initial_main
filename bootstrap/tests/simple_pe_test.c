#include <stdio.h>
#include <windows.h>

int main() {
    printf("Testing basic file creation...\n");
    
    HANDLE hFile = CreateFileA(
        "simple_test.exe",
        GENERIC_WRITE,
        0,
        NULL,
        CREATE_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        NULL
    );
    
    if (hFile == INVALID_HANDLE_VALUE) {
        printf("ERROR: Failed to create file. Error code: %lu\n", GetLastError());
        return 1;
    }
    
    const char* test_data = "Hello, World!";
    DWORD bytesWritten;
    
    if (!WriteFile(hFile, test_data, strlen(test_data), &bytesWritten, NULL)) {
        printf("ERROR: Failed to write file. Error code: %lu\n", GetLastError());
        CloseHandle(hFile);
        return 1;
    }
    
    CloseHandle(hFile);
    printf("SUCCESS: File created and written successfully\n");
    return 0;
}
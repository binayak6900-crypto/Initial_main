set -e

cd "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild"
/usr/bin/cmake.exe --regenerate-during-build -S$(CMAKE_SOURCE_DIR) -B$(CMAKE_BINARY_DIR)

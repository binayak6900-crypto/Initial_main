set -e

cd "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild"
/usr/bin/ccmake.exe -S$(CMAKE_SOURCE_DIR) -B$(CMAKE_BINARY_DIR)

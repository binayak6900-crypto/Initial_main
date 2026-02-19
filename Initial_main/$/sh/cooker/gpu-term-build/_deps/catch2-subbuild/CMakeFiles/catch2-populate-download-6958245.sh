set -e

cd "/d/\$/sh/cooker/gpu-term-build/_deps"
/usr/bin/cmake.exe -DCMAKE_MESSAGE_LOG_LEVEL=VERBOSE -P "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild/catch2-populate-prefix/src/catch2-populate-stamp/download-catch2-populate.cmake"
/usr/bin/cmake.exe -DCMAKE_MESSAGE_LOG_LEVEL=VERBOSE -P "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild/catch2-populate-prefix/src/catch2-populate-stamp/verify-catch2-populate.cmake"
/usr/bin/cmake.exe -DCMAKE_MESSAGE_LOG_LEVEL=VERBOSE -P "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild/catch2-populate-prefix/src/catch2-populate-stamp/extract-catch2-populate.cmake"
/usr/bin/cmake.exe -E touch "/d/\$/sh/cooker/gpu-term-build/_deps/catch2-subbuild/catch2-populate-prefix/src/catch2-populate-stamp/catch2-populate-download"

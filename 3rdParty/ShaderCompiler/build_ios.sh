#!/bin/sh
cd build
cmake .. -G Xcode -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX="install" -DSHADER_COMPILER_SHARED_LIB="OFF" -DSHADER_COMPILER_PLATFORM_IOS="ON"
cmake --build . --config Release --target install

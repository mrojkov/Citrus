#!/bin/sh
mkdir ios_build
cd ios_build
xcrun -sdk iphoneos clang -O3 -c -arch armv7 ../etc2.c -o etc2_armv7.o
xcrun -sdk iphoneos clang -O3 -c -arch arm64 ../etc2.c -o etc2_arm64.o
ar -crs libEtc2_armv7.a etc2_armv7.o
ar -crs libEtc2_arm64.a etc2_arm64.o
lipo -create libEtc2_armv7.a libEtc2_arm64.a -output libEtc2Decoder.a
mv libEtc2Decoder.a ..
cd ..
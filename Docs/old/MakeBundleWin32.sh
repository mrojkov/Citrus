#!/bin/bash
# If this doesn't work, ensure you have UNIX line endings in this file
# (\n, not \r\n.) You can use Notepad++ to switch them.

export GAME=RoyalCandy
export OUTPUT_DIR=Output
export ASSEMBLIES="$GAME.Game Lime LemonBinding Kumquat OpenTK protobuf-net"

# Cygwin package requirements: gcc-mingw, pkg-config
# If you want to pass -z to mkbundle: mingw-zlib1, mingw-zlib-devel
 
export MONO=/cygdrive/c/Mono-2.10.8/
# Required to find mkbundle
export PATH=$PATH:$MONO/bin
# Required for the pkg-config call that mkbundle causes to work
export PKG_CONFIG_PATH=$MONO/lib/pkgconfig

# The -U _WIN32 undefines the _WIN32 symbol. The source code mkbundle executes
# is totally broken on Win32 but actually works if you don't let it know
# that it is on Win32.
export CC="i686-pc-mingw32-gcc -U _WIN32"

mkdir ./$OUTPUT_DIR
$MONO/bin/mkbundle $GAME.exe $ASSEMBLIES --deps -o $OUTPUT_DIR/$GAME.exe

# Copy mono-2.0.dll here since Output.exe depends on it.
cp $MONO/bin/mono-2.0.dll ./$OUTPUT_DIR
cp $MONO/bin/MonoPosixHelper.dll ./$OUTPUT_DIR
cp ./wrap_oal.dll ./$OUTPUT_DIR
cp ./OpenAL32.dll ./$OUTPUT_DIR
cp ./Data.Desktop ./$OUTPUT_DIR
cp ./Lemon.dll ./$OUTPUT_DIR

cmake_minimum_required(VERSION 3.12)

project(ShaderCompiler)

if(CMAKE_CXX_COMPILER_ID STREQUAL "AppleClang")
   set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -ftemplate-depth=300")
   set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
   set(CMAKE_CXX_FLAGS_DEBUG "-g -O0" )
   set(CMAKE_CXX_FLAGS_RELWITHDEBINFO "-g -O3" )
   set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -stdlib=libc++ -dead_strip")
endif()

add_subdirectory(external EXCLUDE_FROM_ALL)

option(SHADER_COMPILER_SHARED_LIB "Build Shared Libraries" OFF)

set(SHADER_COMPILER_LIB_TYPE STATIC)
if(SHADER_COMPILER_SHARED_LIB)
set(SHADER_COMPILER_LIB_TYPE SHARED)
endif()

if(SHADER_COMPILER_PLATFORM_IOS)
	set(CMAKE_XCODE_ATTRIBUTE_IPHONEOS_DEPLOYMENT_TARGET "9.0")
	set(CMAKE_OSX_SYSROOT iphoneos)	
	set(CMAKE_OSX_ARCHITECTURES "armv7;arm64")
endif()

set(SHADER_COMPILER_HEADER_DIR include)
set(SHADER_COMPILER_SOURCE_DIR source)

set(SHADER_COMPILER_HEADERS
	${SHADER_COMPILER_HEADER_DIR}/ShaderCompiler.h)

set(SHADER_COMPILER_SOURCES
	${SHADER_COMPILER_SOURCE_DIR}/ShaderCompiler.cpp)

add_library(ShaderCompiler ${SHADER_COMPILER_LIB_TYPE} ${SHADER_COMPILER_SOURCES})
target_include_directories(ShaderCompiler PUBLIC ${SHADER_COMPILER_HEADER_DIR})
target_compile_definitions(ShaderCompiler PRIVATE SHADER_COMPILER_IMPL)
if(SHADER_COMPILER_SHARED_LIB)
	target_compile_definitions(ShaderCompiler PUBLIC SHADER_COMPILER_SHARED_LIB)
endif()

target_link_libraries(ShaderCompiler PRIVATE glslang SPIRV SPIRV-Tools-opt spirv-cross-glsl)

install(TARGETS ShaderCompiler
	RUNTIME DESTINATION bin
	LIBRARY DESTINATION lib
	ARCHIVE DESTINATION lib)
install(FILES ${SHADER_COMPILER_HEADERS} DESTINATION include)
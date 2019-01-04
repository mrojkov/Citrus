cmake_minimum_required(VERSION 3.12)

project(ShaderCompiler)

add_subdirectory(external EXCLUDE_FROM_ALL)

option(SHADER_COMPILER_SHARED_LIB "Build Shared Libraries" OFF)

set(SHADER_COMPILER_LIB_TYPE STATIC)
if(SHADER_COMPILER_SHARED_LIB)
set(SHADER_COMPILER_LIB_TYPE SHARED)
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

target_link_libraries(ShaderCompiler PRIVATE glslang SPIRV spirv-cross-glsl)

install(TARGETS ShaderCompiler
	RUNTIME DESTINATION bin
	LIBRARY DESTINATION lib
	ARCHIVE DESTINATION lib)
install(FILES ${SHADER_COMPILER_HEADERS} DESTINATION include)
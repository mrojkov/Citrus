set(ENABLE_HLSL OFF CACHE INTERNAL "" FORCE)
set(SPIRV_SKIP_TESTS ON CACHE INTERNAL "" FORCE)

add_subdirectory(glslang)
add_subdirectory(SPIRV-Tools)
add_subdirectory(SPIRV-Cross)
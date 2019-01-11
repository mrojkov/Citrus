#ifndef __SHADER_COMPILER_H__
#define __SHADER_COMPILER_H__

#if defined(WIN32)
#	define C_DECL __cdecl
#	define EXPORT __declspec(dllexport)
#	define IMPORT __declspec(dllimport)
#else
#	define C_DECL
#	define EXPORT __attribute__((visibility("default")))
#	define IMPORT
#endif

#if defined(__cplusplus)
#	define EXTERN_C extern "C"
#else
#	define EXTERN_C
#endif

#if defined(SHADER_COMPILER_SHARED_LIB)
#	if defined(SHADER_COMPILER_IMPL)
#		define API EXTERN_C EXPORT
#	else
#		define API EXTERN_C IMPORT
#	endif
#else
#	define API
#endif

typedef enum
{
	SHADER_STAGE_UNDEFINED,
	SHADER_STAGE_VERTEX,
	SHADER_STAGE_FRAGMENT
} ShaderStage;

typedef enum
{
	SHADER_VARIABLE_TYPE_UNKNOWN,
	SHADER_VARIABLE_TYPE_BOOL,
	SHADER_VARIABLE_TYPE_BOOL_VECTOR2,
	SHADER_VARIABLE_TYPE_BOOL_VECTOR3,
	SHADER_VARIABLE_TYPE_BOOL_VECTOR4,
	SHADER_VARIABLE_TYPE_INT,
	SHADER_VARIABLE_TYPE_INT_VECTOR2,
	SHADER_VARIABLE_TYPE_INT_VECTOR3,
	SHADER_VARIABLE_TYPE_INT_VECTOR4,
	SHADER_VARIABLE_TYPE_FLOAT,
	SHADER_VARIABLE_TYPE_FLOAT_VECTOR2,
	SHADER_VARIABLE_TYPE_FLOAT_VECTOR3,
	SHADER_VARIABLE_TYPE_FLOAT_VECTOR4,
	SHADER_VARIABLE_TYPE_FLOAT_MATRIX2,
	SHADER_VARIABLE_TYPE_FLOAT_MATRIX3,
	SHADER_VARIABLE_TYPE_FLOAT_MATRIX4,
	SHADER_VARIABLE_TYPE_SAMPLER_2D,
	SHADER_VARIABLE_TYPE_SAMPLER_CUBE
} ShaderVariableType;

typedef void* ShaderHandle;
typedef void* ProgramHandle;

API ShaderHandle C_DECL CreateShader();
API bool C_DECL CompileShader(ShaderHandle shaderHandle, ShaderStage stage, const char* source);
API const char* C_DECL GetShaderInfoLog(ShaderHandle shaderHandle);
API void C_DECL DestroyShader(ShaderHandle shaderHandle);

API ProgramHandle C_DECL CreateProgram();
API void C_DECL BindAttribLocation(ProgramHandle programHandle, const char* name, int location);
API bool C_DECL LinkProgram(ProgramHandle programHandle, ShaderHandle vsHandle, ShaderHandle fsHandle);
API const char* C_DECL GetProgramInfoLog(ProgramHandle programHandle);
API unsigned int C_DECL GetSpvSize(ProgramHandle programHandle, ShaderStage stage);
API const unsigned int* C_DECL GetSpv(ProgramHandle programHandle, ShaderStage stage);
API int C_DECL GetActiveAttribCount(ProgramHandle programHandle);
API const char* C_DECL GetActiveAttribName(ProgramHandle programHandle, int index);
API ShaderVariableType C_DECL GetActiveAttribType(ProgramHandle programHandle, int index);
API int C_DECL GetActiveAttribLocation(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformBlockCount(ProgramHandle programHandle);
API int C_DECL GetActiveUniformBlockBinding(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformBlockSize(ProgramHandle programHandle, int index);
API ShaderStage C_DECL GetActiveUniformBlockStage(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformCount(ProgramHandle programHandle);
API const char* C_DECL GetActiveUniformName(ProgramHandle programHandle, int index);
API ShaderVariableType C_DECL GetActiveUniformType(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformArraySize(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformArrayStride(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformMatrixStride(ProgramHandle programHandle, int index);
API ShaderStage C_DECL GetActiveUniformStage(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformBinding(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformBlockIndex(ProgramHandle programHandle, int index);
API int C_DECL GetActiveUniformBlockOffset(ProgramHandle programHandle, int index);
API void C_DECL DestroyProgram(ProgramHandle programHandle);

#endif
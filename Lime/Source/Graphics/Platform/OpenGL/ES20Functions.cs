#if iOS || MAC || ANDROID
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Lime.Graphics.Platform.OpenGL
{
	public sealed class GL
	{
#if iOS
		const string library = "__Internal";
#elif MAC
		const string library = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif ANDROID
		const string library = "libGLESv2";
#endif

		internal static class Core
		{
			[DllImport(library, EntryPoint = "glActiveShaderProgramEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ActiveShaderProgramEXT(uint pipeline, uint program);

			[DllImport(library, EntryPoint = "glActiveTexture", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ActiveTexture(TextureUnit texture);

			[DllImport(library, EntryPoint = "glAttachShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void AttachShader(uint program, uint shader);

			[DllImport(library, EntryPoint = "glBeginQueryEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BeginQueryEXT(All target, uint id);

			[DllImport(library, EntryPoint = "glBindAttribLocation", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindAttribLocation(uint program, uint index, string name);
			[DllImport(library, EntryPoint = "glBindBuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindBuffer(BufferTarget target, uint buffer);

			[DllImport(library, EntryPoint = "glBindFramebuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindFramebuffer(FramebufferTarget target, uint framebuffer);

			[DllImport(library, EntryPoint = "glBindProgramPipelineEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindProgramPipelineEXT(uint pipeline);

			[DllImport(library, EntryPoint = "glBindRenderbuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer);

			[DllImport(library, EntryPoint = "glBindTexture", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindTexture(TextureTarget target, uint texture);

			[DllImport(library, EntryPoint = "glBindVertexArrayOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BindVertexArrayOES(uint array);

			[DllImport(library, EntryPoint = "glBlendColor", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BlendColor(float red, float green, float blue, float alpha);

			[DllImport(library, EntryPoint = "glBlendEquation", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BlendEquation(BlendEquationMode mode);

			[DllImport(library, EntryPoint = "glBlendEquationSeparate", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BlendEquationSeparate(BlendEquationMode modeRGB, BlendEquationMode modeAlpha);

			[DllImport(library, EntryPoint = "glBlendFunc", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BlendFunc(BlendingFactorSrc sfactor, BlendingFactorDest dfactor);

			[DllImport(library, EntryPoint = "glBlendFuncSeparate", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BlendFuncSeparate(BlendingFactorSrc srcRGB, BlendingFactorDest dstRGB, BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha);

			[DllImport(library, EntryPoint = "glBufferData", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BufferData(BufferTarget target, IntPtr size, IntPtr data, BufferUsageHint usage);

			[DllImport(library, EntryPoint = "glBufferSubData", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void BufferSubData(BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);

			[DllImport(library, EntryPoint = "glCheckFramebufferStatus", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target);

			[DllImport(library, EntryPoint = "glClear", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Clear(ClearBufferMask mask);

			[DllImport(library, EntryPoint = "glClearColor", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ClearColor(float red, float green, float blue, float alpha);

#if MAC
			[DllImport(library, EntryPoint = "glClearDepth", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ClearDepth(double depth);
#else
			[DllImport(library, EntryPoint = "glClearDepthf", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ClearDepth(float depth);
#endif

			[DllImport(library, EntryPoint = "glClearStencil", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ClearStencil(int s);

			[DllImport(library, EntryPoint = "glClientWaitSyncAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern All ClientWaitSyncAPPLE(IntPtr sync, uint flags, ulong timeout);

			[DllImport(library, EntryPoint = "glColorMask", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ColorMask(bool red, bool green, bool blue, bool alpha);

			[DllImport(library, EntryPoint = "glCompileShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CompileShader(uint shader);

			[DllImport(library, EntryPoint = "glCompressedTexImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CompressedTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data);

			[DllImport(library, EntryPoint = "glCompressedTexSubImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CompressedTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, int imageSize, IntPtr data);

			[DllImport(library, EntryPoint = "glCopyTexImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CopyTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int x, int y, int width, int height, int border);

			[DllImport(library, EntryPoint = "glCopyTexSubImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CopyTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int x, int y, int width, int height);

			[DllImport(library, EntryPoint = "glCopyTextureLevelsAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CopyTextureLevelsAPPLE(uint destinationTexture, uint sourceTexture, int sourceBaseLevel, int sourceLevelCount);

			[DllImport(library, EntryPoint = "glCreateProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern uint CreateProgram();

			[DllImport(library, EntryPoint = "glCreateShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern uint CreateShader(ShaderType type);

			[DllImport(library, EntryPoint = "glCreateShaderProgramvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern int CreateShaderProgramvEXT(All type, int count, string strings);

			[DllImport(library, EntryPoint = "glCullFace", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void CullFace(CullFaceMode mode);

			[DllImport(library, EntryPoint = "glDeleteBuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteBuffers(int n, int* buffers);

			[DllImport(library, EntryPoint = "glDeleteFramebuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteFramebuffers(int n, int* framebuffers);

			[DllImport(library, EntryPoint = "glDeleteProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DeleteProgram(uint program);

			[DllImport(library, EntryPoint = "glDeleteProgramPipelinesEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteProgramPipelinesEXT(int n, uint* pipelines);

			[DllImport(library, EntryPoint = "glDeleteQueriesEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteQueriesEXT(int n, uint* ids);

			[DllImport(library, EntryPoint = "glDeleteRenderbuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteRenderbuffers(int n, int* renderbuffers);

			[DllImport(library, EntryPoint = "glDeleteShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DeleteShader(uint shader);

			[DllImport(library, EntryPoint = "glDeleteSyncAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DeleteSyncAPPLE(IntPtr sync);

			[DllImport(library, EntryPoint = "glDeleteTextures", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteTextures(int n, int* textures);

			[DllImport(library, EntryPoint = "glDeleteVertexArraysOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DeleteVertexArraysOES(int n, uint* arrays);

			[DllImport(library, EntryPoint = "glDepthFunc", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DepthFunc(DepthFunction func);

			[DllImport(library, EntryPoint = "glDepthMask", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DepthMask(bool flag);

#if MAC
			[DllImport(library, EntryPoint = "glDepthRange", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DepthRange(double zNear, double zFar);
#else
			[DllImport(library, EntryPoint = "glDepthRangef", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DepthRange(float zNear, float zFar);
#endif

			[DllImport(library, EntryPoint = "glDetachShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DetachShader(uint program, uint shader);

			[DllImport(library, EntryPoint = "glDisable", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Disable(EnableCap cap);

			[DllImport(library, EntryPoint = "glDisableVertexAttribArray", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DisableVertexAttribArray(uint index);

			[DllImport(library, EntryPoint = "glDiscardFramebufferEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void DiscardFramebufferEXT(All target, int numAttachments, All* attachments);

			[DllImport(library, EntryPoint = "glDrawArrays", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DrawArrays(PrimitiveType mode, int first, int count);

			[DllImport(library, EntryPoint = "glDrawArraysInstancedEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DrawArraysInstancedEXT(All mode, int first, int count, int instanceCount);

			[DllImport(library, EntryPoint = "glDrawElements", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DrawElements(PrimitiveType mode, int count, DrawElementsType type, IntPtr indices);

			[DllImport(library, EntryPoint = "glDrawElementsInstancedEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void DrawElementsInstancedEXT(All mode, int count, All type, IntPtr indices, int instanceCount);

			[DllImport(library, EntryPoint = "glEnable", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Enable(EnableCap cap);

			[DllImport(library, EntryPoint = "glEnableVertexAttribArray", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void EnableVertexAttribArray(uint index);

			[DllImport(library, EntryPoint = "glEndQueryEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void EndQueryEXT(All target);

			[DllImport(library, EntryPoint = "glFenceSyncAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern IntPtr FenceSyncAPPLE(All condition, uint flags);

			[DllImport(library, EntryPoint = "glFinish", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Finish();

			[DllImport(library, EntryPoint = "glFlush", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Flush();

			[DllImport(library, EntryPoint = "glFlushMappedBufferRangeEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void FlushMappedBufferRangeEXT(All target, IntPtr offset, IntPtr length);

			[DllImport(library, EntryPoint = "glFramebufferRenderbuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void FramebufferRenderbuffer(FramebufferTarget target, FramebufferSlot attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer);

			[DllImport(library, EntryPoint = "glFramebufferTexture2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void FramebufferTexture2D(FramebufferTarget target, FramebufferSlot attachment, TextureTarget textarget, int texture, int level);

			[DllImport(library, EntryPoint = "glFrontFace", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void FrontFace(FrontFaceDirection mode);

			[DllImport(library, EntryPoint = "glGenBuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenBuffers(int n, [Out] int* buffers);

			[DllImport(library, EntryPoint = "glGenerateMipmap", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void GenerateMipmap(TextureTarget target);

			[DllImport(library, EntryPoint = "glGenFramebuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenFramebuffers(int n, [Out] int* framebuffers);

			[DllImport(library, EntryPoint = "glGenProgramPipelinesEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenProgramPipelinesEXT(int n, [Out] uint* pipelines);

			[DllImport(library, EntryPoint = "glGenQueriesEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenQueriesEXT(int n, [Out] uint* ids);

			[DllImport(library, EntryPoint = "glGenRenderbuffers", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenRenderbuffers(int n, [Out] int* renderbuffers);

			[DllImport(library, EntryPoint = "glGenTextures", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenTextures(int n, [Out] int* textures);

			[DllImport(library, EntryPoint = "glGenVertexArraysOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GenVertexArraysOES(int n, [Out] uint* arrays);

			[DllImport(library, EntryPoint = "glGetActiveAttrib", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetActiveAttrib(uint program, uint index, int bufsize, [Out] int* length, [Out] int* size, [Out] ActiveAttribType* type, [Out] StringBuilder name);

			[DllImport(library, EntryPoint = "glGetActiveUniform", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetActiveUniform(uint program, uint index, int bufsize, [Out] int* length, [Out] int* size, [Out] ActiveUniformType* type, [Out] StringBuilder name);

			[DllImport(library, EntryPoint = "glGetAttachedShaders", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetAttachedShaders(uint program, int maxcount, [Out] int* count, [Out] uint* shaders);

			[DllImport(library, EntryPoint = "glGetAttribLocation", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern int GetAttribLocation(uint program, string name);

			[DllImport(library, EntryPoint = "glGetBooleanv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetBooleanv(GetPName pname, [Out] bool* @params);

			[DllImport(library, EntryPoint = "glGetBufferParameteriv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetBufferParameteriv(BufferTarget target, BufferParameterName pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetBufferPointervOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void GetBufferPointervOES(All target, All pname, [Out] IntPtr @params);

			[DllImport(library, EntryPoint = "glGetError", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern ErrorCode GetError();

			[DllImport(library, EntryPoint = "glGetFloatv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetFloatv(GetPName pname, [Out] float* @params);

			[DllImport(library, EntryPoint = "glGetFramebufferAttachmentParameteriv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetFramebufferAttachmentParameteriv(FramebufferTarget target, FramebufferSlot attachment, FramebufferParameterName pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetInteger64vAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetInteger64vAPPLE(All pname, [Out] long* @params);

			[DllImport(library, EntryPoint = "glGetIntegerv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetIntegerv(GetPName pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetObjectLabelEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetObjectLabelEXT(All type, uint @object, int bufSize, [Out] int* length, [Out] StringBuilder label);

			[DllImport(library, EntryPoint = "glGetProgramInfoLog", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetProgramInfoLog(int program, int bufsize, [Out] int* length, [Out] StringBuilder infolog);

			[DllImport(library, EntryPoint = "glGetProgramiv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetProgramiv(int program, ProgramParameter pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetProgramPipelineInfoLogEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetProgramPipelineInfoLogEXT(uint pipeline, int bufSize, [Out] int* length, [Out] StringBuilder infoLog);

			[DllImport(library, EntryPoint = "glGetProgramPipelineivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetProgramPipelineivEXT(uint pipeline, All pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetQueryivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetQueryivEXT(All target, All pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetQueryObjectuivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetQueryObjectuivEXT(uint id, All pname, [Out] uint* @params);

			[DllImport(library, EntryPoint = "glGetRenderbufferParameteriv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetRenderbufferParameteriv(RenderbufferTarget target, RenderbufferParameterName pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetShaderInfoLog", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetShaderInfoLog(int shader, int bufsize, [Out] int* length, [Out] StringBuilder infolog);

			[DllImport(library, EntryPoint = "glGetShaderiv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetShaderiv(int shader, ShaderParameter pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetShaderPrecisionFormat", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetShaderPrecisionFormat(ShaderType shadertype, ShaderPrecision precisiontype, [Out] int* range, [Out] int* precision);

			[DllImport(library, EntryPoint = "glGetShaderSource", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetShaderSource(uint shader, int bufsize, [Out] int* length, [Out] StringBuilder source);

			[DllImport(library, EntryPoint = "glGetString", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern IntPtr GetString(StringName name);

			[DllImport(library, EntryPoint = "glGetSyncivAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetSyncivAPPLE(IntPtr sync, All pname, int bufSize, [Out] int* length, [Out] int* values);

			[DllImport(library, EntryPoint = "glGetTexParameterfv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetTexParameterfv(TextureTarget target, GetTextureParameter pname, [Out] float* @params);

			[DllImport(library, EntryPoint = "glGetTexParameteriv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetTexParameteriv(TextureTarget target, GetTextureParameter pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetUniformfv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetUniformfv(uint program, int location, [Out] float* @params);

			[DllImport(library, EntryPoint = "glGetUniformiv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetUniformiv(uint program, int location, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetUniformLocation", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern int GetUniformLocation(uint program, string name);

			[DllImport(library, EntryPoint = "glGetVertexAttribfv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetVertexAttribfv(uint index, VertexAttribParameter pname, [Out] float* @params);

			[DllImport(library, EntryPoint = "glGetVertexAttribiv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void GetVertexAttribiv(uint index, VertexAttribParameter pname, [Out] int* @params);

			[DllImport(library, EntryPoint = "glGetVertexAttribPointerv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void GetVertexAttribPointerv(uint index, VertexAttribPointerParameter pname, [Out] IntPtr pointer);

			[DllImport(library, EntryPoint = "glHint", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Hint(HintTarget target, HintMode mode);

			[DllImport(library, EntryPoint = "glInsertEventMarkerEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void InsertEventMarkerEXT(int length, string marker);

			[DllImport(library, EntryPoint = "glIsBuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsBuffer(uint buffer);

			[DllImport(library, EntryPoint = "glIsEnabled", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsEnabled(EnableCap cap);

			[DllImport(library, EntryPoint = "glIsFramebuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsFramebuffer(uint framebuffer);

			[DllImport(library, EntryPoint = "glIsProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsProgram(uint program);

			[DllImport(library, EntryPoint = "glIsProgramPipelineEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsProgramPipelineEXT(uint pipeline);

			[DllImport(library, EntryPoint = "glIsQueryEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsQueryEXT(uint id);

			[DllImport(library, EntryPoint = "glIsRenderbuffer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsRenderbuffer(uint renderbuffer);

			[DllImport(library, EntryPoint = "glIsShader", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsShader(uint shader);

			[DllImport(library, EntryPoint = "glIsSyncAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsSyncAPPLE(IntPtr sync);

			[DllImport(library, EntryPoint = "glIsTexture", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsTexture(uint texture);

			[DllImport(library, EntryPoint = "glIsVertexArrayOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool IsVertexArrayOES(uint array);

			[DllImport(library, EntryPoint = "glLabelObjectEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void LabelObjectEXT(All type, uint @object, int length, string label);

			[DllImport(library, EntryPoint = "glLineWidth", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void LineWidth(float width);

			[DllImport(library, EntryPoint = "glLinkProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void LinkProgram(uint program);

			[DllImport(library, EntryPoint = "glMapBufferOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern IntPtr MapBufferOES(All target, All access);

			[DllImport(library, EntryPoint = "glMapBufferRangeEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern IntPtr MapBufferRangeEXT(All target, IntPtr offset, IntPtr length, uint access);

			[DllImport(library, EntryPoint = "glPixelStorei", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void PixelStorei(PixelStoreParameter pname, int param);

			[DllImport(library, EntryPoint = "glPolygonOffset", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void PolygonOffset(float factor, float units);

			[DllImport(library, EntryPoint = "glPopGroupMarkerEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void PopGroupMarkerEXT();

			[DllImport(library, EntryPoint = "glProgramParameteriEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramParameteriEXT(uint program, All pname, int value);

			[DllImport(library, EntryPoint = "glProgramUniform1fEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform1fEXT(uint program, int location, float x);

			[DllImport(library, EntryPoint = "glProgramUniform1fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform1fvEXT(uint program, int location, int count, float* value);

			[DllImport(library, EntryPoint = "glProgramUniform1iEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform1iEXT(uint program, int location, int x);

			[DllImport(library, EntryPoint = "glProgramUniform1ivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform1ivEXT(uint program, int location, int count, int* value);

			[DllImport(library, EntryPoint = "glProgramUniform2fEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform2fEXT(uint program, int location, float x, float y);

			[DllImport(library, EntryPoint = "glProgramUniform2fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform2fvEXT(uint program, int location, int count, float* value);

			[DllImport(library, EntryPoint = "glProgramUniform2iEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform2iEXT(uint program, int location, int x, int y);

			[DllImport(library, EntryPoint = "glProgramUniform2ivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform2ivEXT(uint program, int location, int count, int* value);

			[DllImport(library, EntryPoint = "glProgramUniform3fEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform3fEXT(uint program, int location, float x, float y, float z);

			[DllImport(library, EntryPoint = "glProgramUniform3fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform3fvEXT(uint program, int location, int count, float* value);

			[DllImport(library, EntryPoint = "glProgramUniform3iEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform3iEXT(uint program, int location, int x, int y, int z);

			[DllImport(library, EntryPoint = "glProgramUniform3ivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform3ivEXT(uint program, int location, int count, int* value);

			[DllImport(library, EntryPoint = "glProgramUniform4fEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform4fEXT(uint program, int location, float x, float y, float z, float w);

			[DllImport(library, EntryPoint = "glProgramUniform4fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform4fvEXT(uint program, int location, int count, float* value);

			[DllImport(library, EntryPoint = "glProgramUniform4iEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ProgramUniform4iEXT(uint program, int location, int x, int y, int z, int w);

			[DllImport(library, EntryPoint = "glProgramUniform4ivEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniform4ivEXT(uint program, int location, int count, int* value);

			[DllImport(library, EntryPoint = "glProgramUniformMatrix2fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniformMatrix2fvEXT(uint program, int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glProgramUniformMatrix3fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniformMatrix3fvEXT(uint program, int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glProgramUniformMatrix4fvEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ProgramUniformMatrix4fvEXT(uint program, int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glPushGroupMarkerEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void PushGroupMarkerEXT(int length, string marker);

			[DllImport(library, EntryPoint = "glReadPixels", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr pixels);

			[DllImport(library, EntryPoint = "glReleaseShaderCompiler", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ReleaseShaderCompiler();

			[DllImport(library, EntryPoint = "glRenderbufferStorage", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void RenderbufferStorage(RenderbufferTarget target, RenderbufferInternalFormat internalformat, int width, int height);

			[DllImport(library, EntryPoint = "glRenderbufferStorageMultisampleAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void RenderbufferStorageMultisampleAPPLE(All target, int samples, All internalformat, int width, int height);

			[DllImport(library, EntryPoint = "glResolveMultisampleFramebufferAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ResolveMultisampleFramebufferAPPLE();

			[DllImport(library, EntryPoint = "glSampleCoverage", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void SampleCoverage(float value, bool invert);

			[DllImport(library, EntryPoint = "glScissor", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Scissor(int x, int y, int width, int height);

			[DllImport(library, EntryPoint = "glShaderSource", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void ShaderSource(int shader, int count, string[] @string, int* length);

			[DllImport(library, EntryPoint = "glStencilFunc", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilFunc(StencilFunction func, int @ref, uint mask);

			[DllImport(library, EntryPoint = "glStencilFuncSeparate", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilFuncSeparate(StencilFace face, StencilFunction func, int @ref, uint mask);

			[DllImport(library, EntryPoint = "glStencilMask", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilMask(uint mask);

			[DllImport(library, EntryPoint = "glStencilMaskSeparate", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilMaskSeparate(CullFaceMode face, uint mask);

			[DllImport(library, EntryPoint = "glStencilOp", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass);

			[DllImport(library, EntryPoint = "glStencilOpSeparate", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void StencilOpSeparate(StencilFace face, StencilOp fail, StencilOp zfail, StencilOp zpass);

			[DllImport(library, EntryPoint = "glTexImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels);

			[DllImport(library, EntryPoint = "glTexParameterf", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void TexParameterf(TextureTarget target, TextureParameterName pname, float param);

			[DllImport(library, EntryPoint = "glTexParameterfv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void TexParameterfv(TextureTarget target, TextureParameterName pname, float* @params);

			[DllImport(library, EntryPoint = "glTexParameteri", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void TexParameteri(TextureTarget target, TextureParameterName pname, int param);

			[DllImport(library, EntryPoint = "glTexParameteriv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void TexParameteriv(TextureTarget target, TextureParameterName pname, int* @params);

			[DllImport(library, EntryPoint = "glTexStorage2DEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void TexStorage2DEXT(All target, int levels, All internalformat, int width, int height);

			[DllImport(library, EntryPoint = "glTexSubImage2D", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels);

			[DllImport(library, EntryPoint = "glUniform1f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform1f(int location, float x);

			[DllImport(library, EntryPoint = "glUniform1fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform1fv(int location, int count, float* v);

			[DllImport(library, EntryPoint = "glUniform1i", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform1i(int location, int x);

			[DllImport(library, EntryPoint = "glUniform1iv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform1iv(int location, int count, int* v);

			[DllImport(library, EntryPoint = "glUniform2f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform2f(int location, float x, float y);

			[DllImport(library, EntryPoint = "glUniform2fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform2fv(int location, int count, float* v);

			[DllImport(library, EntryPoint = "glUniform2i", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform2i(int location, int x, int y);

			[DllImport(library, EntryPoint = "glUniform2iv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform2iv(int location, int count, int* v);

			[DllImport(library, EntryPoint = "glUniform3f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform3f(int location, float x, float y, float z);

			[DllImport(library, EntryPoint = "glUniform3fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform3fv(int location, int count, float* v);

			[DllImport(library, EntryPoint = "glUniform3i", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform3i(int location, int x, int y, int z);

			[DllImport(library, EntryPoint = "glUniform3iv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform3iv(int location, int count, int* v);

			[DllImport(library, EntryPoint = "glUniform4f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform4f(int location, float x, float y, float z, float w);

			[DllImport(library, EntryPoint = "glUniform4fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform4fv(int location, int count, float* v);

			[DllImport(library, EntryPoint = "glUniform4i", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Uniform4i(int location, int x, int y, int z, int w);

			[DllImport(library, EntryPoint = "glUniform4iv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void Uniform4iv(int location, int count, int* v);

			[DllImport(library, EntryPoint = "glUniformMatrix2fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void UniformMatrix2fv(int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glUniformMatrix3fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void UniformMatrix3fv(int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glUniformMatrix4fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void UniformMatrix4fv(int location, int count, bool transpose, float* value);

			[DllImport(library, EntryPoint = "glUnmapBufferOES", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern bool UnmapBufferOES(All target);

			[DllImport(library, EntryPoint = "glUseProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void UseProgram(uint program);

			[DllImport(library, EntryPoint = "glUseProgramStagesEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void UseProgramStagesEXT(uint pipeline, uint stages, uint program);

			[DllImport(library, EntryPoint = "glValidateProgram", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ValidateProgram(uint program);

			[DllImport(library, EntryPoint = "glValidateProgramPipelineEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void ValidateProgramPipelineEXT(uint pipeline);

			[DllImport(library, EntryPoint = "glVertexAttrib1f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttrib1f(uint indx, float x);

			[DllImport(library, EntryPoint = "glVertexAttrib1fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void VertexAttrib1fv(uint indx, float* values);

			[DllImport(library, EntryPoint = "glVertexAttrib2f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttrib2f(uint indx, float x, float y);

			[DllImport(library, EntryPoint = "glVertexAttrib2fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void VertexAttrib2fv(uint indx, float* values);

			[DllImport(library, EntryPoint = "glVertexAttrib3f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttrib3f(uint indx, float x, float y, float z);

			[DllImport(library, EntryPoint = "glVertexAttrib3fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void VertexAttrib3fv(uint indx, float* values);

			[DllImport(library, EntryPoint = "glVertexAttrib4f", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttrib4f(uint indx, float x, float y, float z, float w);

			[DllImport(library, EntryPoint = "glVertexAttrib4fv", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal unsafe static extern void VertexAttrib4fv(uint indx, float* values);

			[DllImport(library, EntryPoint = "glVertexAttribDivisorEXT", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttribDivisorEXT(uint index, uint divisor);

			[DllImport(library, EntryPoint = "glVertexAttribPointer", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void VertexAttribPointer(int indx, int size, VertexAttribPointerType type, bool normalized, int stride, IntPtr ptr);

			[DllImport(library, EntryPoint = "glViewport", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void Viewport(int x, int y, int width, int height);

			[DllImport(library, EntryPoint = "glWaitSyncAPPLE", ExactSpelling = true)]
			[SuppressUnmanagedCodeSecurity]
			internal static extern void WaitSyncAPPLE(IntPtr sync, uint flags, ulong timeout);
		}

		public static void BlendEquationSeparate(BlendEquationMode modeRGB, BlendEquationMode modeAlpha)
		{
			Core.BlendEquationSeparate(modeRGB, modeAlpha);
		}

		public static void Clear(ClearBufferMask mask)
		{
			Core.Clear(mask);
		}

		public static void Enable(EnableCap cap)
		{
			Core.Enable(cap);
		}
		
		public unsafe static void GetInteger(GetPName pname, out int @params)
		{
			fixed (int* ptr = &@params) {
				Core.GetIntegerv(pname, ptr);
			}
		}

		public static void ActiveTexture(TextureUnit texture)
		{
			Core.ActiveTexture(texture);
		}

		public static void AttachShader(int program, int shader)
		{
			Core.AttachShader((uint)program,(uint)shader);
		}

		public static void BindAttribLocation(int program, int index, string name)
		{
			Core.BindAttribLocation((uint)program,(uint)index, name);
		}

		public static void BindBuffer(BufferTarget target, int buffer)
		{
			Core.BindBuffer(target,(uint)buffer);
		}

		public static void BindBuffer(BufferTarget target, uint buffer)
		{
			Core.BindBuffer(target, buffer);
		}

		public static void BindFramebuffer(FramebufferTarget target, int framebuffer)
		{
			Core.BindFramebuffer(target,(uint)framebuffer);
		}

		public static void BindFramebuffer(FramebufferTarget target, uint framebuffer)
		{
			Core.BindFramebuffer(target, framebuffer);
		}

		public static void BindRenderbuffer(RenderbufferTarget target, int renderbuffer)
		{
			Core.BindRenderbuffer(target,(uint)renderbuffer);
		}

		public static void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer)
		{
			Core.BindRenderbuffer(target, renderbuffer);
		}

		public static void BindTexture(TextureTarget target, uint texture)
		{
			Core.BindTexture(target, texture);
		}

		public static void BindTexture(TextureTarget target, int texture)
		{
			Core.BindTexture(target,(uint)texture);
		}

		public static void BlendColor(float red, float green, float blue, float alpha)
		{
			Core.BlendColor(red, green, blue, alpha);
		}

		public static void BlendFuncSeparate(BlendingFactorSrc srcRGB, BlendingFactorDest dstRGB, BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha)
		{
			Core.BlendFuncSeparate(srcRGB, dstRGB, srcAlpha, dstAlpha);
		}

		public static void BufferData(BufferTarget target, int size, IntPtr data, BufferUsageHint usage)
		{
			Core.BufferData(target, (IntPtr)size, data, usage);
		}

		public static void BufferSubData(BufferTarget target, IntPtr offset, int size, IntPtr data)
		{
			Core.BufferSubData(target, offset, (IntPtr)size, data);
		}

		public static FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target)
		{
			return Core.CheckFramebufferStatus(target);
		}

		public static void ClearColor(float red, float green, float blue, float alpha)
		{
			Core.ClearColor(red, green, blue, alpha);
		}

		public static void ClearDepth(float depth)
		{
			Core.ClearDepth(depth);
		}

		public static void ClearStencil(int s)
		{
			Core.ClearStencil(s);
		}

		public static void ColorMask(bool red, bool green, bool blue, bool alpha)
		{
			Core.ColorMask(red, green, blue, alpha);
		}

		public static void CompileShader(int shader)
		{
			Core.CompileShader((uint)shader);
		}

		public static void CompressedTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data)
		{
			Core.CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data);
		}

		public static int CreateProgram()
		{
			return(int)Core.CreateProgram();
		}

		public static int CreateShader(ShaderType type)
		{
			return(int)Core.CreateShader(type);
		}

		public static void CullFace(CullFaceMode mode)
		{
			Core.CullFace(mode);
		}

		public unsafe static void DeleteBuffer(int buffer)
		{
			Core.DeleteBuffers(1, &buffer);
		}

		public unsafe static void DeleteFramebuffer(int framebuffer)
		{
			Core.DeleteFramebuffers(1, &framebuffer);
		}

		public static void DeleteProgram(int program)
		{
			Core.DeleteProgram((uint)program);
		}

		public unsafe static void DeleteRenderbuffer(int renderbuffer)
		{
			Core.DeleteRenderbuffers(1, &renderbuffer);
		}

		public static void DeleteShader(int shader)
		{
			Core.DeleteShader((uint)shader);
		}

		public unsafe static void DeleteTexture(int texture)
		{
			Core.DeleteTextures(1, &texture);
		}

		public static void DepthFunc(DepthFunction func)
		{
			Core.DepthFunc(func);
		}

		public static void DepthMask(bool flag)
		{
			Core.DepthMask(flag);
		}

		public static void DepthRange(float zNear, float zFar)
		{
			Core.DepthRange(zNear, zFar);
		}

		public static void Disable(EnableCap cap)
		{
			Core.Disable(cap);
		}

		public static void DisableVertexAttribArray(int index)
		{
			Core.DisableVertexAttribArray((uint)index);
		}

		public static void DrawArrays(PrimitiveType mode, int first, int count)
		{
			Core.DrawArrays(mode, first, count);
		}

		public static void DrawElements(PrimitiveType mode, int count, DrawElementsType type, IntPtr indices)
		{
			Core.DrawElements(mode, count, type, indices);
		}

		public static void EnableVertexAttribArray(int index)
		{
			Core.EnableVertexAttribArray((uint)index);
		}

		public static void Finish()
		{
			Core.Finish();
		}

		public static void Flush()
		{
			Core.Flush();
		}

		public static void FramebufferRenderbuffer(FramebufferTarget target, FramebufferSlot attachment, RenderbufferTarget renderbuffertarget, int renderbuffer)
		{
			Core.FramebufferRenderbuffer(target, attachment, renderbuffertarget,(uint)renderbuffer);
		}

		public static void FramebufferRenderbuffer(FramebufferTarget target, FramebufferSlot attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer)
		{
			Core.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
		}

		public static void FramebufferTexture2D(FramebufferTarget target, FramebufferSlot attachment, TextureTarget textarget, int texture, int level)
		{
			Core.FramebufferTexture2D(target, attachment, textarget, texture, level);
		}

		public static void FrontFace(FrontFaceDirection mode)
		{
			Core.FrontFace(mode);
		}

		public unsafe static int GenBuffer()
		{
			int result;
			Core.GenBuffers(1, &result);
			return result;
		}

		public unsafe static int GenFramebuffer()
		{
			int result;
			Core.GenFramebuffers(1, &result);
			return result;
		}

		public unsafe static int GenRenderbuffer()
		{
			int result;
			Core.GenRenderbuffers(1, &result);
			return result;
		}

		public unsafe static int GenTexture()
		{
			int result;
			Core.GenTextures(1, &result);
			return result;
		}

		public unsafe static void GetActiveUniform(int program, int index, int bufsize, out int length, out int size, out ActiveUniformType type, [Out] StringBuilder name)
		{
			fixed (int* ptr = &length) {
				int* ptr2 = ptr;
				fixed (int* ptr3 = &size) {
					int* ptr4 = ptr3;
					fixed (ActiveUniformType* ptr5 = &type) {
						ActiveUniformType* ptr6 = ptr5;
						Core.GetActiveUniform((uint)program,(uint)index, bufsize, ptr2, ptr4, ptr6, name);
						length = *ptr2;
						size = *ptr4;
						type = *ptr6;
					}
				}
			}
		}

		public static ErrorCode GetError()
		{
			return Core.GetError();
		}

		public unsafe static string GetProgramInfoLog(int program)
		{
			int length;
			Core.GetProgramiv(program, ProgramParameter.InfoLogLength, &length);
			var infolog = new StringBuilder();
			Core.GetProgramInfoLog(program, length, null, infolog);
			return infolog.ToString();
		}

		public unsafe static void GetProgram(int program, ProgramParameter pname, out int @params)
		{
			fixed (int* ptr = &@params) {
				Core.GetProgramiv(program, pname, ptr);
				@params = *ptr;
			}
		}

		public unsafe static void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, out int @params)
		{
			fixed (int* ptr = &@params) {
				Core.GetRenderbufferParameteriv(target, pname, ptr);
				@params = *ptr;
			}
		}

		public unsafe static string GetShaderInfoLog(int shader)
		{
			int length;
			Core.GetShaderiv(shader, ShaderParameter.InfoLogLength, &length);
			var infolog = new StringBuilder();
			Core.GetShaderInfoLog(shader, length, null, infolog);
			return infolog.ToString();

		}

		public unsafe static void GetShader(int shader, ShaderParameter pname, out int @params)
		{
			fixed (int* ptr = &@params) {
				Core.GetShaderiv(shader, pname, ptr);
				@params = *ptr;
			}
		}

		public unsafe static string GetString(StringName name)
		{
			return new string((sbyte*)(void*)Core.GetString(name));
		}

		public static int GetUniformLocation(int program, string name)
		{
			return Core.GetUniformLocation((uint)program, name);
		}

		public static void LinkProgram(int program)
		{
			Core.LinkProgram((uint)program);
		}

		public static void PixelStore(PixelStoreParameter pname, int param)
		{
			Core.PixelStorei(pname, param);
		}

		public static void ReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
		{
			Core.ReadPixels(x, y, width, height, format, type, pixels);
		}

		public static void RenderbufferStorage(RenderbufferTarget target, RenderbufferInternalFormat internalformat, int width, int height)
		{
			Core.RenderbufferStorage(target, internalformat, width, height);
		}

		public static void Scissor(int x, int y, int width, int height)
		{
			Core.Scissor(x, y, width, height);
		}

		public unsafe static void ShaderSource(int shader, string @string)
		{
			int length = @string.Length;
			Core.ShaderSource(shader, 1, new string[] { @string }, &length);
		}

		public static void StencilFuncSeparate(StencilFace face, StencilFunction func, int @ref, int mask)
		{
			Core.StencilFuncSeparate(face, func, @ref,(uint)mask);
		}

		public static void StencilMask(int mask)
		{
			Core.StencilMask((uint)mask);
		}

		public static void StencilOpSeparate(StencilFace face, StencilOp fail, StencilOp zfail, StencilOp zpass)
		{
			Core.StencilOpSeparate(face, fail, zfail, zpass);
		}

		public static void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels)
		{
			Core.TexImage2D(target, level, internalformat, width, height, border, format, type, pixels);
		}

		public static void TexParameter(TextureTarget target, TextureParameterName pname, float param)
		{
			Core.TexParameterf(target, pname, param);
		}

		public static void CompressedTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, int imageSize, IntPtr pixels)
		{
			Core.CompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, pixels);
		}

		public static void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
		{
			Core.TexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels);
		}

		public unsafe static void Uniform1(int location, int count, float* v)
		{
			Core.Uniform1fv(location, count, v);
		}

		public static void Uniform1(int location, int x)
		{
			Core.Uniform1i(location, x);
		}

		public unsafe static void Uniform1(int location, int count, int* v)
		{
			Core.Uniform1iv(location, count, v);
		}

		public unsafe static void Uniform2(int location, int count, float* v)
		{
			Core.Uniform2fv(location, count, v);
		}

		public unsafe static void Uniform2(int location, int count, int* v)
		{
			Core.Uniform2iv(location, count, v);
		}

		public unsafe static void Uniform3(int location, int count, int* v)
		{
			Core.Uniform3iv(location, count, v);
		}

		public unsafe static void Uniform3(int location, int count, float* v)
		{
			Core.Uniform3fv(location, count, v);
		}

		public unsafe static void Uniform4(int location, int count, int* v)
		{
			Core.Uniform4iv(location, count, v);
		}

		public unsafe static void Uniform4(int location, int count, float* v)
		{
			Core.Uniform4fv(location, count, v);
		}

		public unsafe static void UniformMatrix2(int location, int count, bool transpose, float* data)
		{
			Core.UniformMatrix2fv(location, count, transpose, data);
		}

		public unsafe static void UniformMatrix3(int location, int count, bool transpose, float* data)
		{
			Core.UniformMatrix3fv(location, count, transpose, data);
		}

		public unsafe static void UniformMatrix4(int location, int count, bool transpose, float* value)
		{
			Core.UniformMatrix4fv(location, count, transpose, value);
		}

		public static void UseProgram(int program)
		{
			Core.UseProgram((uint)program);
		}

		public static void VertexAttribPointer(int indx, int size, VertexAttribPointerType type, bool normalized, int stride, int ptr)
		{
			Core.VertexAttribPointer(indx, size, type, normalized, stride, new IntPtr(ptr));
		}

		public static void Viewport(int x, int y, int width, int height)
		{
			Core.Viewport(x, y, width, height);
		}

		public static ErrorCode GetErrorCode()
		{
			return Core.GetError();
		}
	}
}
#endif
using System;
using System.Runtime.InteropServices;

namespace Lime.Graphics.Platform.Vulkan
{
	public static class ShaderCompiler
	{
#if __IOS__
		private const string LibraryName = "__Internal";
#elif __MACOS__
		private const string LibraryName = "ShaderCompiler";
#endif
		public enum Stage
		{
			Undefined,
			Vertex,
			Fragment
		}

		public enum VariableType
		{
			Unknown,
			Bool,
			BoolVector2,
			BoolVector3,
			BoolVector4,
			Int,
			IntVector2,
			IntVector3,
			IntVector4,
			Float,
			FloatVector2,
			FloatVector3,
			FloatVector4,
			FloatMatrix2,
			FloatMatrix3,
			FloatMatrix4,
			Sampler2D,
			SamplerCube
		}

		[DllImport(LibraryName, EntryPoint = "CreateShader", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateShader();

		[DllImport(LibraryName, EntryPoint = "CompileShader", CallingConvention = CallingConvention.Cdecl)]
		private static extern int CompileShaderInternal(IntPtr shaderHandle, Stage stage, IntPtr source);

		public static bool CompileShader(IntPtr shaderHandle, Stage stage, string source)
		{
			var interopSource = Marshal.StringToHGlobalAnsi(source);
			try {
				return CompileShaderInternal(shaderHandle, stage, interopSource) != 0;
			} finally {
				Marshal.FreeHGlobal(interopSource);
			}
		}

		[DllImport(LibraryName, EntryPoint = "GetShaderInfoLog", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr GetShaderInfoLogInternal(IntPtr shaderHandle);

		public static string GetShaderInfoLog(IntPtr shaderHandle)
		{
			return Marshal.PtrToStringAnsi(GetShaderInfoLogInternal(shaderHandle));
		}

		[DllImport(LibraryName, EntryPoint = "DestroyShader", CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyShader(IntPtr shaderHandle);

		[DllImport(LibraryName, EntryPoint = "CreateProgram", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateProgram();

		[DllImport(LibraryName, EntryPoint = "BindAttribLocation", CallingConvention = CallingConvention.Cdecl)]
		private static extern void BindAttribLocation(IntPtr programHandle, IntPtr name, int location);

		public static void BindAttribLocation(IntPtr programHandle, string name, int location)
		{
			var interopName = Marshal.StringToHGlobalAnsi(name);
			try {
				BindAttribLocation(programHandle, interopName, location);
			} finally {
				Marshal.FreeHGlobal(interopName);
			}
		}

		[DllImport(LibraryName, EntryPoint = "LinkProgram", CallingConvention = CallingConvention.Cdecl)]
		private static extern int LinkProgramInternal(IntPtr programHandle, IntPtr vertexShaderHandle, IntPtr fragmentShaderHandle);

		public static bool LinkProgram(IntPtr programHandle, IntPtr vertexShaderHandle, IntPtr fragmentShaderHandle)
		{
			return LinkProgramInternal(programHandle, vertexShaderHandle, fragmentShaderHandle) != 0;
		}

		[DllImport(LibraryName, EntryPoint = "GetProgramInfoLog", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr GetProgramInfoLogInternal(IntPtr programHandle);

		public static string GetProgramInfoLog(IntPtr programHandle)
		{
			return Marshal.PtrToStringAnsi(GetProgramInfoLogInternal(programHandle));
		}

		[DllImport(LibraryName, EntryPoint = "GetSpvSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetSpvSize(IntPtr programHandle, Stage stage);

		[DllImport(LibraryName, EntryPoint = "GetSpv", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetSpv(IntPtr programHandle, Stage stage);

		[DllImport(LibraryName, EntryPoint = "GetActiveAttribCount", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveAttribCount(IntPtr programHandle);

		[DllImport(LibraryName, EntryPoint = "GetActiveAttribName", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr GetActiveAttribNameInternal(IntPtr programHandle, int index);

		private static string GetActiveAttribName(IntPtr programHandle, int index)
		{
			return Marshal.PtrToStringAnsi(GetActiveAttribNameInternal(programHandle, index));
		}

		[DllImport(LibraryName, EntryPoint = "GetActiveAttribType", CallingConvention = CallingConvention.Cdecl)]
		public static extern VariableType GetActiveAttribType(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveAttribLocation", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveAttribLocation(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockCount", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBlockCount(IntPtr programHandle);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockBinding", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBlockBinding(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBlockSize(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockStage", CallingConvention = CallingConvention.Cdecl)]
		public static extern Stage GetActiveUniformBlockStage(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformCount", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformCount(IntPtr programHandle);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformName", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetActiveUniformNameInternal(IntPtr programHandle, int index);

		public static string GetActiveUniformName(IntPtr programHandle, int index)
		{
			return Marshal.PtrToStringAnsi(GetActiveUniformNameInternal(programHandle, index));
		}

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformType", CallingConvention = CallingConvention.Cdecl)]
		public static extern VariableType GetActiveUniformType(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformArraySize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformArraySize(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformArrayStride", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformArrayStride(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformMatrixStride", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformMatrixStride(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformStage", CallingConvention = CallingConvention.Cdecl)]
		public static extern Stage GetActiveUniformStage(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBinding", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBinding(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockIndex", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBlockIndex(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "GetActiveUniformBlockOffset", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetActiveUniformBlockOffset(IntPtr programHandle, int index);

		[DllImport(LibraryName, EntryPoint = "DestroyProgram", CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyProgram(IntPtr programHandle);
	}
}

using System;

namespace Lime.Graphics.Platform
{
	public interface IPlatformShaderProgram : IDisposable
	{
		UniformDesc[] GetUniformDescriptions();
		void SetUniform(int index, IntPtr data, int elementCount);
	}

	public enum ShaderVariableType
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
		SamplerCube,
		SamplerExternal,
	}

	public struct UniformDesc
	{
		public string Name;
		public ShaderVariableType Type;
		public int ArraySize;
	}

	public static class ShaderVariableTypeExtensions
	{
		public static bool IsSampler(this ShaderVariableType type)
		{
			switch (type) {
				case ShaderVariableType.Sampler2D:
				case ShaderVariableType.SamplerCube:
				case ShaderVariableType.SamplerExternal:
					return true;
				default:
					return false;
			}
		}

		public static int GetRowCount(this ShaderVariableType type)
		{
			switch (type) {
				case ShaderVariableType.BoolVector2:
				case ShaderVariableType.IntVector2:
				case ShaderVariableType.FloatVector2:
				case ShaderVariableType.FloatMatrix2:
					return 2;
				case ShaderVariableType.BoolVector3:
				case ShaderVariableType.IntVector3:
				case ShaderVariableType.FloatVector3:
				case ShaderVariableType.FloatMatrix3:
					return 3;
				case ShaderVariableType.BoolVector4:
				case ShaderVariableType.IntVector4:
				case ShaderVariableType.FloatVector4:
				case ShaderVariableType.FloatMatrix4:
					return 4;
				default:
					return 1;
			}
		}

		public static int GetColumnCount(this ShaderVariableType type)
		{
			switch (type) {
				case ShaderVariableType.FloatMatrix2:
					return 2;
				case ShaderVariableType.FloatMatrix3:
					return 3;
				case ShaderVariableType.FloatMatrix4:
					return 4;
				default:
					return 1;
			}
		}
	}
}

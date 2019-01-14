using System;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES20;

namespace Lime.Graphics.Platform.OpenGL
{
	internal unsafe class PlatformShaderProgram : IPlatformShaderProgram
	{
		private UniformInfo[] uniformInfos;
		private int[] dirtyUniformElementCount;
		private int[] dirtyUniformIndices;
		private int dirtyUniformCount;
		private int uniformStagingDataSize;
		private IntPtr uniformStagingData;

		internal int GLProgram;
		internal int[] TextureSlots;

		public PlatformRenderContext Context { get; }

		public PlatformShaderProgram(
			PlatformRenderContext context,
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocations,
			ShaderProgram.Sampler[] samplers)
		{
			Context = context;
			Initialize(shaders, attribLocations, samplers);
		}

		public void Dispose()
		{
			if (uniformStagingData != IntPtr.Zero) {
				Marshal.FreeHGlobal(uniformStagingData);
				uniformStagingData = IntPtr.Zero;
			}
			if (GLProgram != 0) {
				GL.DeleteProgram(GLProgram);
				GLHelper.CheckGLErrors();
				GLProgram = 0;
			}
		}

		private void Initialize(
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocations,
			ShaderProgram.Sampler[] samplers)
		{
			GLProgram = GL.CreateProgram();
			GLHelper.CheckGLErrors();
			foreach (var shader in shaders) {
				GL.AttachShader(GLProgram, ((PlatformShader)shader).GLShader);
				GLHelper.CheckGLErrors();
			}
			foreach (var attribLocation in attribLocations) {
				GL.BindAttribLocation(GLProgram, attribLocation.Index, attribLocation.Name);
				GLHelper.CheckGLErrors();
			}
			GL.LinkProgram(GLProgram);
			GLHelper.CheckGLErrors();
			GL.GetProgram(GLProgram, ProgramParameter.LinkStatus, out int linkStatus);
			GLHelper.CheckGLErrors();
			if (linkStatus == 0) {
				var infoLog = GL.GetProgramInfoLog(GLProgram);
				GLHelper.CheckGLErrors();
				GL.DeleteProgram(GLProgram);
				GLHelper.CheckGLErrors();
				throw new InvalidOperationException($"Shader program link failed:\n{infoLog}");
			}
			GL.UseProgram(GLProgram);
			GLHelper.CheckGLErrors();
			foreach (var sampler in samplers) {
				var location = GL.GetUniformLocation(GLProgram, sampler.Name);
				GL.Uniform1(location, sampler.Stage);
			}
			Reflect(samplers);
			uniformStagingData = Marshal.AllocHGlobal(uniformStagingDataSize);
			dirtyUniformCount = 0;
			dirtyUniformIndices = new int[uniformInfos.Length];
			dirtyUniformElementCount = new int[uniformInfos.Length];
			for (var i = 0; i < uniformInfos.Length; i++) {
				dirtyUniformElementCount[i] = 0;
			}
			TextureSlots = uniformInfos
				.Select(ui => ui.TextureSlot)
				.Where(slot => slot >= 0)
				.ToArray();
		}

		private void Reflect(ShaderProgram.Sampler[] samplers)
		{
			GL.GetProgram(GLProgram, ProgramParameter.ActiveUniforms, out int uniformCount);
			GLHelper.CheckGLErrors();
			GL.GetProgram(GLProgram, ProgramParameter.ActiveUniformMaxLength, out int maxUniformNameLength);
			GLHelper.CheckGLErrors();
			uniformInfos = new UniformInfo[uniformCount];
			uniformStagingDataSize = 0;
			for (var i = 0; i < uniformCount; i++) {
				StringBuilder sb = new StringBuilder();
				GL.GetActiveUniform(GLProgram, i, maxUniformNameLength, out _, out var arraySize, out ActiveUniformType type, sb);
				GLHelper.CheckGLErrors();
				var name = sb.ToString();
				var location = GL.GetUniformLocation(GLProgram, name);
				GLHelper.CheckGLErrors();
				var info = new UniformInfo {
					Name = AdjustUniformName(name),
					Type = ConvertUniformType(type),
					ArraySize = arraySize,
					Location = location,
					StagingDataOffset = -1,
					TextureSlot = -1
				};
				if (info.Type.IsSampler()) {
					if (info.ArraySize > 1) {
						throw new NotSupportedException();
					}
					info.TextureSlot = samplers.First(sampler => sampler.Name == info.Name).Stage;
				} else {
					info.StagingDataOffset = uniformStagingDataSize;
					info.ElementSize = 4 * info.Type.GetRowCount() * info.Type.GetColumnCount();
					uniformStagingDataSize += info.ElementSize * info.ArraySize;
				}
				uniformInfos[i] = info;
			}
		}

		public void SetUniform(int index, IntPtr data, int elementCount)
		{
			var info = uniformInfos[index];
			if (info.StagingDataOffset < 0) {
				throw new InvalidOperationException();
			}
			elementCount = Math.Min(elementCount, info.ArraySize);
			GraphicsUtility.CopyMemory(uniformStagingData + info.StagingDataOffset, data, info.ElementSize * elementCount);
			var lastDirtyElementCount = dirtyUniformElementCount[index];
			if (lastDirtyElementCount == 0) {
				dirtyUniformIndices[dirtyUniformCount++] = index;
			}
			dirtyUniformElementCount[index] = Math.Max(lastDirtyElementCount, elementCount);
		}

		internal void SyncUniforms()
		{
			for (var i = 0; i < dirtyUniformCount; i++) {
				var uniformIndex = dirtyUniformIndices[i];
				var elementCount = dirtyUniformElementCount[uniformIndex];
				var info = uniformInfos[uniformIndex];
				var data = uniformStagingData + info.StagingDataOffset;
				switch (info.Type) {
					case ShaderVariableType.Bool:
					case ShaderVariableType.Int:
						GL.Uniform1(info.Location, elementCount, (int*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.BoolVector2:
					case ShaderVariableType.IntVector2:
						GL.Uniform2(info.Location, elementCount, (int*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.BoolVector3:
					case ShaderVariableType.IntVector3:
						GL.Uniform3(info.Location, elementCount, (int*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.BoolVector4:
					case ShaderVariableType.IntVector4:
						GL.Uniform4(info.Location, elementCount, (int*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.Float:
						GL.Uniform1(info.Location, elementCount, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatVector2:
						GL.Uniform2(info.Location, elementCount, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatVector3:
						GL.Uniform3(info.Location, elementCount, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatVector4:
						GL.Uniform4(info.Location, elementCount, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatMatrix2:
						GL.UniformMatrix2(info.Location, elementCount, false, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatMatrix3:
						GL.UniformMatrix3(info.Location, elementCount, false, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					case ShaderVariableType.FloatMatrix4:
						GL.UniformMatrix4(info.Location, elementCount, false, (float*)data);
						GLHelper.CheckGLErrors();
						break;
					default:
						throw new NotSupportedException();
				}
				dirtyUniformElementCount[uniformIndex] = 0;
			}
			dirtyUniformCount = 0;
		}

		public UniformDesc[] GetUniformDescriptions()
		{
			return uniformInfos.Select(info => new UniformDesc {
				Name = info.Name,
				Type = info.Type,
				ArraySize = info.ArraySize
			}).ToArray();
		}

		private static ShaderVariableType ConvertUniformType(ActiveUniformType type)
		{
			switch (type) {
				case ActiveUniformType.Bool:
					return ShaderVariableType.Bool;
				case ActiveUniformType.BoolVec2:
					return ShaderVariableType.BoolVector2;
				case ActiveUniformType.BoolVec3:
					return ShaderVariableType.BoolVector3;
				case ActiveUniformType.BoolVec4:
					return ShaderVariableType.BoolVector4;
				case ActiveUniformType.Int:
					return ShaderVariableType.Int;
				case ActiveUniformType.IntVec2:
					return ShaderVariableType.IntVector2;
				case ActiveUniformType.IntVec3:
					return ShaderVariableType.IntVector3;
				case ActiveUniformType.IntVec4:
					return ShaderVariableType.IntVector4;
				case ActiveUniformType.Float:
					return ShaderVariableType.Float;
				case ActiveUniformType.FloatVec2:
					return ShaderVariableType.FloatVector2;
				case ActiveUniformType.FloatVec3:
					return ShaderVariableType.FloatVector3;
				case ActiveUniformType.FloatVec4:
					return ShaderVariableType.FloatVector4;
				case ActiveUniformType.FloatMat2:
					return ShaderVariableType.FloatMatrix2;
				case ActiveUniformType.FloatMat3:
					return ShaderVariableType.FloatMatrix3;
				case ActiveUniformType.FloatMat4:
					return ShaderVariableType.FloatMatrix4;
				case ActiveUniformType.Sampler2D:
					return ShaderVariableType.Sampler2D;
				case ActiveUniformType.SamplerCube:
					return ShaderVariableType.SamplerCube;
				default:
					throw new NotSupportedException();
			}
		}

		private string AdjustUniformName(string name)
		{
			var dotIndex = name.LastIndexOf('.');
			var bracketIndex = name.LastIndexOf('[');
			if (bracketIndex > dotIndex) {
				return name.Remove(bracketIndex);
			}
			return name;
		}

		private class UniformInfo
		{
			public string Name;
			public ShaderVariableType Type;
			public int ArraySize;
			public int Location;
			public int StagingDataOffset;
			public int ElementSize;
			public int TextureSlot;
		}
	}
}

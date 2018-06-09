#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

#if iOS || ANDROID
using GetProgramParameterName = OpenTK.Graphics.ES20.ProgramParameter;
#elif MAC || MONOMAC
using GetProgramParameterName = OpenTK.Graphics.OpenGL.ProgramParameter;
#endif

#pragma warning disable 0618

namespace Lime
{
	public class ShaderProgram : IGLObject
	{
		public class AttribLocation
		{
			public string Name;
			public int Index;
		}

		public class Sampler
		{
			public string Name;
			public int Stage;
		}

		private int handle;
		private List<Shader> shaders = new List<Shader>();
		private List<AttribLocation> attribLocations;
		private List<Sampler> samplers;

		private ShaderParam[] paramsToSync;
		private BoundShaderParam[] boundParams;
		private Uniform[] uniforms;
		
		public ShaderProgram(IEnumerable<Shader> shaders, IEnumerable<AttribLocation> attribLocations, IEnumerable<Sampler> samplers)
		{
			this.shaders = new List<Shader>(shaders);
			this.attribLocations = new List<AttribLocation>(attribLocations);
			this.samplers = new List<Sampler>(samplers);
			Create();
			GLObjectRegistry.Instance.Add(this);
		}

		private void Create()
		{
			handle = GL.CreateProgram();
			foreach (var shader in shaders) {
				GL.AttachShader(handle, shader.GetHandle());
			}
			foreach (var i in attribLocations) {
				GL.BindAttribLocation(handle, i.Index, i.Name);
			}
			Link();
			uniforms = ReflectUniforms().OrderBy(i => i.SortingKey).ToArray();
			paramsToSync = new ShaderParam[uniforms.Length];
			boundParams = new BoundShaderParam[uniforms.Length];
			GL.UseProgram(handle);
			PlatformRenderer.CheckErrors();
			foreach (var i in samplers) {
				GL.Uniform1(GL.GetUniformLocation(handle, i.Name), i.Stage);
				PlatformRenderer.CheckErrors();
			}
		}

		~ShaderProgram()
		{
			Dispose();
		}

		public void Dispose()
		{
			Discard();
		}

		public void Discard()
		{
			if (handle != 0) {
				var capturedHandle = handle;
				Window.Current.InvokeOnRendering(() => {
#if MAC || MONOMAC
					GL.DeleteProgram(1, new int[] { capturedHandle });
#else
					GL.DeleteProgram(capturedHandle);
#endif
				});
				handle = 0;
			}
		}

		private void Link()
		{
			GL.LinkProgram(handle);
			var result = new int[1];
			GL.GetProgram(handle, ProgramParameter.LinkStatus, result);
			if (result[0] == 0) {
				var infoLog = GetLinkLog();
				Logger.Write("Shader program link log:\n{0}", infoLog);
				throw new Lime.Exception(infoLog.ToString());
			}
			PlatformRenderer.CheckErrors();
		}

		private string GetLinkLog()
		{
			var logLength = new int[1];
			GL.GetProgram(handle, ProgramParameter.InfoLogLength, logLength);
			if (logLength[0] > 0) {
				var infoLog = new System.Text.StringBuilder(logLength[0]);
				unsafe {
					GL.GetProgramInfoLog(handle, logLength[0], (int*)null, infoLog);
				}
				return infoLog.ToString();
			}
			return "";
		}

		internal int GetHandle()
		{
			if (handle == 0) {
				Create();
			}
			return handle;
		}

		internal unsafe void SyncParams(ShaderParams[] paramsArray, int count)
		{
			Array.Clear(paramsToSync, 0, paramsToSync.Length);
			for (var i = count - 1; i >= 0; i--) {
				var @params = paramsArray[i];
				var k = 0;
				for (var j = 0; j < uniforms.Length; j++) {
					if (paramsToSync[j] != null) {
						continue;
					}
					var uniformReflections = uniforms[j];
					while (k < @params.Count && uniformReflections.SortingKey > @params.SortingKeys[k]) {
						k++;
					}
					if (k == @params.Count) {
						break;
					}
					if (uniformReflections.SortingKey == @params.SortingKeys[k]) {
						paramsToSync[j] = @params.Items[@params.ItemIndices[k]];
					}
				}
			}
			for (var i = 0; i < paramsToSync.Length; i++) {
				var param = paramsToSync[i];
				if (param == null) {
					continue;
				}
				if (param == boundParams[i].Param &&
					param.Version == boundParams[i].Version
				) {
					continue;
				}
				var uniform = uniforms[i];
				switch (uniform.Type) {
					case ActiveUniformType.Float:
						SyncFloat(uniform.Location, (ShaderParam<float>)param);
						break;
					case ActiveUniformType.FloatVec2:
						SyncFloatVector2(uniform.Location, (ShaderParam<Vector2>)param);
						break;
					case ActiveUniformType.FloatVec3:
						SyncFloatVector3(uniform.Location, (ShaderParam<Vector3>)param);
						break;
					case ActiveUniformType.FloatVec4:
						SyncFloatVector4(uniform.Location, (ShaderParam<Vector4>)param);
						break;
					case ActiveUniformType.FloatMat4:
						SyncFloatMatrix4x4(uniform.Location, (ShaderParam<Matrix44>)param);
						break;
					case ActiveUniformType.Bool:
					case ActiveUniformType.Int:
						SyncInt(uniform.Location, (ShaderParam<int>)param);
						break;
					case ActiveUniformType.BoolVec2:
					case ActiveUniformType.IntVec2:
						SyncIntVector2(uniform.Location, (ShaderParam<IntVector2>)param);
						break;
				}
				boundParams[i] = new BoundShaderParam {
					Param = param,
					Version = param.Version
				};
			}
		}

		private unsafe void SyncFloat(int location, ShaderParam<float> param)
		{
			fixed (float* dataPtr = param.Data) {
				GL.Uniform1(location, param.Count, dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncFloatVector2(int location, ShaderParam<Vector2> param)
		{
			fixed (Vector2* dataPtr = param.Data) {
				GL.Uniform2(location, param.Count, (float*)dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncFloatVector3(int location, ShaderParam<Vector3> parameter)
		{
			fixed (Vector3* dataPtr = parameter.Data) {
				GL.Uniform3(location, parameter.Count, (float*)dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncFloatVector4(int location, ShaderParam<Vector4> param)
		{
			fixed (Vector4* dataPtr = param.Data) {
				GL.Uniform4(location, param.Count, (float*)dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncFloatMatrix4x4(int location, ShaderParam<Matrix44> param)
		{
			fixed (Matrix44* dataPtr = param.Data) {
				GL.UniformMatrix4(location, param.Count, false, (float*)dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncInt(int location, ShaderParam<int> param)
		{
			fixed (int* dataPtr = param.Data) {
				GL.Uniform1(location, param.Count, dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private unsafe void SyncIntVector2(int location, ShaderParam<IntVector2> param)
		{
			fixed (IntVector2* dataPtr = param.Data) {
				GL.Uniform2(location, param.Count, (int*)dataPtr);
				PlatformRenderer.CheckErrors();
			}
		}

		private IEnumerable<Uniform> ReflectUniforms()
		{
			int count, maxNameLength;
			GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out count);
			GL.GetProgram(handle, GetProgramParameterName.ActiveUniformMaxLength, out maxNameLength);
			var sb = new StringBuilder(maxNameLength);
			for (var i = 0; i < count; i++) {
				sb.Clear();
				int size, nameLength;
				ActiveUniformType type;
				GL.GetActiveUniform(handle, i, maxNameLength, out nameLength, out size, out type, sb);
				var name = AdjustUniformName(sb.ToString());
				if (type != ActiveUniformType.Sampler2D &&
					type != ActiveUniformType.SamplerCube
				) {
					yield return new Uniform {
						Type = type,
						SortingKey = GetSortingKey(name, type),
						Location = GL.GetUniformLocation(handle, name)
					};
				}
			}
		}

		private static int GetSortingKey(string name, ActiveUniformType type)
		{
			switch (type) {
				case ActiveUniformType.Bool:
				case ActiveUniformType.Int:
					return ShaderParams.GetSortingKey(name, typeof(int));
				case ActiveUniformType.BoolVec2:
				case ActiveUniformType.IntVec2:
					return ShaderParams.GetSortingKey(name, typeof(IntVector2));
				case ActiveUniformType.Float:
					return ShaderParams.GetSortingKey(name, typeof(float));
				case ActiveUniformType.FloatVec2:
					return ShaderParams.GetSortingKey(name, typeof(Vector2));
				case ActiveUniformType.FloatVec3:
					return ShaderParams.GetSortingKey(name, typeof(Vector3));
				case ActiveUniformType.FloatVec4:
					return ShaderParams.GetSortingKey(name, typeof(Vector4));
				case ActiveUniformType.FloatMat4:
					return ShaderParams.GetSortingKey(name, typeof(Matrix44));
				default:
					throw new NotSupportedException();
			}
		}

		private static string AdjustUniformName(string name)
		{
			var arraySign = name.IndexOf('[');
			if (arraySign >= 0) {
				name = name.Remove(arraySign);
			}
			return name;
		}
	}

	internal struct Uniform
	{
		public ActiveUniformType Type;
		public int SortingKey;
		public int Location;
	}

	internal struct BoundShaderParam
	{
		public ShaderParam Param;
		public int Version;
	}
}

#endif
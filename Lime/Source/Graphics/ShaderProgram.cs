using System;
using System.Collections.Generic;
using System.Linq;
using Lime.Graphics.Platform;

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

		private IPlatformShaderProgram platformProgram;
		private Shader[] shaders;
		private AttribLocation[] attribLocations;
		private Sampler[] samplers;

		private ShaderParam[] paramsToSync;
		private BoundShaderParam[] boundParams;
		private Uniform[] uniforms;

		public ShaderProgram(IEnumerable<Shader> shaders, IEnumerable<AttribLocation> attribLocations, IEnumerable<Sampler> samplers)
		{
			this.shaders = shaders.ToArray();
			this.attribLocations = attribLocations.ToArray();
			this.samplers = samplers.ToArray();
			GLObjectRegistry.Instance.Add(this);
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
			if (platformProgram != null) {
				var platformProgramCopy = platformProgram;
				Window.Current.InvokeOnRendering(() => {
					platformProgramCopy.Dispose();
				});
				platformProgram = null;
			}
		}

		internal IPlatformShaderProgram GetPlatformProgram()
		{
			if (platformProgram == null) {
				Create();
			}
			return platformProgram;
		}

		private void Create()
		{
			var platformShaders = shaders.Select(i => i.GetPlatformShader()).ToArray();
			platformProgram = RenderContextManager.CurrentContext.CreateShaderProgram(platformShaders, attribLocations, samplers);
			uniforms = ReflectUniforms().OrderBy(i => i.SortingKey).ToArray();
			paramsToSync = new ShaderParam[uniforms.Length];
			boundParams = new BoundShaderParam[uniforms.Length];
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
					case ShaderVariableType.Float:
						UpdateUniform(uniform.Index, (ShaderParam<float>)param);
						break;
					case ShaderVariableType.FloatVector2:
						UpdateUniform(uniform.Index, (ShaderParam<Vector2>)param);
						break;
					case ShaderVariableType.FloatVector3:
						UpdateUniform(uniform.Index, (ShaderParam<Vector3>)param);
						break;
					case ShaderVariableType.FloatVector4:
						UpdateUniform(uniform.Index, (ShaderParam<Vector4>)param);
						break;
					case ShaderVariableType.FloatMatrix4:
						UpdateUniform(uniform.Index, (ShaderParam<Matrix44>)param);
						break;
					case ShaderVariableType.Bool:
					case ShaderVariableType.Int:
						UpdateUniform(uniform.Index, (ShaderParam<int>)param);
						break;
					case ShaderVariableType.BoolVector2:
					case ShaderVariableType.IntVector2:
						UpdateUniform(uniform.Index, (ShaderParam<IntVector2>)param);
						break;
				}
				boundParams[i] = new BoundShaderParam {
					Param = param,
					Version = param.Version
				};
			}
		}

		private unsafe void UpdateUniform<T>(int uniformIndex, ShaderParam<T> p) where T : unmanaged
		{
			fixed (T* data = p.Data) {
				platformProgram.SetUniform(uniformIndex, new IntPtr(data), p.Count);
			}
		}

		private IEnumerable<Uniform> ReflectUniforms()
		{
			var uniformDescs = platformProgram.GetUniformDescriptions();
			for (var i = 0; i < uniformDescs.Length; i++) {
				var desc = uniformDescs[i];
				if (!desc.Type.IsSampler()) {
					yield return new Uniform {
						Type = desc.Type,
						SortingKey = GetSortingKey(desc.Name, desc.Type),
						Index = i
					};
				}
			}
		}

		private static int GetSortingKey(string name, ShaderVariableType type)
		{
			switch (type) {
				case ShaderVariableType.Bool:
				case ShaderVariableType.Int:
					return ShaderParams.GetSortingKey(name, typeof(int));
				case ShaderVariableType.BoolVector2:
				case ShaderVariableType.IntVector2:
					return ShaderParams.GetSortingKey(name, typeof(IntVector2));
				case ShaderVariableType.Float:
					return ShaderParams.GetSortingKey(name, typeof(float));
				case ShaderVariableType.FloatVector2:
					return ShaderParams.GetSortingKey(name, typeof(Vector2));
				case ShaderVariableType.FloatVector3:
					return ShaderParams.GetSortingKey(name, typeof(Vector3));
				case ShaderVariableType.FloatVector4:
					return ShaderParams.GetSortingKey(name, typeof(Vector4));
				case ShaderVariableType.FloatMatrix4:
					return ShaderParams.GetSortingKey(name, typeof(Matrix44));
				default:
					throw new NotSupportedException($"name: {name}, type: {type.ToString()}");
			}
		}
	}

	internal struct Uniform
	{
		public ShaderVariableType Type;
		public int SortingKey;
		public int Index;
	}

	internal struct BoundShaderParam
	{
		public ShaderParam Param;
		public int Version;
	}
}

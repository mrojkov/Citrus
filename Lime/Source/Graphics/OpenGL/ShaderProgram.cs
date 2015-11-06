#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
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
		public int ProjectionMatrixUniformId { get; private set; }
		private Dictionary<string, int> uniformIds = new Dictionary<string, int>();
		private List<Shader> shaders = new List<Shader>();
		private List<AttribLocation> attribLocations;
		private List<Sampler> samplers;

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
				BindAttribLocation(i.Index, i.Name);
			}
			Link();
			foreach (var i in samplers) {
				BindSampler(i.Name, i.Stage);
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
				Application.InvokeOnMainThread(() => {
#if MAC
					GL.DeleteProgram(1, new int[] { capturedHandle });
#else
					GL.DeleteProgram(capturedHandle);
#endif
				});
				handle = 0;
			}
		}

		private void BindAttribLocation(int index, string name)
		{
			GL.BindAttribLocation(handle, index, name);
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
			InitializeUniformIds();
			PlatformRenderer.CheckErrors();
		}

		protected virtual void InitializeUniformIds()
		{
			ProjectionMatrixUniformId = GetUniformId("matProjection");
		}

		protected int GetUniformId(string name)
		{
			int id;
			if (uniformIds.TryGetValue(name, out id)) {
				return id;
			}
			id = GL.GetUniformLocation(handle, name);
			uniformIds[name] = id;
			return id;
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

		public void Use()
		{
			if (handle == 0) {
				Create();
			}
			GL.UseProgram(handle);
			LoadUniformValues();
		}

		protected virtual void LoadUniformValues()
		{
		}

		public void LoadMatrix(int uniformId, Matrix44 matrix)
		{
			unsafe {
				float* p = (float*)&matrix;
				GL.UniformMatrix4(uniformId, 1, false, p);
			}
		}

		public void LoadFloat(int uniformId, float value)
		{
			GL.Uniform1(uniformId, value);
		}

		public void LoadInteger(int uniformId, int value)
		{
			GL.Uniform1(uniformId, value);
		}

		public void LoadBoolean(int uniformId, bool value)
		{
			GL.Uniform1(uniformId, value ? 1 : 0);
		}

		public void LoadVector2(int uniformId, Vector2 vector)
		{
			GL.Uniform2(uniformId, vector.X, vector.Y);
		}

		private void BindSampler(string name, int stage)
		{
			Use();
			GL.Uniform1(GL.GetUniformLocation(handle, name), stage);
			PlatformRenderer.CheckErrors();
		}
	}
}
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	class ShaderProgram
	{
		int handle;
		public int ProjectionMatrixUniformId { get; private set; }
		public int UseAlphaTexture1UniformId { get; private set; }
		public int UseAlphaTexture2UniformId { get; private set; }
		private Dictionary<string, int> uniformIds = new Dictionary<string, int>();

		public ShaderProgram()
		{
			handle = GL.CreateProgram();
		}

		public void AttachShader(Shader shader)
		{
			GL.AttachShader(handle, shader.GetHandle());
		}

		public void DetachShader(Shader shader)
		{
			GL.DetachShader(handle, shader.GetHandle());
		}

		public void BindAttribLocation(int index, string name)
		{
			GL.BindAttribLocation(handle, index, name);
		}

		public void Link()
		{
			GL.LinkProgram(handle);
			var result = new int[1];
			GL.GetProgram(handle, GetProgramParameterName.LinkStatus, result);
			if (result[0] == 0) {
				var infoLog = GetLinkLog();
				Logger.Write("Shader program link log:\n{0}", infoLog);
				throw new Lime.Exception(infoLog.ToString());
			}
			ProjectionMatrixUniformId = GetUniformId("matProjection");
			UseAlphaTexture1UniformId = GetUniformId("useAlphaTexture1");
			UseAlphaTexture2UniformId = GetUniformId("useAlphaTexture2");
			PlatformRenderer.CheckErrors();
		}

		public int GetUniformId(string name)
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
			GL.GetProgram(handle, GetProgramParameterName.InfoLogLength, logLength);
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
			GL.UseProgram(handle);
		}

		public void LoadMatrix(int uniformId, Matrix44 matrix)
		{
			unsafe {
				float* p = (float*)&matrix;
				GL.UniformMatrix4(uniformId, 1, false, p);
			}
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

		public void BindSampler(string name, int stage)
		{
			Use();
			GL.Uniform1(GL.GetUniformLocation(handle, name), stage);
			PlatformRenderer.CheckErrors();
		}
	}
}

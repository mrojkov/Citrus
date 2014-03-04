using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
#if iOS
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class ShaderProgram
	{
		int handle;
		public int ProjectionMatrixUniformId { get; private set; }

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
				var logLength = new int[1];
				GL.GetProgram(handle, ProgramParameter.InfoLogLength, logLength);
				if (logLength[0] > 0) {
					var infoLog = new System.Text.StringBuilder(logLength[0]);
					unsafe {
						GL.GetProgramInfoLog(handle, logLength[0], (int*)null, infoLog);
					}
					Logger.Write("Shader program link log:\n{0}", infoLog);
					throw new Lime.Exception(infoLog.ToString());
				}
			}
			ProjectionMatrixUniformId = GL.GetUniformLocation(handle, "matProjection");
			Renderer.CheckErrors();
		}

		public void Use()
		{
			GL.UseProgram(handle);
		}

		public void BindSampler(string name, int stage)
		{
			Use();
			GL.Uniform1(GL.GetUniformLocation(handle, name), stage);
			Renderer.CheckErrors();
		}
	}
}

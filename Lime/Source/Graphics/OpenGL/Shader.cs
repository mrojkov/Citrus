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
	public class Shader
	{
		int handle;

		public Shader(bool fragmentOrVertex, string source)
		{
			source = ReplacePrecisionModifiers(source);
			handle = GL.CreateShader(fragmentOrVertex ? ShaderType.FragmentShader : ShaderType.VertexShader);
			GL.ShaderSource(handle, 1, new string[] { source }, new int[] { source.Length });
			GL.CompileShader(handle);
			var result = new int[1];
			GL.GetShader(handle, ShaderParameter.CompileStatus, result);
			if (result[0] == 0) {
				var infoLog = GetCompileLog();
				Logger.Write("Shader compile log:\n{0}", infoLog);
				throw new Lime.Exception(infoLog);
			}
		}

		private string GetCompileLog()
		{
			var logLength = new int[1];
			GL.GetShader(handle, ShaderParameter.InfoLogLength, logLength);
			if (logLength[0] > 0) {
				var infoLog = new System.Text.StringBuilder(logLength[0]);
				unsafe {
					GL.GetShaderInfoLog(handle, logLength[0], (int*)null, infoLog);
				}
				return infoLog.ToString();
			}
			return "";
		}

		private static string ReplacePrecisionModifiers(string source)
		{
			if (GameView.Instance.RenderingApi == RenderingApi.OpenGL) {
				source = source.Replace(" lowp ", " ");
				source = source.Replace(" mediump ", " ");
				source = source.Replace(" highp ", " ");
			}
			return source;
		}

		public int GetHandle()
		{
			return handle;
		}
	}

	public class VertexShader : Shader
	{
		public VertexShader(string source)
			: base(fragmentOrVertex: false, source: source)
		{
		}
	}

	public class FragmentShader : Shader
	{
		public FragmentShader(string source)
			: base(fragmentOrVertex: true, source: source)
		{
		}
	}
}

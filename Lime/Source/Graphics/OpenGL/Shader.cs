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

namespace Lime
{
	public class Shader : IGLObject, IDisposable
	{
		private int handle;
		private string source;
		private bool fragmentOrVertex;

		public Shader(bool fragmentOrVertex, string source)
		{
			this.fragmentOrVertex = fragmentOrVertex;
			this.source = ReplacePrecisionModifiers(source);
			CreateShader();
			GLObjectRegistry.Instance.Add(this);
		}

		~Shader()
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
					GL.DeleteShader(capturedHandle);
				});
				handle = 0;
			}
		}

		private void CreateShader()
		{
			handle = GL.CreateShader(fragmentOrVertex ? ShaderType.FragmentShader : ShaderType.VertexShader);
#if MAC || MONOMAC
			var length = source.Length;
			GL.ShaderSource(handle, 1, new string[] { source }, ref length);
#else
			GL.ShaderSource(handle, 1, new string[] { source }, new int[] { source.Length });
#endif
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
			if (Application.RenderingBackend == RenderingBackend.OpenGL) {
				source = source.Replace("lowp ", "");
				source = source.Replace("mediump ", "");
				source = source.Replace("highp ", "");
			}
			return source;
		}

		public int GetHandle()
		{
			if (handle == 0) {
				CreateShader();
			}
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
#endif
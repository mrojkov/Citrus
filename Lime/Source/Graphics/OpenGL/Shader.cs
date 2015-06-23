#if OPENGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using OpenTK.Graphics;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
using ShaderInfoKind = OpenTK.Graphics.ES20.ShaderParameter;
#else
using OpenTK.Graphics.OpenGL;
using ShaderInfoKind = OpenTK.Graphics.OpenGL.ShaderParameter;
#endif

namespace Lime
{
	internal class Shader : IGLObject, IDisposable
	{
		private int handle;
		private string source;
		private ShaderType type;

		public Shader(ShaderType type, string source)
		{
			this.type = type;
			this.source = ReplacePrecisionModifiers(source);
			CreateShader();
			GLObjectRegistry.Instance.Add(this);
		}

		~Shader()
		{
			Discard();
		}

		public void Dispose()
		{
			Discard();
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (handle != 0) {
				Application.InvokeOnMainThread(() => {
					GL.DeleteShader(handle);
				});
				handle = 0;
			}
		}

		private void CreateShader()
		{
			handle = GL.CreateShader(type);
#if MAC
			var length = source.Length;
			GL.ShaderSource(handle, 1, new string[] { source }, ref length);
#else
			GL.ShaderSource(handle, 1, new string[] { source }, new int[] { source.Length });
#endif
			GL.CompileShader(handle);
			var result = new int[1];
			GL.GetShader(handle, ShaderInfoKind.CompileStatus, result);
			if (result[0] == 0) {
				var infoLog = GetCompileLog();
				Logger.Write("Shader compile log:\n{0}", infoLog);
				throw new Lime.Exception(infoLog);
			}
		}

		private string GetCompileLog()
		{
			var logLength = new int[1];
			GL.GetShader(handle, ShaderInfoKind.InfoLogLength, logLength);
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

		public static Shader LoadFromResources(string name, ShaderPool pool)
		{
			name = string.Format("Lime.Resources.Shaders.{0}.glsl", name);
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			using (var reader = new StreamReader(stream)) {
				return pool[reader.ReadToEnd()];
			}
		}
	}

	internal sealed class ShaderPool
	{
		public static readonly ShaderPool VertexShaders = new ShaderPool(ShaderType.VertexShader);
		public static readonly ShaderPool FragmentShaders = new ShaderPool(ShaderType.FragmentShader);

		private ShaderType itemType;
		private Dictionary<string, Shader> items = new Dictionary<string, Shader>();

		public Shader this[string source]
		{
			get { return GetShaderBySource(source); }
		}

		private ShaderPool(ShaderType itemType)
		{
			this.itemType = itemType;
		}

		private Shader GetShaderBySource(string source)
		{
			Shader shader;
			if (items.TryGetValue(source, out shader)) {
				return shader;
			}
			shader = new Shader(itemType, source);
			items[source] = shader;
			return shader;
		}
	}
}
#endif
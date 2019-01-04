using System;
using Lime.Graphics.Platform;

namespace Lime
{
	public class Shader : IGLObject, IDisposable
	{
		private IPlatformShader platformShader;
		private string source;
		private ShaderStageMask stage;

		protected Shader(ShaderStageMask stage, string source)
		{
			this.stage = stage;
			this.source = source;
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
			if (platformShader != null) {
				var platformShaderCopy = platformShader;
				Window.Current.InvokeOnRendering(() => {
					platformShaderCopy.Dispose();
				});
				platformShader = null;
			}
		}

		internal IPlatformShader GetPlatformShader()
		{
			if (platformShader == null) {
				platformShader = RenderContextManager.CurrentContext.CreateShader(stage, source);
			}
			return platformShader;
		}
	}

	public class VertexShader : Shader
	{
		public VertexShader(string source)
			: base(ShaderStageMask.Vertex, source: source)
		{
		}
	}

	public class FragmentShader : Shader
	{
		public FragmentShader(string source)
			: base(ShaderStageMask.Fragment, source: source)
		{
		}
	}
}

using System;
using Lime.Graphics.Platform;

namespace Lime
{
	public class Shader : IDisposable
	{
		private IPlatformShader platformShader;
		private string source;
		private ShaderStageMask stage;

		protected Shader(ShaderStageMask stage, string source)
		{
			this.stage = stage;
			this.source = source;
		}

		~Shader()
		{
			DisposeInternal();
		}

		public void Dispose()
		{
			DisposeInternal();
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal()
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
				platformShader = PlatformRenderer.Context.CreateShader(stage, source);
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

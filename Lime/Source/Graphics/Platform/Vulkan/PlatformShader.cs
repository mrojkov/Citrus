using System;

namespace Lime.Graphics.Platform.Vulkan
{
	internal class PlatformShader : IPlatformShader
	{
		private PlatformRenderContext context;
		private IntPtr shader;

		internal IntPtr Shader => shader;

		public ShaderStageMask Stage { get; }

		public PlatformShader(PlatformRenderContext context, ShaderStageMask stage, string source)
		{
			this.context = context;
			Stage = stage;
			Create(source);
		}

		public void Dispose()
		{
			if (shader != IntPtr.Zero) {
				ShaderCompiler.DestroyShader(shader);
				shader = IntPtr.Zero;
			}
		}

		private void Create(string source)
		{
			var compilerStage = Stage == ShaderStageMask.Vertex
				? ShaderCompiler.Stage.Vertex
				: ShaderCompiler.Stage.Fragment;
			shader = ShaderCompiler.CreateShader();
			if (!ShaderCompiler.CompileShader(shader, compilerStage, source)) {
				var infoLog = ShaderCompiler.GetShaderInfoLog(shader);
				ShaderCompiler.DestroyShader(shader);
				throw new InvalidOperationException($"Shader compilation failed:\n{infoLog}");
			}
		}
	}
}

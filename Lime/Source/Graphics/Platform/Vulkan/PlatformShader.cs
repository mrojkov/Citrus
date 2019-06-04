using System;
using System.Runtime.InteropServices;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PlatformShader : IPlatformShader
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
			var hash = ComputeHash(Stage, source);
			var spv = context.PipelineCache.GetShaderSpv(hash);
			if (spv != null) {
				fixed (byte* spvPtr = spv) {
					ShaderCompiler.SetShaderSpv(shader, new IntPtr(spvPtr), (uint)spv.Length);
				}
			} else {
				if (!ShaderCompiler.CompileShader(shader, compilerStage, source)) {
					var infoLog = ShaderCompiler.GetShaderInfoLog(shader);
					ShaderCompiler.DestroyShader(shader);
					throw new InvalidOperationException($"Shader compilation failed:\n{infoLog}");
				}
				spv = new byte[ShaderCompiler.GetShaderSpvSize(shader)];
				Marshal.Copy(ShaderCompiler.GetShaderSpv(shader), spv, 0, spv.Length);
				context.PipelineCache.AddShaderSpv(hash, spv);
			}
		}

		private static long ComputeHash(ShaderStageMask stage, string source)
		{
			var hasher = new Hasher();
			hasher.Begin();
			hasher.Write(stage);
			hasher.Write(source.Length);
			hasher.Write(source);
			return hasher.End();
		}
	}
}

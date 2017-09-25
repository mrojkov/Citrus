namespace Lime
{
	internal class DepthBuffer : IMaterial
	{
		public Color4 ColorFactor { get; set; }
		public Matrix44 ViewProjection { get; set; }

		private DepthBufferProgram program;

		public DepthBuffer()
		{
			ColorFactor = Color4.White;
		}

		public void Apply()
		{
			PlatformRenderer.SetBlending(Blending.Alpha);
			PrepareShaderProgram();

			program.Use();
			program.LoadMatrix(program.LightWorldViewProjUniformId, Renderer.FixupWVP(Renderer.World * ViewProjection));
		}

		private void PrepareShaderProgram()
		{
			if (program != null) {
				return;
			}

			program = new DepthBufferProgram();
		}

		public IMaterial Clone()
		{
			return new DepthBuffer {
				ColorFactor = ColorFactor,
			};
		}
	}
}

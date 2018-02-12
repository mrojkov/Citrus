using System;
using Yuzu;

namespace Lime
{
	internal class DepthBuffer : IMaterial, IMaterialSkin
	{
		public Color4 ColorFactor { get; set; }
		public Matrix44 ViewProjection { get; set; }

		public bool SkinEnabled
		{
			get { return skinEnabled; }
			set
			{
				if (skinEnabled != value) {
					skinEnabled = value;
					program = null;
				}
			}
		}

		public int PassCount => 1;

		private bool skinEnabled = false;
		private DepthBufferProgram program;
		private Matrix44[] boneTransforms;
		private int boneCount;

		public DepthBuffer()
		{
			ColorFactor = Color4.White;
		}
		
		public void Apply(int pass)
		{
			PlatformRenderer.SetBlending(Blending.Alpha);
			PrepareShaderProgram();
			program.Use();

			if (skinEnabled) {
				program.LoadMatrixArray(program.BonesUniformId, boneTransforms, boneCount);
			}

			program.LoadMatrix(program.LightWorldViewProjUniformId, Renderer.FixupWVP(Renderer.World * ViewProjection));
		}
		
		private void PrepareShaderProgram()
		{
			if (program != null) {
				return;
			}

			program = new DepthBufferProgram(skinEnabled);
		}

		public void Invalidate()
		{
			program = null;
		}

		public IMaterial Clone()
		{
			return new DepthBuffer {
				ColorFactor = ColorFactor,
			};
		}

		public void SetBones(Matrix44[] boneTransforms, int boneCount)
		{
			this.boneTransforms = boneTransforms;
			this.boneCount = boneCount;
		}
	}
}

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

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private ShaderParamKeys shaderParamKeys;

		private bool skinEnabled = false;
		private DepthBufferProgram program;
		private Matrix44[] boneTransforms;
		private int boneCount;

		public DepthBuffer()
		{
			ColorFactor = Color4.White;
			shaderParams = new ShaderParams();
			shaderParamKeys = new ShaderParamKeys(shaderParams);
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
		}

		public void Apply(int pass)
		{
			//PrepareShaderProgram();
			//shaderParams.Set(shaderParamKeys.LightWorldViewProj, Renderer.FixupWVP(Renderer.World * ViewProjection));
			//if (skinEnabled) {
			//	shaderParams.Set(shaderParamKeys.Bones, boneTransforms, boneCount);
			//}
			//PlatformRenderer.SetBlendState(Blending.Alpha.GetBlendState());
			//PlatformRenderer.SetShaderProgram(program);
			//PlatformRenderer.SetShaderParams(shaderParamsArray);
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

		private class ShaderParamKeys
		{
			public readonly ShaderParamKey<Matrix44> Bones;
			public readonly ShaderParamKey<Matrix44> LightWorldViewProj;

			public ShaderParamKeys(ShaderParams parameters)
			{
				Bones = parameters.GetParamKey<Matrix44>("u_Bones");
				LightWorldViewProj = parameters.GetParamKey<Matrix44>("u_LightWorldViewProj");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzu;

namespace Lime
{
	public class CommonMaterial : IMaterial, IMaterialSkin, IMaterialFog
	{
		private static Dictionary<CommonMaterialProgramSpec, CommonMaterialProgram> programCache =
			new Dictionary<CommonMaterialProgramSpec, CommonMaterialProgram>();

		private CommonMaterialProgram program;
		private ITexture diffuseTexture;
		private Matrix44[] boneTransforms;
		private int boneCount;
		private bool skinEnabled;
		private FogMode fogMode;

		[YuzuMember]
		public string Name { get; set; }

		[YuzuMember]
		public Color4 DiffuseColor { get; set; }

		[YuzuMember]
		public Color4 FogColor { get; set; }

		[YuzuMember]
		public float FogStart { get; set; }

		[YuzuMember]
		public float FogEnd { get; set; }

		[YuzuMember]
		public float FogDensity { get; set; }

		[YuzuMember]
		public Blending Blending { get; set; }

		[YuzuMember]
		public ITexture DiffuseTexture
		{
			get { return diffuseTexture; }
			set
			{
				if (diffuseTexture != value) {
					diffuseTexture = value;
					program = null;
				}
			}
		}

		[YuzuMember]
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

		[YuzuMember]
		public FogMode FogMode
		{
			get { return fogMode; }
			set
			{
				if (fogMode != value) {
					fogMode = value;
					program = null;
				}
			}
		}

		public CommonMaterial()
		{
			DiffuseColor = Color4.White;
			FogColor = Color4.White;
			Blending = Blending.Alpha;

			shaderParams = new ShaderParams();
			shaderParamKeys = new ShaderParamKeys(shaderParams);
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
		}

		public void SetBones(Matrix44[] boneTransforms, int boneCount)
		{
			this.boneTransforms = boneTransforms;
			this.boneCount = boneCount;
		}

		public int PassCount => 1;

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private ShaderParamKeys shaderParamKeys;

		public void Apply(int pass)
		{
			PrepareShaderProgram();
			shaderParams.Set(shaderParamKeys.World, Renderer.FixupWVP(Renderer.World));
			shaderParams.Set(shaderParamKeys.WorldView, Renderer.FixupWVP(Renderer.WorldView));
			shaderParams.Set(shaderParamKeys.WorldViewProj, Renderer.FixupWVP(Renderer.WorldViewProjection));
			shaderParams.Set(shaderParamKeys.ColorFactor, Renderer.ColorFactor.ToVector4());
			shaderParams.Set(shaderParamKeys.DiffuseColor, DiffuseColor.ToVector4());
			shaderParams.Set(shaderParamKeys.FogColor, FogColor.ToVector4());
			shaderParams.Set(shaderParamKeys.FogStart, FogStart);
			shaderParams.Set(shaderParamKeys.FogEnd, FogEnd);
			shaderParams.Set(shaderParamKeys.FogDensity, FogDensity);
			if (skinEnabled) {
				shaderParams.Set(shaderParamKeys.Bones, boneTransforms, boneCount);
			}
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetTextureLegacy(CommonMaterialProgram.DiffuseTextureStage, diffuseTexture);
			PlatformRenderer.SetShaderProgram(program);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		private void PrepareShaderProgram()
		{
			if (program != null) {
				return;
			}
			var spec = new CommonMaterialProgramSpec {
				SkinEnabled = skinEnabled,
				DiffuseTextureEnabled = diffuseTexture != null,
				FogMode = FogMode
			};
			if (programCache.TryGetValue(spec, out program)) {
				return;
			}
			program = new CommonMaterialProgram(spec);
			programCache[spec] = program;
		}

		public void Invalidate()
		{
			program = null;
		}

		public IMaterial Clone()
		{
			return new CommonMaterial {
				Name = Name,
				DiffuseColor = DiffuseColor,
				FogMode = FogMode,
				FogColor = FogColor,
				FogStart = FogStart,
				FogEnd = FogEnd,
				FogDensity = FogDensity,
				Blending = Blending,
				DiffuseTexture = DiffuseTexture,
				SkinEnabled = SkinEnabled
			};
		}

		private class ShaderParamKeys
		{
			public readonly ShaderParamKey<Matrix44> WorldViewProj;
			public readonly ShaderParamKey<Matrix44> WorldView;
			public readonly ShaderParamKey<Matrix44> World;
			public readonly ShaderParamKey<Vector4> ColorFactor;
			public readonly ShaderParamKey<Vector4> DiffuseColor;
			public readonly ShaderParamKey<Vector4> LightColor;
			public readonly ShaderParamKey<Vector3> LightDirection;
			public readonly ShaderParamKey<float> LightIntensity;
			public readonly ShaderParamKey<float> LightStrength;
			public readonly ShaderParamKey<float> LightAmbient;
			public readonly ShaderParamKey<Matrix44> LightWorldViewProj;
			public readonly ShaderParamKey<Vector4> ShadowColor;
			public readonly ShaderParamKey<Matrix44> Bones;
			public readonly ShaderParamKey<Vector4> FogColor;
			public readonly ShaderParamKey<float> FogStart;
			public readonly ShaderParamKey<float> FogEnd;
			public readonly ShaderParamKey<float> FogDensity;

			public ShaderParamKeys(ShaderParams parameters)
			{
				WorldViewProj = parameters.GetParamKey<Matrix44>("u_WorldViewProj");
				WorldView = parameters.GetParamKey<Matrix44>("u_WorldView");
				World = parameters.GetParamKey<Matrix44>("u_World");
				ColorFactor = parameters.GetParamKey<Vector4>("u_ColorFactor");
				DiffuseColor = parameters.GetParamKey<Vector4>("u_DiffuseColor");
				LightColor = parameters.GetParamKey<Vector4>("u_LightColor");
				LightDirection = parameters.GetParamKey<Vector3>("u_LightDirection");
				LightIntensity = parameters.GetParamKey<float>("u_LightIntensity");
				LightStrength = parameters.GetParamKey<float>("u_LightStrength");
				LightAmbient = parameters.GetParamKey<float>("u_AmbientLight");
				LightWorldViewProj = parameters.GetParamKey<Matrix44>("u_LightWorldViewProjection");
				ShadowColor = parameters.GetParamKey<Vector4>("u_ShadowColor");
				Bones = parameters.GetParamKey<Matrix44>("u_Bones");
				FogColor = parameters.GetParamKey<Vector4>("u_FogColor");
				FogStart = parameters.GetParamKey<float>("u_FogStart");
				FogEnd = parameters.GetParamKey<float>("u_FogEnd");
				FogDensity = parameters.GetParamKey<float>("u_FogDensity");
			}
		}
	}
}

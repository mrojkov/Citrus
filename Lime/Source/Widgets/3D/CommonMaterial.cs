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

		private CommonMaterialProgram shadowsProgram;
		private CommonMaterialProgram program;
		private ITexture diffuseTexture;
		private Matrix44[] boneTransforms;
		private int boneCount;
		private bool skinEnabled;
		private FogMode fogMode;

		[YuzuMember("Name")]
		public string Id { get; set; }

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
		public SkinningMode SkinningMode
		{
			get => skinningMode;
			set
			{
				if (skinningMode != value) {
					skinningMode = value;
					Invalidate();
				}
			}
		}

		[YuzuMember]
		public ITexture DiffuseTexture
		{
			get { return diffuseTexture; }
			set
			{
				if (diffuseTexture != value) {
					diffuseTexture = value;
					Invalidate();
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
					Invalidate();
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
					Invalidate();
				}
			}
		}

		public static bool ShadowsRenderingMode { get; set; }

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

		public void SetBones(Vector4[] dualQuaternionPartA, Vector4[] dualQuaternionPartB, int boneCount)
		{
			this.dualQuaternionPartA = dualQuaternionPartA;
			this.dualQuaternionPartB = dualQuaternionPartB;
			this.boneCount = boneCount;
		}

		public int PassCount => 1;

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private ShaderParamKeys shaderParamKeys;
		private Vector4[] dualQuaternionPartA;
		private Vector4[] dualQuaternionPartB;
		private SkinningMode skinningMode;

		public void Apply(int pass)
		{
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
				if (SkinningMode == SkinningMode.Linear) {
					shaderParams.Set(shaderParamKeys.Bones, boneTransforms, boneCount);
				} else {
					shaderParams.Set(shaderParamKeys.DualQuaternionA, dualQuaternionPartA, boneCount);
					shaderParams.Set(shaderParamKeys.DualQuaternionB, dualQuaternionPartB, boneCount);
				}
			}
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetTextureLegacy(CommonMaterialProgram.DiffuseTextureStage, diffuseTexture);
			PlatformRenderer.SetShaderProgram(GetEffectiveShaderProgram());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		private CommonMaterialProgram GetEffectiveShaderProgram()
		{
			if (ShadowsRenderingMode) {
				return shadowsProgram ?? (shadowsProgram = GetShaderProgram());
			} else {
				return program ?? (program = GetShaderProgram());
			}
		}

		private CommonMaterialProgram GetShaderProgram()
		{
			CommonMaterialProgram program;
			var spec = new CommonMaterialProgramSpec {
				SkinEnabled = skinEnabled,
				DiffuseTextureEnabled = diffuseTexture != null,
				FogMode = FogMode,
				SkinningMode = SkinningMode,
				ShadowRenderingMode = ShadowsRenderingMode,
			};
			if (!programCache.TryGetValue(spec, out program)) {
				program = new CommonMaterialProgram(spec);
				programCache.Add(spec, program);
			}
			return program;
		}

		public void Invalidate()
		{
			shadowsProgram = program = null;
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
			public readonly ShaderParamKey<Vector4> DualQuaternionA;
			public readonly ShaderParamKey<Vector4> DualQuaternionB;
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
				DualQuaternionA = parameters.GetParamKey<Vector4>("u_DualQuaternionPartA");
				DualQuaternionB = parameters.GetParamKey<Vector4>("u_DualQuaternionPartB");
				FogColor = parameters.GetParamKey<Vector4>("u_FogColor");
				FogStart = parameters.GetParamKey<float>("u_FogStart");
				FogEnd = parameters.GetParamKey<float>("u_FogEnd");
				FogDensity = parameters.GetParamKey<float>("u_FogDensity");
			}
		}
	}
}

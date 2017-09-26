using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzu;

namespace Lime
{
	public class CommonMaterial : IMaterial, IMaterialSkin, IMaterialFog, IMaterialLightning, IMaterialShadowReciever
	{
		private static Dictionary<CommonMaterialProgramSpec, CommonMaterialProgram> programCache =
			new Dictionary<CommonMaterialProgramSpec, CommonMaterialProgram>();

		private CommonMaterialProgram program;
		private ITexture diffuseTexture;
		private Matrix44[] boneTransforms;
		private int boneCount;
		private bool skinEnabled;
		private FogMode fogMode;
		private bool processLightning;
		private LightSource lightSource;
		private bool recieveShadows;
		
		[YuzuMember]
		public string Name { get; set; }

		[YuzuMember]
		public Color4 DiffuseColor { get; set; }

		[YuzuMember]
		public Color4 ColorFactor { get; set; }

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

		[YuzuMember]
		public bool ProcessLightning
		{
			get { return processLightning; }
			set
			{
				if (processLightning != value) {
					processLightning = value;
					program = null;
				}
			}
		}

		public bool RecieveShadows
		{
			get { return recieveShadows; }
			set
			{
				if (recieveShadows != value) {
					recieveShadows = value;
					program = null;
				}
				
			}
		}

		public CommonMaterial()
		{
			DiffuseColor = Color4.White;
			ColorFactor = Color4.White;
			FogColor = Color4.White;
			Blending = Blending.Alpha;
		}

		public void SetLightData(LightSource data)
		{
			lightSource = data;
		}

		public void SetBones(Matrix44[] boneTransforms, int boneCount)
		{
			this.boneTransforms = boneTransforms;
			this.boneCount = boneCount;
		}

		public void Apply()
		{
			PlatformRenderer.SetBlending(Blending);
			PrepareShaderProgram();
			program.Use();
			program.LoadMatrix(program.WorldViewProjUniformId, Renderer.FixupWVP(Renderer.WorldViewProjection));
			program.LoadColor(program.DiffuseColorUniformId, DiffuseColor * ColorFactor);
			program.LoadMatrix(program.WorldUniformId, Renderer.World);

			if (processLightning && lightSource != null) {
				program.LoadColor(program.LightColorUniformId, lightSource.Color);
				program.LoadVector3(program.LightDirectionUniformId, lightSource.Position.Normalized);
				program.LoadFloat(program.LightIntensityUniformId, lightSource.Intensity);

				if (recieveShadows) {
					var lightWVP = Renderer.World * lightSource.ViewProjection * Matrix44.CreateScale(new Vector3(1, -1, 1));
					program.LoadMatrix(program.LightWorldViewProjectionUniformId, lightWVP);
					PlatformRenderer.SetTexture(lightSource.ShadowMap, CommonMaterialProgram.ShadowMapTextureStage);
				}
			}

			if (skinEnabled) {
				program.LoadMatrixArray(program.BonesUniformId, boneTransforms, boneCount);
			}

			if (fogMode != FogMode.None) {
				program.LoadMatrix(program.WorldViewUniformId, Renderer.WorldView);
				program.LoadColor(program.FogColorUniformId, FogColor);
				if (fogMode == FogMode.Linear) {
					program.LoadFloat(program.FogStartUniformId, FogStart);
					program.LoadFloat(program.FogEndUniformId, FogEnd);
				} else {
					program.LoadFloat(program.FogDensityUniformId, FogDensity);
				}
			}

			if (diffuseTexture != null) {
				PlatformRenderer.SetTexture(diffuseTexture, CommonMaterialProgram.DiffuseTextureStage);
			}
		}

		private void PrepareShaderProgram()
		{
			if (program != null) {
				return;
			}
			var spec = new CommonMaterialProgramSpec {
				SkinEnabled = skinEnabled,
				DiffuseTextureEnabled = diffuseTexture != null,
				FogMode = FogMode,
				ProcessLightning = ProcessLightning,
				RecieveShadows = RecieveShadows
			};
			if (programCache.TryGetValue(spec, out program)) {
				return;
			}
			program = new CommonMaterialProgram(spec);
			programCache[spec] = program;
		}

		public IMaterial Clone()
		{
			return new CommonMaterial {
				Name = Name,
				DiffuseColor = DiffuseColor,
				ColorFactor = ColorFactor,
				FogMode = FogMode,
				FogColor = FogColor,
				FogStart = FogStart,
				FogEnd = FogEnd,
				FogDensity = FogDensity,
				Blending = Blending,
				DiffuseTexture = DiffuseTexture,
				SkinEnabled = SkinEnabled,
				ProcessLightning = ProcessLightning,
				RecieveShadows = RecieveShadows
			};
		}
	}
}

using System.Collections.Generic;

namespace Lime
{
	internal class ModelMaterialTechnique : ShaderProgram
	{
		private static readonly ModelMaterialTechnique[] instances;

		private const string VertexShader = @"
			#ifdef GLES
			precision highp float;
			#endif

			attribute vec4 a_Position;
			attribute vec4 a_Color;
			attribute vec2 a_DiffuseUV;
			attribute vec2 a_OpacityUV;
			attribute vec4 a_BlendIndices;
			attribute vec4 a_BlendWeights;

			varying vec4 v_Color;
			varying vec2 v_DiffuseUV;
			varying vec2 v_OpacityUV;

			uniform mat4 u_WorldViewProj;
			uniform mat4 u_Bones[50];
			
			void main()
			{
				vec4 position = a_Position;
			#ifdef SKIN_ENABLED
				mat4 skinTransform =
					u_Bones[int(a_BlendIndices.x)] * a_BlendWeights.x +
					u_Bones[int(a_BlendIndices.y)] * a_BlendWeights.y +
					u_Bones[int(a_BlendIndices.z)] * a_BlendWeights.z +
					u_Bones[int(a_BlendIndices.w)] * a_BlendWeights.w;
				position = skinTransform * position;
			#endif
				v_Color = a_Color;
				v_DiffuseUV = a_DiffuseUV;
				v_OpacityUV = a_OpacityUV;
				gl_Position = u_WorldViewProj * position;
			}
		";

		private const string FragmentShader = @"
			#ifdef GLES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_DiffuseUV;
			varying vec2 v_OpacityUV;
			
			uniform vec4 u_DiffuseColor;
			uniform float u_Opacity;
			uniform sampler2D u_DiffuseTexture;
			uniform sampler2D u_OpacityTexture;

			void main()
			{
				vec4 color = v_Color * u_DiffuseColor;
			#ifdef DIFFUSE_TEXTURE_ENABLED
				color.rgba *= texture2D(u_DiffuseTexture, v_DiffuseUV).rgba;
			#endif
			#ifdef OPACITY_TEXTURE_ENABLED
				color.a *= texture2D(u_OpacityTexture, v_OpacityUV).OPACITY_ALPHA_CHANNEL;
			#endif
				color.a *= u_Opacity;
				gl_FragColor = color;
			}
		";

		private ModelMaterialCap caps;
		private UniformIds uniformIds;

		static ModelMaterialTechnique()
		{
			var allCaps = EnumExtensions.GetAtomicFlags<ModelMaterialCap>();
			var entryCount = 1 << allCaps.Length;
			instances = new ModelMaterialTechnique[entryCount];
			for (var i = 0; i < entryCount; i++) {
				var caps = ModelMaterialCap.None;
				foreach (var cap in allCaps) {
					if ((i & (int)cap) == (int)cap) {
						caps |= cap;
					}
				}
				instances[(int)caps] = new ModelMaterialTechnique(caps);
			}
		}

		public ModelMaterialTechnique(ModelMaterialCap caps)
			: base(GetShaders(caps), GetAttribLocations(), GetSamplers())
		{
			this.caps = caps;
		}

		protected override void InitializeUniformIds()
		{
			uniformIds.WorldViewProj = GetUniformId("u_WorldViewProj");
			uniformIds.DiffuseColor = GetUniformId("u_DiffuseColor");
			uniformIds.Opacity = GetUniformId("u_Opacity");
			uniformIds.Bones = GetUniformId("u_Bones");
		}

		public void Apply(ModelMaterial material, ref ModelMaterialExternals externals)
		{
			Use();
			LoadMatrix(uniformIds.WorldViewProj, externals.WorldViewProj);
			LoadColor(uniformIds.DiffuseColor, material.DiffuseColor);
			LoadFloat(uniformIds.Opacity, material.Opacity);
			if ((caps & ModelMaterialCap.Skin) != 0) {
				LoadMatrixArray(uniformIds.Bones, externals.Bones, externals.BoneCount);
			}
			if ((caps & ModelMaterialCap.DiffuseTexture) != 0) {
				PlatformRenderer.SetTexture(material.DiffuseTexture.GetHandle(), TextureUnits.Diffuse);
			}
			if ((caps & ModelMaterialCap.OpacityTexture) != 0) {
#if ANDROID
				PlatformRenderer.SetTexture(material.OpacityTexture.AlphaTexture.GetHandle(), TextureUnits.Opacity);
#else
				PlatformRenderer.SetTexture(material.OpacityTexture.GetHandle(), TextureUnits.Opacity);
#endif
			}
			PlatformRenderer.SetBlending(Blending.Alpha);
		}

		public static ModelMaterialTechnique Get(ModelMaterialCap caps)
		{
			return instances[(int)caps];
		}

		private static IEnumerable<Shader> GetShaders(ModelMaterialCap caps)
		{
			return new Shader[] {
				new VertexShader(AddDefinitions(VertexShader, caps)),
				new FragmentShader(AddDefinitions(FragmentShader, caps))
			};
		}

		private static IEnumerable<AttribLocation> GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = PlatformMesh.Attributes.Vertex },
				new AttribLocation { Name = "a_Color", Index = PlatformMesh.Attributes.Color },
				new AttribLocation { Name = "a_DiffuseUV", Index = PlatformMesh.Attributes.UV1 },
				new AttribLocation { Name = "a_SpecularUV", Index = PlatformMesh.Attributes.UV2 },
				new AttribLocation { Name = "a_HeightUV", Index = PlatformMesh.Attributes.UV3 },
				new AttribLocation { Name = "a_OpacityUV", Index = PlatformMesh.Attributes.UV4 },
				new AttribLocation { Name = "a_BlendIndices", Index = PlatformMesh.Attributes.BlendIndices },
				new	AttribLocation { Name = "a_BlendWeights", Index = PlatformMesh.Attributes.BlendWeights }
			};
		}

		private static IEnumerable<Sampler> GetSamplers()
		{
			return new Sampler[] {
				new Sampler { Name = "u_DiffuseTexture", Stage = TextureUnits.Diffuse },
				new Sampler { Name = "u_SpecularTexture", Stage = TextureUnits.Specular },
				new Sampler { Name = "u_HeightTexture", Stage = TextureUnits.Height },
				new Sampler { Name = "u_OpacityTexture", Stage = TextureUnits.Opacity }
			};
		}

		private static string AddDefinitions(string shader, ModelMaterialCap caps)
		{
			if ((caps & ModelMaterialCap.DiffuseTexture) != 0) {
				shader = "#define DIFFUSE_TEXTURE_ENABLED\n" + shader;
			}
//			if ((caps & ModelMaterialCap.SpecularTexture) != 0) {
//				shader = "#define SPECULAR_TEXTURE_ENABLED\n" + shader;
//			}
//			if ((caps & ModelMaterialCap.HeightTexture) != 0) {
//				shader = "#define HEIGHT_TEXTURE_ENABLED\n" + shader;
//			}
//			if ((caps & ModelMaterialCap.OpacityTexture) != 0) {
//				shader = "#define OPACITY_TEXTURE_ENABLED\n" + shader;
//#if ANDROID
//				shader = "#define OPACITY_ALPHA_CHANNEL r\n" + shader;
//#else
//				shader = "#define OPACITY_ALPHA_CHANNEL a\n" + shader;
//#endif
//			}
			if ((caps & ModelMaterialCap.Skin) != 0) {
				shader = "#define SKIN_ENABLED\n" + shader;
			}
			return shader;
		}

		private struct UniformIds
		{
			public int WorldViewProj;
			public int DiffuseColor;
			public int Opacity;
			public int Bones;
		}

		private static class TextureUnits
		{
			public const int Diffuse = 0;
			public const int Specular = 1;
			public const int Height = 2;
			public const int Opacity = 3;
		}
	}
}
using System.Collections.Generic;

namespace Lime
{
	internal class MaterialTechnique : ShaderProgram
	{
		private static readonly MaterialTechnique[] instances;

		private const string VertexShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			attribute vec4 a_Position;
			attribute vec4 a_Color;
			attribute vec2 a_UV;
			attribute vec4 a_BlendIndices;
			attribute vec4 a_BlendWeights;

			varying vec4 v_Color;
			varying vec2 v_UV;

			#ifdef FOG_ENABLED
			varying float v_FogFactor;
			uniform float u_FogStart;
			uniform float u_FogEnd;
			uniform float u_FogDensity;
			#endif

			uniform mat4 u_WorldView;
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
				v_UV = a_UV;
			#ifdef FOG_ENABLED
				vec4 viewPos = u_WorldView * position;
				float d = abs(viewPos.z);
			#if defined(FOG_LINEAR)
				v_FogFactor = (d - u_FogStart) / (u_FogEnd - u_FogStart);
			#elif defined(FOG_EXP)
				v_FogFactor = 1.0 - 1.0 / exp(d * u_FogDensity);
			#elif defined(FOG_EXP2)
				v_FogFactor = 1.0 - 1.0 / exp((d * u_FogDensity) * (d * u_FogDensity));
			#endif
				v_FogFactor = clamp(v_FogFactor, 0.0, 1.0);
			#endif
				gl_Position = u_WorldViewProj * position;
			}
		";

		private const string FragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_UV;

			#ifdef FOG_ENABLED
			varying float v_FogFactor;
			uniform vec4 u_FogColor;
			#endif

			uniform vec4 u_DiffuseColor;
			uniform float u_Opacity;
			uniform sampler2D u_DiffuseTexture;
			uniform sampler2D u_OpacityTexture;

			void main()
			{
				vec4 color = v_Color * u_DiffuseColor;
			#ifdef DIFFUSE_TEXTURE_ENABLED
				color.rgba *= texture2D(u_DiffuseTexture, v_UV).rgba;
			#endif
			#ifdef OPACITY_TEXTURE_ENABLED
				color.a *= texture2D(u_OpacityTexture, v_UV).OPACITY_ALPHA_CHANNEL;
			#endif
			#ifdef FOG_ENABLED
				color.rgb = mix(color.rgb, u_FogColor.rgb, v_FogFactor);
			#endif
				color.a *= u_Opacity;
				gl_FragColor = color;
			}
		";

		private MaterialCap caps;
		private UniformIds uniformIds;

		static MaterialTechnique()
		{
			var allCaps = EnumExtensions.GetAtomicFlags<MaterialCap>();
			var entryCount = 1 << allCaps.Length;
			instances = new MaterialTechnique[entryCount];
			for (var i = 0; i < entryCount; i++) {
				var caps = MaterialCap.None;
				foreach (var cap in allCaps) {
					if ((i & (int)cap) == (int)cap) {
						caps |= cap;
					}
				}
				instances[(int)caps] = new MaterialTechnique(caps);
			}
		}

		public MaterialTechnique(MaterialCap caps)
			: base(GetShaders(caps), GetAttribLocations(), GetSamplers())
		{
			this.caps = caps;
		}

		protected override void InitializeUniformIds()
		{
			uniformIds.WorldView = GetUniformId("u_WorldView");
			uniformIds.WorldViewProj = GetUniformId("u_WorldViewProj");
			uniformIds.DiffuseColor = GetUniformId("u_DiffuseColor");
			uniformIds.Opacity = GetUniformId("u_Opacity");
			uniformIds.Bones = GetUniformId("u_Bones");
			uniformIds.FogColor = GetUniformId("u_FogColor");
			uniformIds.FogStart = GetUniformId("u_FogStart");
			uniformIds.FogEnd = GetUniformId("u_FogEnd");
			uniformIds.FogDensity = GetUniformId("u_FogDensity");
		}

		public void Apply(Material material, ref MaterialExternals externals)
		{
			Use();
			LoadMatrix(uniformIds.WorldViewProj, externals.WorldViewProj);
			LoadColor(uniformIds.DiffuseColor, material.DiffuseColor * externals.ColorFactor);
			LoadFloat(uniformIds.Opacity, material.Opacity);
			if ((caps & MaterialCap.Fog) != 0) {
				LoadMatrix(uniformIds.WorldView, externals.WorldView);
				LoadColor(uniformIds.FogColor, material.FogColor);
				if ((caps & MaterialCap.FogLinear) != 0) {
					LoadFloat(uniformIds.FogStart, material.FogStart);
					LoadFloat(uniformIds.FogEnd, material.FogEnd);
				} else {
					LoadFloat(uniformIds.FogDensity, material.FogDensity);
				}
				var c = WidgetContext.Current.CurrentCamera;
				var cp = c.Position;
				var v = c.View;
				var wv = externals.WorldView;
				var v2 = Matrix44.CreateLookAt(new Vector3(0, 0, 45), Vector3.Zero, Vector3.UnitY);
			}
			if ((caps & MaterialCap.Skin) != 0) {
				LoadMatrixArray(uniformIds.Bones, externals.Bones, externals.BoneCount);
			}
			if ((caps & MaterialCap.DiffuseTexture) != 0) {
				PlatformRenderer.SetTexture(material.DiffuseTexture.GetHandle(), TextureUnits.Diffuse);
			}
			if ((caps & MaterialCap.OpacityTexture) != 0) {
#if ANDROID
				PlatformRenderer.SetTexture(material.OpacityTexture.AlphaTexture.GetHandle(), TextureUnits.Opacity);
#else
				PlatformRenderer.SetTexture(material.OpacityTexture.GetHandle(), TextureUnits.Opacity);
#endif
			}
			PlatformRenderer.SetBlending(Blending.Alpha);
		}

		public static MaterialTechnique Get(MaterialCap caps)
		{
			return instances[(int)caps];
		}

		private static IEnumerable<Shader> GetShaders(MaterialCap caps)
		{
			return new Shader[] {
				new VertexShader(AddDefinitions(VertexShader, caps)),
				new FragmentShader(AddDefinitions(FragmentShader, caps))
			};
		}

		private static IEnumerable<AttribLocation> GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = PlatformGeometryBuffer.Attributes.Vertex },
				new AttribLocation { Name = "a_Color", Index = PlatformGeometryBuffer.Attributes.Color },
				new AttribLocation { Name = "a_UV", Index = PlatformGeometryBuffer.Attributes.UV1 },
				new AttribLocation { Name = "a_BlendIndices", Index = PlatformGeometryBuffer.Attributes.BlendIndices },
				new	AttribLocation { Name = "a_BlendWeights", Index = PlatformGeometryBuffer.Attributes.BlendWeights }
			};
		}

		private static IEnumerable<Sampler> GetSamplers()
		{
			return new Sampler[] {
				new Sampler { Name = "u_DiffuseTexture", Stage = TextureUnits.Diffuse },
				new Sampler { Name = "u_OpacityTexture", Stage = TextureUnits.Opacity }
			};
		}

		private static string AddDefinitions(string shader, MaterialCap caps)
		{
			if ((caps & MaterialCap.DiffuseTexture) != 0) {
				shader = "#define DIFFUSE_TEXTURE_ENABLED\n" + shader;
			}
			if ((caps & MaterialCap.Skin) != 0) {
				shader = "#define SKIN_ENABLED\n" + shader;
			}
			if ((caps & MaterialCap.Fog) != 0) {
				shader = "#define FOG_ENABLED\n" + shader;
				if ((caps & MaterialCap.FogLinear) != 0) {
					shader = "#define FOG_LINEAR\n" + shader;
				} else if ((caps & MaterialCap.FogExp) != 0) {
					shader = "#define FOG_EXP\n" + shader;
				} else if ((caps & MaterialCap.FogExp2) != 0) {
					shader = "#define FOG_EXP2\n" + shader;
				}
			}
			return shader;
		}

		private struct UniformIds
		{
			public int WorldView;
			public int WorldViewProj;
			public int DiffuseColor;
			public int Opacity;
			public int Bones;
			public int FogColor;
			public int FogStart;
			public int FogEnd;
			public int FogDensity;
		}

		private static class TextureUnits
		{
			public const int Diffuse = 0;
			public const int Opacity = 1;
		}
	}
}
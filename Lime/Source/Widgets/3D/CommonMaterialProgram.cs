namespace Lime
{
	internal class CommonMaterialProgram : ShaderProgram
	{
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
			varying vec4 v_ViewPos;

			uniform vec4 u_ColorFactor;
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
				v_Color = a_Color * u_ColorFactor;
				v_UV = a_UV;
				v_ViewPos = u_WorldView * position;
				gl_Position = u_WorldViewProj * position;
			}
		";

		private const string FragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_UV;
			varying vec4 v_ViewPos;
			
			#ifdef FOG_ENABLED
			uniform float u_FogStart;
			uniform float u_FogEnd;
			uniform float u_FogDensity;
			uniform vec4 u_FogColor;
			#endif

			uniform vec4 u_DiffuseColor;
			uniform sampler2D u_DiffuseTexture;

			void main()
			{
				vec4 color = v_Color * u_DiffuseColor;
			#ifdef DIFFUSE_TEXTURE_ENABLED
				color.rgba *= texture2D(u_DiffuseTexture, v_UV).rgba;
			#endif
			#ifdef FOG_ENABLED
				float d = abs(v_ViewPos.z);
			#if defined(FOG_LINEAR)
				float fogFactor = (d - u_FogStart) / (u_FogEnd - u_FogStart);
			#elif defined(FOG_EXP)
				float fogFactor = 1.0 - 1.0 / exp(d * u_FogDensity);
			#elif defined(FOG_EXP_SQUARED)
				float fogFactor = 1.0 - 1.0 / exp((d * u_FogDensity) * (d * u_FogDensity));
			#endif
				fogFactor = clamp(fogFactor, 0.0, 1.0);
				color.rgb = mix(color.rgb, u_FogColor.rgb, fogFactor);
			#endif
				gl_FragColor = color;
			}
		";

		public const int DiffuseTextureStage = 0;

		public CommonMaterialProgram(CommonMaterialProgramSpec spec)
			: base(GetShaders(spec), GetAttribLocations(), GetSamplers(spec))
		{ }

		private static Shader[] GetShaders(CommonMaterialProgramSpec spec)
		{
			var preamble = BuildPreamble(spec);
			return new Shader[] {
				new VertexShader(preamble + VertexShader),
				new FragmentShader(preamble + FragmentShader)
			};
		}

		private static string BuildPreamble(CommonMaterialProgramSpec spec)
		{
			var preamble = "";
			if (spec.SkinEnabled) {
				preamble += "#define SKIN_ENABLED\n";
			}
			if (spec.DiffuseTextureEnabled) {
				preamble += "#define DIFFUSE_TEXTURE_ENABLED\n";
			}
			if (spec.FogMode != FogMode.None) {
				preamble += "#define FOG_ENABLED\n";
				if (spec.FogMode == FogMode.Linear) {
					preamble += "#define FOG_LINEAR\n";
				} else if (spec.FogMode == FogMode.Exp) {
					preamble += "#define FOG_EXP\n";
				} else if (spec.FogMode == FogMode.ExpSquared) {
					preamble += "#define FOG_EXP_SQUARED\n";
				}
			}
			return preamble;
		}

		private static Sampler[] GetSamplers(CommonMaterialProgramSpec spec)
		{
			return new[] {
				new Sampler { Name = "u_DiffuseTexture", Stage = DiffuseTextureStage }
			};
		}

		private static AttribLocation[] GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = ShaderPrograms.Attributes.Pos1 },
				new AttribLocation { Name = "a_UV", Index = ShaderPrograms.Attributes.UV1 },
				new AttribLocation { Name = "a_Color", Index = ShaderPrograms.Attributes.Color1 },
				new AttribLocation { Name = "a_BlendIndices", Index = ShaderPrograms.Attributes.BlendIndices },
				new AttribLocation { Name = "a_BlendWeights", Index = ShaderPrograms.Attributes.BlendWeights }
			};
		}
	}

	internal struct CommonMaterialProgramSpec
	{
		public bool SkinEnabled;
		public bool DiffuseTextureEnabled;
		public FogMode FogMode;
	}
}
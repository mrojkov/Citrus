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
			#elif defined(FOG_EXP_SQUARED)
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
			uniform sampler2D u_DiffuseTexture;

			void main()
			{
				vec4 color = v_Color * u_DiffuseColor;
			#ifdef DIFFUSE_TEXTURE_ENABLED
				color.rgb *= texture2D(u_DiffuseTexture, v_UV).rgb;
			#endif
			#ifdef FOG_ENABLED
				color.rgb = mix(color.rgb, u_FogColor.rgb, v_FogFactor);
			#endif
				color.rgb *= color.a;
				gl_FragColor = color;
			}
		";

		public int WorldViewUniformId;
		public int WorldViewProjUniformId;
		public int DiffuseColorUniformId;
		public int OpacityUniformId;
		public int BonesUniformId;
		public int FogColorUniformId;
		public int FogStartUniformId;
		public int FogEndUniformId;
		public int FogDensityUniformId;

		public const int DiffuseTextureStage = 0;

		public CommonMaterialProgram(CommonMaterialProgramSpec spec)
			: base(GetShaders(spec), GetAttribLocations(), GetSamplers())
		{
		}

		protected override void InitializeUniformIds()
		{
			WorldViewUniformId = GetUniformId("u_WorldView");
			WorldViewProjUniformId = GetUniformId("u_WorldViewProj");
			DiffuseColorUniformId = GetUniformId("u_DiffuseColor");
			OpacityUniformId = GetUniformId("u_Opacity");
			BonesUniformId = GetUniformId("u_Bones");
			FogColorUniformId = GetUniformId("u_FogColor");
			FogStartUniformId = GetUniformId("u_FogStart");
			FogEndUniformId = GetUniformId("u_FogEnd");
			FogDensityUniformId = GetUniformId("u_FogDensity");
		}

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

		private static Sampler[] GetSamplers()
		{
			return new Sampler[] {
				new Sampler { Name = "u_DiffuseTexture", Stage = DiffuseTextureStage }
			};
		}

		private static AttribLocation[] GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = PlatformGeometryBuffer.Attributes.Vertex },
				new AttribLocation { Name = "a_UV", Index = PlatformGeometryBuffer.Attributes.UV1 },
				new AttribLocation { Name = "a_Color", Index = PlatformGeometryBuffer.Attributes.Color },
				new AttribLocation { Name = "a_BlendIndices", Index = PlatformGeometryBuffer.Attributes.BlendIndices },
				new AttribLocation { Name = "a_BlendWeights", Index = PlatformGeometryBuffer.Attributes.BlendWeights }
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
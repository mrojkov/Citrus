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
			attribute vec4 a_Normal;

			varying vec4 v_Color;
			varying vec2 v_UV;
			varying vec3 v_Normal;

			#ifdef FOG_ENABLED
			varying float v_FogFactor;
			uniform float u_FogStart;
			uniform float u_FogEnd;
			uniform float u_FogDensity;
			#endif

			uniform mat4 u_World;
			uniform mat4 u_WorldView;
			uniform mat4 u_WorldViewProj;
			uniform mat4 u_Bones[50];

			#ifdef RECIEVE_SHADOWS
			varying vec4 v_ShadowCoord;
			uniform mat4 u_LightWorldViewProjection;
			#endif

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

			#ifdef RECIEVE_SHADOWS
				v_ShadowCoord = u_LightWorldViewProjection * position;
			#endif
				
				gl_Position = u_WorldViewProj * position;
				v_Normal = mat3(u_World) * a_Normal.xyz;
			}
		";

		private const string FragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_UV;
			varying vec3 v_Normal;

			#ifdef FOG_ENABLED
			varying float v_FogFactor;
			uniform vec4 u_FogColor;
			#endif

			uniform vec4 u_DiffuseColor;
			uniform sampler2D u_DiffuseTexture;

			#ifdef RECIEVE_SHADOWS
			varying vec4 v_ShadowCoord;
			uniform sampler2D u_ShadowMapTexture;
			#endif

			#ifdef LIGHTNING_ENABLED
			uniform vec3 u_LightDirection;
			uniform vec4 u_LightColor;
			uniform float u_LightIntensity;
			#endif

			void main()
			{
				vec4 color = v_Color * u_DiffuseColor;
			#ifdef DIFFUSE_TEXTURE_ENABLED
				color.rgba *= texture2D(u_DiffuseTexture, v_UV).rgba;
			#endif
			#ifdef FOG_ENABLED
				color.rgb = mix(color.rgb, u_FogColor.rgb, v_FogFactor);
			#endif
				
			#ifdef LIGHTNING_ENABLED
				vec3 normal = normalize(v_Normal);
				float light = dot(normal, u_LightDirection);
				float visibility = 1.0;

			#ifdef RECIEVE_SHADOWS

				vec2 shadowUV = (v_ShadowCoord.xy + vec2(1.0)) / 2.0;
				float bias = clamp(0.005 * tan(acos(clamp(light, 0, 0.75))), 0, 0.005);
			#ifdef SHADOW_SAMPLING
				vec2 poissonDisk[4];
				poissonDisk[0] = vec2( -0.94201624, -0.39906216 );
				poissonDisk[1] = vec2( 0.94558609, -0.76890725 );
				poissonDisk[2] = vec2( -0.094184101, -0.92938870 );
				poissonDisk[3] = vec2( 0.34495938, 0.29387760 );
				
				for (int i = 0; i < 4; i++) {
					if (texture2D(u_ShadowMapTexture, shadowUV.xy + poissonDisk[i] / 700.0).z < v_ShadowCoord.z - bias) {
						visibility -= 0.2;
					}
				}
			#else
				if (texture2D(u_ShadowMapTexture, shadowUV.xy).z < v_ShadowCoord.z - bias) {
					visibility = 0.2;
				}
			#endif

			#endif

				color.rgb *= (max(0.25, visibility * light * u_LightIntensity) * u_LightColor.rgb);
			#endif

				gl_FragColor = color;
			}
		";

		public int WorldUniformId;
		public int WorldViewUniformId;
		public int WorldViewProjUniformId;
		public int DiffuseColorUniformId;
		public int OpacityUniformId;
		public int BonesUniformId;
		public int FogColorUniformId;
		public int FogStartUniformId;
		public int FogEndUniformId;
		public int FogDensityUniformId;
		public int LightColorUniformId;
		public int LightDirectionUniformId;
		public int LightIntensityUniformId;
		public int LightWorldViewProjectionUniformId;

		public const int DiffuseTextureStage = 0;
		public const int ShadowMapTextureStage = 1;

		public CommonMaterialProgram(CommonMaterialProgramSpec spec)
			: base(GetShaders(spec), GetAttribLocations(), GetSamplers(spec))
		{ }

		protected override void InitializeUniformIds()
		{
			WorldUniformId = GetUniformId("u_World");
			WorldViewUniformId = GetUniformId("u_WorldView");
			WorldViewProjUniformId = GetUniformId("u_WorldViewProj");
			DiffuseColorUniformId = GetUniformId("u_DiffuseColor");
			OpacityUniformId = GetUniformId("u_Opacity");
			BonesUniformId = GetUniformId("u_Bones");
			FogColorUniformId = GetUniformId("u_FogColor");
			FogStartUniformId = GetUniformId("u_FogStart");
			FogEndUniformId = GetUniformId("u_FogEnd");
			FogDensityUniformId = GetUniformId("u_FogDensity");
			LightColorUniformId = GetUniformId("u_LightColor");
			LightDirectionUniformId = GetUniformId("u_LightDirection");
			LightIntensityUniformId = GetUniformId("u_LightIntensity");
			LightWorldViewProjectionUniformId = GetUniformId("u_LightWorldViewProjection");
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
			if (spec.ProcessLightning) {
				preamble += "#define LIGHTNING_ENABLED\n";
			}
			if (spec.RecieveShadows) {
				preamble += "#define RECIEVE_SHADOWS\n";
				if (spec.SoftShadow) {
					preamble += "#define _SHADOW_SAMPLING\n";
				}
			}

			return preamble;
		}

		private static Sampler[] GetSamplers(CommonMaterialProgramSpec spec)
		{
			var samplers = new System.Collections.Generic.List<Sampler>(2);
			samplers.Add(new Sampler { Name = "u_DiffuseTexture", Stage = DiffuseTextureStage });
			if (spec.RecieveShadows) {
				samplers.Add(new Sampler { Name = "u_ShadowMapTexture", Stage = ShadowMapTextureStage });
			}

			return samplers.ToArray();

		}

		private static AttribLocation[] GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = ShaderPrograms.Attributes.Pos1 },
				new AttribLocation { Name = "a_UV", Index = ShaderPrograms.Attributes.UV1 },
				new AttribLocation { Name = "a_Color", Index = ShaderPrograms.Attributes.Color1 },
				new AttribLocation { Name = "a_BlendIndices", Index = ShaderPrograms.Attributes.BlendIndices },
				new AttribLocation { Name = "a_BlendWeights", Index = ShaderPrograms.Attributes.BlendWeights },
				new AttribLocation { Name = "a_Normal", Index =  ShaderPrograms.Attributes.Normal }
			};
		}
	}

	internal struct CommonMaterialProgramSpec
	{
		public bool SkinEnabled;
		public bool DiffuseTextureEnabled;
		public FogMode FogMode;
		public bool ProcessLightning;
		public bool RecieveShadows;
		public bool SoftShadow;
	}
}
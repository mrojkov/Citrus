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
			varying vec4 v_ViewPos;

			uniform vec4 u_ColorFactor;
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
				v_Color = a_Color * u_ColorFactor;
				v_UV = a_UV;

			#ifdef RECIEVE_SHADOWS
				v_ShadowCoord = u_LightWorldViewProjection * position;
			#endif
				
				gl_Position = u_WorldViewProj * position;
				v_Normal = mat3(u_World[0].xyz, u_World[1].xyz, u_World[2].xyz) * a_Normal.xyz;
				v_ViewPos = u_WorldView * position;
			}
		";

		private const string FragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_UV;
			varying vec3 v_Normal;
			varying vec4 v_ViewPos;
			
			#ifdef FOG_ENABLED
			uniform float u_FogStart;
			uniform float u_FogEnd;
			uniform float u_FogDensity;
			uniform vec4 u_FogColor;
			#endif

			uniform vec4 u_DiffuseColor;
			uniform sampler2D u_DiffuseTexture;

			#ifdef RECIEVE_SHADOWS
			varying vec4 v_ShadowCoord;
			uniform sampler2D u_ShadowMapTexture;
			uniform vec4 u_ShadowColor;
			#endif

			#ifdef LIGHTNING_ENABLED
			uniform float u_LightStrength;
			uniform vec3 u_LightDirection;
			uniform vec4 u_LightColor;
			uniform float u_LightIntensity;
			uniform float u_AmbientLight;
			#endif

			#ifdef RECIEVE_SHADOWS
			float texture2DPCF4(sampler2D shadowMap, vec2 uv)
			{
				float x,y,r;

				for (x = -0.5; x <= 0.5; x += 1.0) {
					for (y = -0.5; y <= 0.5; y += 1.0) {
						r += texture2D(shadowMap, uv + vec2(x * SHADOW_MAP_TEXEL_X, y * SHADOW_MAP_TEXEL_Y)).z;
					}
				}
				
				return r / 4.0;
			}

			float texture2DPCF16(sampler2D shadowMap, vec2 uv)
			{
				float x,y,r;

				for (x = -1.5; x <= 1.5; x += 1.0) {
					for (y = -1.5; y <= 1.5; y += 1.0) {
						r += texture2D(shadowMap, uv + vec2(x * SHADOW_MAP_TEXEL_X, y * SHADOW_MAP_TEXEL_Y)).z;
					}
				}
				
				return r / 16.0;
			}
			#endif

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
				
			#ifdef LIGHTNING_ENABLED
				vec3 normal = normalize(v_Normal);
				float light = dot(normal, u_LightDirection);
				vec3 shadowColor = vec3(1.0, 1.0, 1.0);

			#ifdef RECIEVE_SHADOWS
				vec2 shadowUV = (v_ShadowCoord.xy + vec2(1.0)) / 2.0;
				float bias = clamp(0.005 * tan(acos(clamp(light, 0.0, 0.75))), 0.0, 0.005);
				float visibility = 1.0;
				float shadowFactor = 0.0;
			#ifdef SMOOTH_SHADOW
				shadowFactor = 
					clamp((v_ShadowCoord.z - bias) - TEXTURE_PCF(u_ShadowMapTexture, shadowUV.xy), 0.0, 0.0125) * 
					(80.0 * u_ShadowColor.a);

				visibility = 1.0 - shadowFactor;
			#else
				if (texture2D(u_ShadowMapTexture, shadowUV.xy).z < v_ShadowCoord.z - bias) {
					shadowFactor = 1.0 * u_ShadowColor.a;
					visibility = 1.0 - shadowFactor;
				}
			#endif // SMOOTH_SHADOW
				shadowColor = visibility + (shadowFactor * u_ShadowColor);
			#endif // RECIEVE_SHADOWS

				color.rgb *= 
					clamp((light * u_LightIntensity), u_AmbientLight, u_LightStrength * max(u_AmbientLight, u_LightIntensity)) * 
					shadowColor * 
					u_LightColor.a * u_LightColor.rgb;
			#endif // LIGHTNING_ENABLED

				gl_FragColor = color;
			}
		";

		public const int DiffuseTextureStage = 0;
		public const int ShadowMapTextureStage = 1;

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
			if (spec.ProcessLightning) {
				preamble += "#define LIGHTNING_ENABLED\n";
			}
			if (spec.RecieveShadows) {
				preamble += "#define RECIEVE_SHADOWS\n";
				preamble += "#define SHADOW_MAP_TEXEL_X " + (1f / (spec.ShadowMapSize.X <= 0 ? 1024 : spec.ShadowMapSize.X)) + "\n";
				preamble += "#define SHADOW_MAP_TEXEL_Y " + (1f / (spec.ShadowMapSize.Y <= 0 ? 1024 : spec.ShadowMapSize.Y)) + "\n";

				if (spec.SmoothShadows) {
					preamble += "#define SMOOTH_SHADOW\n";
					preamble += "#define TEXTURE_PCF " + (spec.HighQualitySmoothShadows ? "texture2DPCF16" : "texture2DPCF4") + "\n";
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
		public bool SmoothShadows;
		public bool HighQualitySmoothShadows;
		public IntVector2 ShadowMapSize;
	}
}
using System;

namespace Lime
{
	public class AlphaIntensityMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> radiusKey;
		private readonly ShaderParamKey<float> brightnessKey;
		private readonly ShaderParamKey<Vector4> colorKey;

		public float Radius = 0;
		public float Brightness = 1;
		public Vector4 Color = Vector4.One;
		public ITexture MaskTexture;
		public Blending Blending;

		public string Id { get; set; }
		public int PassCount => 1;

		public AlphaIntensityMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			radiusKey = shaderParams.GetParamKey<float>("radius");
			brightnessKey = shaderParams.GetParamKey<float>("brightness");
			colorKey = shaderParams.GetParamKey<Vector4>("glowColor");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(radiusKey, Radius);
			shaderParams.Set(brightnessKey, Brightness);
			shaderParams.Set(colorKey, Color);
			PlatformRenderer.SetTexture(1, MaskTexture);
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(AlphaIntensityShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new AlphaIntensityMaterial() {
			Radius = Radius,
			Brightness = Brightness,
			Color = Color,
			MaskTexture = MaskTexture
		};

		public class AlphaIntensityShaderProgram : ShaderProgram
		{
			private static AlphaIntensityShaderProgram instance;
			public static AlphaIntensityShaderProgram Instance => instance ?? (instance = new AlphaIntensityShaderProgram());

			private const string VertexShader = @"
				attribute vec4 inPos;
				attribute vec4 inColor;
				attribute vec2 inTexCoords1;

				uniform mat4 matProjection;

				varying lowp vec2 texCoords1;
				varying lowp vec4 outColor;

				void main()
				{
					gl_Position = matProjection * inPos;
					texCoords1 = inTexCoords1;
					outColor = inColor;
				}";

			private const string FragmentShader = @"
				uniform lowp sampler2D tex1;
				uniform lowp sampler2D tex2;
				uniform lowp float radius;
				uniform lowp float brightness;
				uniform lowp vec4 glowColor;

				varying lowp vec2 texCoords1;
				varying lowp vec4 outColor;

				void main()
				{
					lowp vec4 color = texture2D(tex1, texCoords1);
					lowp float mask = max(0.0, texture2D(tex2, texCoords1).r - radius);
					gl_FragColor = (1.0 - color.a * (1.0 - mask))
								 * brightness * vec4((glowColor.rgb + vec3(0.2 * mask)), mask)
								 + outColor * color * color.a;
				}";

			private AlphaIntensityShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

			private static Shader[] CreateShaders()
			{
				return new Shader[] {
					new VertexShader(VertexShader),
					new FragmentShader(FragmentShader)
				};
			}
		}
	}
}

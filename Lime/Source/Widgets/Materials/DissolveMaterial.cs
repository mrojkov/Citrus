using System;

namespace Lime
{
	public class DissolveMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> brightnessKey;
		private readonly ShaderParamKey<Vector2> rangeKey;
		private readonly ShaderParamKey<Vector4> colorKey;

		public float Brightness = 1;
		public Vector2 Range = new Vector2(0.0f, -0.001f);
		public Vector4 Color = Vector4.One;
		public ITexture MaskTexture;
		public Func<Blending> BlendingGetter;

		public string Id { get; set; }
		public int PassCount => 1;

		public DissolveMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			brightnessKey = shaderParams.GetParamKey<float>("brightness");
			rangeKey = shaderParams.GetParamKey<Vector2>("range");
			colorKey = shaderParams.GetParamKey<Vector4>("glowColor");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(brightnessKey, Brightness);
			shaderParams.Set(rangeKey, Range);
			shaderParams.Set(colorKey, Color);
			PlatformRenderer.SetTexture(1, MaskTexture);
			PlatformRenderer.SetBlendState(BlendingGetter.Invoke().GetBlendState());
			PlatformRenderer.SetShaderProgram(DissolveMaterialShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new DissolveMaterial() {
			Brightness = Brightness,
			Range = Range,
			Color = Color,
			MaskTexture = MaskTexture
		};

		public class DissolveMaterialShaderProgram : ShaderProgram
		{
			private static DissolveMaterialShaderProgram instance;
			public static DissolveMaterialShaderProgram Instance => instance ?? (instance = new DissolveMaterialShaderProgram());

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
				uniform lowp float brightness;
				uniform lowp vec2 range;
				uniform lowp vec4 glowColor;

				varying lowp vec2 texCoords1;
				varying lowp vec4 outColor;

				void main()
				{
					lowp vec4 color = texture2D(tex1, texCoords1);
					lowp float mask = texture2D(tex2, texCoords1).r;
					lowp float rl = (range.y - range.x);
					lowp float glow = (0.5 * rl - abs(mask - 0.5 * (range.y + range.x))) / rl;
					gl_FragColor = step(0.5 * (range.y + range.x), mask) * color * outColor
								 + color.a * step(range.x, mask) * (1.0 - step(range.y, mask))
								 * vec4(brightness * (glowColor.rgb + vec3(glow, glow, glow)), glow);
				}";

			private DissolveMaterialShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

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

namespace Lime
{
	public class SoftLightMaterial : IMaterial
	{
		private readonly BlendState blendState;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> strengthKey;

		public float Strength { get; set; } = 1f;

		public int PassCount => 1;

		public SoftLightMaterial()
		{
			blendState = BlendState.Default;
			blendState.SrcBlend = Blend.SrcAlpha;
			blendState.DstBlend = Blend.InverseSrcAlpha;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			strengthKey = shaderParams.GetParamKey<float>("strength");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(strengthKey, Strength);
			PlatformRenderer.SetBlendState(blendState);
			PlatformRenderer.SetShaderProgram(SoftLightShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new SoftLightMaterial();
	}

	public class SoftLightShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform mat4 matProjection;
			uniform mat4 globalTransform;
			uniform vec4 globalColor;
			uniform highp float morphKoeff;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
				texCoords2 = inTexCoords2;
			}";

		private const string FragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;

			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			uniform lowp float strength;

			lowp float blendSoftLight(lowp float base, lowp float blend) {
				return (blend<0.5) ? (2.0*base*blend+base*base*(1.0-2.0*blend)) : (sqrt(base)*(2.0*blend-1.0)+2.0*base*(1.0-blend));
			}

			lowp vec3 blendSoftLight(lowp vec3 base, lowp vec3 blend) {
				return vec3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b)) * strength + base * (1.0 - strength);
			}

			void main() {
				lowp vec4 srcColor = texture2D(tex1, texCoords1);
				lowp vec4 noiseColor = texture2D(tex2, texCoords2);
				lowp vec3 c = blendSoftLight(srcColor.rgb * color.rgb, noiseColor.rgb);
				gl_FragColor = vec4(c.rgb, srcColor.a * color.a);
			}";

		private static SoftLightShaderProgram instance;

		public static SoftLightShaderProgram GetInstance() => instance ?? (instance = new SoftLightShaderProgram());

		private SoftLightShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders() => new Shader[] {
			new VertexShader(VertexShader),
			new FragmentShader(FragmentShader)
		};
	}
}

using Yuzu;

namespace Lime
{
	public class BloomMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> brightThresholdKey;
		private readonly ShaderParamKey<Vector3> inversedGammaCorrectionKey;

		[YuzuMember]
		public float BrightThreshold { get; set; } = 1f;
		[YuzuMember]
		public Vector3 InversedGammaCorrection { get; set; } = Vector3.One;

		public string Id { get; set; }
		public int PassCount => 1;

		public BloomMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			brightThresholdKey = shaderParams.GetParamKey<float>("brightThreshold");
			inversedGammaCorrectionKey = shaderParams.GetParamKey<Vector3>("inversedGammaCorrection");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(brightThresholdKey, BrightThreshold);
			shaderParams.Set(inversedGammaCorrectionKey, InversedGammaCorrection);
			PlatformRenderer.SetBlendState(disabledBlendingState);
			PlatformRenderer.SetShaderProgram(BloomShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }
	}

	public class BloomShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inColor;
			attribute vec2 inTexCoords1;

			uniform mat4 matProjection;

			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
			}";

		private const string FragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			uniform lowp sampler2D tex1;
			uniform lowp float brightThreshold;
			uniform lowp vec3 inversedGammaCorrection;

			void main() {
				lowp vec3 luminanceVector = vec3(0.2125, 0.7154, 0.0721);
				lowp vec4 c = texture2D(tex1, texCoords1) * color;
				lowp float luminance = dot(luminanceVector, c.xyz);
				luminance = max(0.0, luminance - brightThreshold);
				c.xyz *= sign(luminance);
				c.xyz = pow(c.xyz, inversedGammaCorrection);
				c.a = 1.0;
				gl_FragColor = c;
			}
			";

		private static BloomShaderProgram instance;

		public static BloomShaderProgram GetInstance() => instance ?? (instance = new BloomShaderProgram());

		private BloomShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders() => new Shader[] {
			new VertexShader(VertexShader),
			new FragmentShader(FragmentShader)
		};
	}
}

using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class SignedDistanceFieldMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector2> stepKey;
		private readonly ShaderParamKey<Vector3> sharpnessKey;

		public float Strength { get; set; } = 5f;
		public float Limit { get; set; } = 0.1f;
		public Vector2 Step { get; set; } = Vector2.One * (1f / 128f);

		public int PassCount => 1;

		public SignedDistanceFieldMaterial() : this(Blending.Alpha) { }

		public SignedDistanceFieldMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			stepKey = shaderParams.GetParamKey<Vector2>("step");
			sharpnessKey = shaderParams.GetParamKey<Vector3>("uSharpness");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(stepKey, Step);
			shaderParams.Set(sharpnessKey, new Vector3(Strength, Strength * 0.25f, Limit));
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SharpenShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SharpenMaterial(blending) {
				Strength = Strength,
				Limit = Limit,
				Step = Step
			};
		}
	}

	public class SDFShaderProgram : ShaderProgram
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
			uniform lowp float contrast;

			void main() {
				lowp vec3 c = texture2D(tex1, texCoords1).xxx;
				gl_FragColor = vec4((c-0.5)*contrast,1.0);
			";

		private static SDFShaderProgram instance;

		public static SDFShaderProgram GetInstance() => instance ?? (instance = new SDFShaderProgram());

		private SDFShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

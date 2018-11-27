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
		private readonly ShaderParamKey<float> contrastKey;

		public float Contrast { get; set; } = 30f;

		public int PassCount => 1;

		public SignedDistanceFieldMaterial() : this(Blending.Alpha) { }

		public SignedDistanceFieldMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			contrastKey = shaderParams.GetParamKey<float>("contrast");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(contrastKey, Contrast);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SignedDistanceFieldMaterial(blending) {
				Contrast = Contrast
			};
		}
	}

	public class SDFShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec2 inTexCoords1;

			uniform mat4 matProjection;

			varying lowp vec2 texCoords1;

			void main()
			{
				gl_Position = matProjection * inPos;
				texCoords1 = inTexCoords1;
			}";

		private const string FragmentShader = @"
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float contrast;

			void main() {
				lowp vec3 c = texture2D(tex1, texCoords1).xxx;
				gl_FragColor = vec4((c-0.5)*contrast,1.0);
			}";

		private const string FragmentShaderSmooth = @"
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float contrast;

			void main() {
				lowp vec3 c = texture2D(tex1, texCoords1).xxx;
				lowp vec3 res = smoothstep(.5-contrast, .5+contrast, c);
				gl_FragColor = vec4(res,1.0);
			}";

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

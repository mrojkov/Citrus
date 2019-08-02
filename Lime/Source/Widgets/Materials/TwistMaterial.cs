using System;

namespace Lime
{
	public class TwistMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> angleKey;

		public float Angle = 0;
		public Func<Blending> BlendingGetter;

		public string Id { get; set; }
		public int PassCount => 1;

		public TwistMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			angleKey = shaderParams.GetParamKey<float>("angle");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(angleKey, Angle);
			PlatformRenderer.SetBlendState(BlendingGetter.Invoke().GetBlendState());
			PlatformRenderer.SetShaderProgram(TwistShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new TwistMaterial() {
			Angle = Angle,
		};

		public class TwistShaderProgram : ShaderProgram
		{
			private static TwistShaderProgram instance;
			public static TwistShaderProgram Instance => instance ?? (instance = new TwistShaderProgram());

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
					texCoords1 = inTexCoords1 - vec2(0.5, 0.5);
					outColor = inColor;
				}";

			private const string FragmentShader = @"
				uniform lowp sampler2D tex1;
				uniform lowp float angle;

				varying lowp vec2 texCoords1;
				varying lowp vec4 outColor;

				void main()
				{
					lowp float newAngle = (1.0 - length(texCoords1) * 1.3888) * 0.8;
					newAngle = angle * newAngle * newAngle * newAngle;

					lowp float cosAngle = cos(newAngle);
					lowp float sinAngle = sin(newAngle);

					lowp vec4 color = texture2D(tex1,
						mat2(cosAngle, -sinAngle, sinAngle, cosAngle) * texCoords1 + vec2(0.5, 0.5)
					);
					gl_FragColor = color * outColor;
				}";

			private TwistShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

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

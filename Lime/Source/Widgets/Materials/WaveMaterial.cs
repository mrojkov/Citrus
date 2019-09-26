using System;

namespace Lime
{
	public class WaveMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> timeKey;
		private readonly ShaderParamKey<Vector2> frequencyKey;
		private readonly ShaderParamKey<Vector2> pointKey;
		private readonly ShaderParamKey<Vector2> phaseKey;
		private readonly ShaderParamKey<Vector2> amplitudeKey;
		private readonly ShaderParamKey<Vector2> uv0Key;
		private readonly ShaderParamKey<Vector2> uv1Key;

		public Vector2 Frequency { get; set; } = new Vector2(0, 0);
		public Vector2 Phase { get; set; } = new Vector2(0, 0);
		public Vector2 Point { get; set; } = new Vector2(0.5f, 0.5f);
		public Vector2 Amplitude { get; set; } = new Vector2(0.05f, 0.05f);
		public Vector2 UV0 { get; set; }
		public Vector2 UV1 { get; set; }
		public Blending Blending { get; set; }

		public string Id { get; set; }
		public int PassCount => 1;

		public WaveMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			timeKey = shaderParams.GetParamKey<float>("time");
			frequencyKey = shaderParams.GetParamKey<Vector2>("frequency");
			pointKey = shaderParams.GetParamKey<Vector2>("point");
			phaseKey = shaderParams.GetParamKey<Vector2>("phase");
			amplitudeKey = shaderParams.GetParamKey<Vector2>("amplitude");
			uv0Key = shaderParams.GetParamKey<Vector2>("uv0");
			uv1Key = shaderParams.GetParamKey<Vector2>("uv1");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(frequencyKey, Frequency);
			shaderParams.Set(pointKey, Point);
			shaderParams.Set(phaseKey, 2f * Mathf.Pi * Phase);
			shaderParams.Set(amplitudeKey, Amplitude);
			shaderParams.Set(uv0Key, UV0);
			shaderParams.Set(uv1Key, UV1);
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(WaveShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new WaveMaterial() {
			Frequency = Frequency,
			Point = Point,
			Phase = Phase,
			Amplitude = Amplitude,
		};

		public class WaveShaderProgram : ShaderProgram
		{
			private static WaveShaderProgram instance;
			public static WaveShaderProgram Instance => instance ?? (instance = new WaveShaderProgram());

			private const string VertexShader = @"
				attribute vec4 inPos;
				attribute vec4 inColor;
				attribute vec2 inTexCoords1;

				uniform mat4 matProjection;

				varying lowp vec2 uv;
				varying lowp vec4 color;

				void main()
				{
					gl_Position = matProjection * inPos;
					uv = inTexCoords1;
					color = inColor;
				}";

			private const string FragmentShader = @"
				uniform lowp sampler2D tex1;
				uniform lowp vec2 frequency;
				uniform lowp vec2 point;
				uniform lowp vec2 phase;
				uniform lowp vec2 amplitude;
				uniform lowp vec2 uv0;
				uniform lowp vec2 uv1;

				varying lowp vec2 uv;
				varying lowp vec4 color;

				lowp float SoftClamp(lowp float f)
				{
					lowp float x = abs(f * 2.0 - 1.0);
					return 1.0 - x * x * x;
				}

				void main()
				{
					lowp vec2 localUV = (uv - uv0) / (uv1 - uv0);
					lowp vec2 coef = vec2(1.0 - distance(point, localUV), 0.0);
					coef.y = coef.x;
					coef.x = sin(frequency.x * coef.x + phase.x) * amplitude.x * SoftClamp(localUV.x);
					coef.y = sin(frequency.y * coef.y + phase.y) * amplitude.y * SoftClamp(localUV.y);
					lowp vec2 dir = coef * normalize(point - localUV);
					gl_FragColor = texture2D(tex1, uv0 + (localUV + dir) * (uv1 - uv0)) * color;
				}";


			private WaveShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

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

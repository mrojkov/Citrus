using System;

namespace Lime
{
	public class WaveMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> timeKey;
		private readonly ShaderParamKey<Vector2> phaseKey;
		private readonly ShaderParamKey<Vector2> pointKey;
		private readonly ShaderParamKey<Vector2> frequencyKey;
		private readonly ShaderParamKey<Vector2> amplitudeKey;
		private readonly ShaderParamKey<Vector2> uv0Key;
		private readonly ShaderParamKey<Vector2> uv1Key;

		private long frameCount = 0;

		public bool AutoLoopEnabled = false;
		public float Time = 0;
		public Vector2 Phase = new Vector2(30, 30);
		public Vector2 Point = new Vector2(0.5f, 0.5f);
		public Vector2 Frequency = new Vector2(4, 0);
		public Vector2 Amplitude = new Vector2(0.05f, 0.05f);
		public Vector2 UV0;
		public Vector2 UV1;
		public Blending Blending;

		public string Id { get; set; }
		public int PassCount => 1;

		public WaveMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			timeKey = shaderParams.GetParamKey<float>("time");
			phaseKey = shaderParams.GetParamKey<Vector2>("phase");
			pointKey = shaderParams.GetParamKey<Vector2>("point");
			frequencyKey = shaderParams.GetParamKey<Vector2>("frequency");
			amplitudeKey = shaderParams.GetParamKey<Vector2>("amplitude");
			uv0Key = shaderParams.GetParamKey<Vector2>("uv0");
			uv1Key = shaderParams.GetParamKey<Vector2>("uv1");
		}

		public void Apply(int pass)
		{
			if (AutoLoopEnabled) {
				shaderParams.Set(timeKey, Time + (float)(frameCount * AnimationUtils.SecondsPerFrame));
				frameCount++;
			} else {
				shaderParams.Set(timeKey, Time);
			}
			shaderParams.Set(phaseKey, Phase);
			shaderParams.Set(pointKey, Point);
			shaderParams.Set(frequencyKey, Frequency);
			shaderParams.Set(amplitudeKey, Amplitude);
			shaderParams.Set(uv0Key, UV0);
			shaderParams.Set(uv1Key, UV1);
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(WaveShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new WaveMaterial() {
			Time = Time,
			Phase = Phase,
			Point = Point,
			Frequency = Frequency,
			Amplitude = Amplitude,
			AutoLoopEnabled = AutoLoopEnabled,
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
				uniform lowp float time;
				uniform lowp vec2 phase;
				uniform lowp vec2 point;
				uniform lowp vec2 frequency;
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
					coef.x = sin(phase.x * coef.x + frequency.x * time) * amplitude.x * SoftClamp(localUV.x);
					coef.y = sin(phase.y * coef.y + frequency.y * time) * amplitude.y * SoftClamp(localUV.y);
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

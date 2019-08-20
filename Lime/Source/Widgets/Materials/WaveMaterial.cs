using System;

namespace Lime
{
	public class WaveMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> timeKey;
		private readonly ShaderParamKey<float> frequencyKey;
		private readonly ShaderParamKey<Vector2> pointKey;
		private readonly ShaderParamKey<Vector2> timeSpeedKey;
		private readonly ShaderParamKey<Vector2> amplitudeKey;
		private readonly ShaderParamKey<Vector2> uv0Key;
		private readonly ShaderParamKey<Vector2> uv1Key;

		private long frameCount = 0;

		public bool AutoLoopEnabled = false;
		public float Time = 0;
		public float Frequency = 30;
		public Vector2 Point = new Vector2(0.5f, 0.5f);
		public Vector2 TimeSpeed = new Vector2(4, 0);
		public Vector2 Amplitude = new Vector2(0.05f, 0.05f);
		public Vector2 UV0;
		public Vector2 UV1;

		public Func<Blending> BlendingGetter;

		public string Id { get; set; }
		public int PassCount => 1;

		public WaveMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			timeKey = shaderParams.GetParamKey<float>("time");
			frequencyKey = shaderParams.GetParamKey<float>("frequency");
			pointKey = shaderParams.GetParamKey<Vector2>("point");
			timeSpeedKey = shaderParams.GetParamKey<Vector2>("timeSpeed");
			amplitudeKey = shaderParams.GetParamKey<Vector2>("amplitude");
			uv0Key = shaderParams.GetParamKey<Vector2>("UV0");
			uv1Key = shaderParams.GetParamKey<Vector2>("UV1");
		}

		public void Apply(int pass)
		{
			if (AutoLoopEnabled) {
				shaderParams.Set(timeKey, Time + (float)(frameCount * AnimationUtils.SecondsPerFrame));
				frameCount++;
			} else {
				shaderParams.Set(timeKey, Time);
			}
			shaderParams.Set(frequencyKey, Frequency);
			shaderParams.Set(pointKey, Point);
			shaderParams.Set(timeSpeedKey, TimeSpeed);
			shaderParams.Set(amplitudeKey, Amplitude);
			shaderParams.Set(uv0Key, UV0);
			shaderParams.Set(uv1Key, UV1);
			PlatformRenderer.SetBlendState(BlendingGetter.Invoke().GetBlendState());
			PlatformRenderer.SetShaderProgram(WaveShaderProgram.Instance);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new WaveMaterial() {
			Time = Time,
			Frequency = Frequency,
			Point = Point,
			TimeSpeed = TimeSpeed,
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
				uniform lowp float frequency;
				uniform lowp vec2 point;
				uniform lowp vec2 timeSpeed;
				uniform lowp vec2 amplitude;
				uniform lowp vec2 UV0;
				uniform lowp vec2 UV1;

				varying lowp vec2 uv;
				varying lowp vec4 color;

				lowp float SoftClamp(lowp float f)
				{
					lowp float x = abs(f * 2.0 - 1.0);
					return 1.0 - x * x * x;
				}

				void main()
				{
					lowp vec2 localUV = (uv - UV0) / (UV1 - UV0);
					lowp vec2 coef = vec2(1.0 - distance(point, localUV), 0.0);
					coef.y = coef.x;
					coef.x = sin(frequency * coef.x + timeSpeed.x * time) * amplitude.x * SoftClamp(localUV.x);
					coef.y = sin(frequency * coef.y + timeSpeed.y * time) * amplitude.y * SoftClamp(localUV.y);
					lowp vec2 dir = coef * normalize(point - localUV);
					gl_FragColor = texture2D(tex1, UV0 + (localUV + dir) * (UV1 - UV0)) * color;
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

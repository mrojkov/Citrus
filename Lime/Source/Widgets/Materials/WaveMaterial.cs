using System;
using System.Text;
using System.Collections.Generic;

namespace Lime
{
	public class WaveMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector2> phaseKey;
		private readonly ShaderParamKey<Vector2> frequencyKey;
		private readonly ShaderParamKey<Vector2> pivotKey;
		private readonly ShaderParamKey<Vector2> amplitudeKey;
		private readonly ShaderParamKey<Vector2> uv0Key;
		private readonly ShaderParamKey<Vector2> uv1Key;

		public bool IsClamped { get; set; } = true;
		public Blending Blending { get; set; } = Blending.Alpha;
		public WaveType Type { get; set; } = WaveType.Sinusoidal;
		public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);
		public Vector2 Phase { get; set; }
		public Vector2 Frequency { get; set; }
		public Vector2 Amplitude { get; set; } = new Vector2(0.05f, 0.05f);
		public Vector2 UV0 { get; set; }
		public Vector2 UV1 { get; set; }

		public string Id { get; set; }
		public int PassCount => 1;

		public WaveMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			phaseKey = shaderParams.GetParamKey<Vector2>("phase");
			frequencyKey = shaderParams.GetParamKey<Vector2>("frequency");
			pivotKey = shaderParams.GetParamKey<Vector2>("pivot");
			amplitudeKey = shaderParams.GetParamKey<Vector2>("amplitude");
			uv0Key = shaderParams.GetParamKey<Vector2>("UV0");
			uv1Key = shaderParams.GetParamKey<Vector2>("UV1");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(phaseKey, Phase * Mathf.TwoPi);
			shaderParams.Set(frequencyKey, Frequency);
			shaderParams.Set(pivotKey, Pivot);
			shaderParams.Set(amplitudeKey, Amplitude);
			shaderParams.Set(uv0Key, UV0);
			shaderParams.Set(uv1Key, UV1);
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(WaveShaderProgram.GetInstance(Type, IsClamped));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new WaveMaterial {
			Blending = Blending,
			Phase = Phase,
			Frequency = Frequency,
			Pivot = Pivot,
			Amplitude = Amplitude,
			IsClamped = IsClamped,
			Type = Type
		};
	}

	public enum WaveType
	{
		Sinusoidal,
		CircleWave
	}
	
	public class WaveShaderProgram : ShaderProgram
	{
		private static readonly Dictionary<int, WaveShaderProgram> instances =
			new Dictionary<int, WaveShaderProgram>(Enum.GetValues(typeof(WaveType)).Length);

		private static int GetInstanceKey(WaveType type, bool isClamped) => (int)type | ((isClamped ? 1 : 0) << 8);

		public static WaveShaderProgram GetInstance(WaveType type = WaveType.Sinusoidal, bool isClamped = false)
		{
			var key = GetInstanceKey(type, isClamped);
			return instances.TryGetValue(key, out var shaderProgram)
				? shaderProgram
				: (instances[key] = new WaveShaderProgram(type, isClamped));
		}

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

		private const string FragmentShaderHeader = @"
			uniform lowp sampler2D tex1;
			uniform lowp vec2 phase;
			uniform lowp vec2 frequency;
			uniform lowp vec2 pivot;
			uniform lowp vec2 amplitude;
			uniform lowp vec2 UV0;
			uniform lowp vec2 UV1;

			varying lowp vec2 uv;
			varying lowp vec4 color;

			void main()
			{
				lowp vec2 localUV = (uv - UV0) / (UV1 - UV0);
			";

		private const string FragmentSinWave = @"
				lowp vec2 coef = vec2(1.0 - distance(pivot, localUV));
				coef = sin(frequency * coef + phase) * amplitude;
				lowp vec2 dir = coef * normalize(pivot - localUV);
			";

		private const string FragmentCircleWave = @"
				lowp vec2 corner = pivot * 2.0;
				lowp vec2 origin = corner * (floor(localUV / corner) + 0.5);
				lowp vec2 coef = dot(localUV, frequency) + phase;
				lowp vec2 dir = amplitude * vec2(cos(coef.x), sin(coef.y)) * distance(localUV, origin);
			";

		private const string FragmentClamping = @"
				dir *= 1.0 - pow(abs(localUV * 2.0 - 1.0), vec2(3.0));
			";

		private const string FragmentShaderFooter = @"
				gl_FragColor = texture2D(tex1, UV0 + clamp(localUV + dir, 0.0, 1.0) * (UV1 - UV0)) * color;
			}";

		private WaveShaderProgram(WaveType type, bool clamping)
			: base(CreateShaders(type, clamping), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(WaveType type, bool clamping)
		{
			var fragmentShader = new StringBuilder(FragmentShaderHeader,
				FragmentShaderHeader.Length + FragmentSinWave.Length + FragmentShaderFooter.Length);

			switch (type) {
				case WaveType.Sinusoidal:
					fragmentShader.Append(FragmentSinWave);
					break;
				case WaveType.CircleWave:
					fragmentShader.Append(FragmentCircleWave);
					break;
			}

			if (clamping) {
				fragmentShader.Append(FragmentClamping);
			}

			fragmentShader.Append(FragmentShaderFooter);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

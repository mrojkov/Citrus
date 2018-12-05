using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class FXAAMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector2> texelStepKey;
		private readonly ShaderParamKey<Vector4> amountKey;

		public Vector2 TexelStep { get; set; }
		public float LumaTreshold { get; set; }
		public float MulReduce { get; set; }
		public float MinReduce { get; set; }
		public float MaxSpan { get; set; }
		public bool Opaque { get; set; }

		public string Id { get; set; }
		public int PassCount => 1;

		public FXAAMaterial() : this(Blending.Alpha) { }

		public FXAAMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			texelStepKey = shaderParams.GetParamKey<Vector2>("uTexelStep");
			amountKey = shaderParams.GetParamKey<Vector4>("uAmount");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(texelStepKey, TexelStep);
			shaderParams.Set(amountKey, new Vector4(LumaTreshold, 0.25f / MulReduce, 1f / MinReduce, MaxSpan));
			PlatformRenderer.SetBlendState(!Opaque ? blending.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(FXAAShaderProgram.GetInstance(Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new FXAAMaterial(blending) {
				Opaque = Opaque
			};
		}
	}

	public class FXAAShaderProgram : ShaderProgram
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

		private const string FragmentShaderPart1 = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			uniform lowp sampler2D tex1;
			uniform lowp vec2 uTexelStep;
			uniform lowp vec4 uAmount;

			void main() {
				lowp vec4 c = texture2D(tex1, texCoords1);
				lowp vec3 rgbNW = texture2D(tex1, texCoords1 + vec2(-uTexelStep.x, uTexelStep.y)).rgb;
				lowp vec3 rgbNE = texture2D(tex1, texCoords1 + vec2(uTexelStep.x, uTexelStep.y)).rgb;
				lowp vec3 rgbSW = texture2D(tex1, texCoords1 + vec2(-uTexelStep.x, -uTexelStep.y)).rgb;
				lowp vec3 rgbSE = texture2D(tex1, texCoords1 + vec2(uTexelStep.x, -uTexelStep.y)).rgb;

				lowp vec3 toLuma = vec3(0.299, 0.587, 0.114);
				lowp float lumaNW = dot(rgbNW, toLuma);
				lowp float lumaNE = dot(rgbNE, toLuma);
				lowp float lumaSW = dot(rgbSW, toLuma);
				lowp float lumaSE = dot(rgbSE, toLuma);
				lowp float lumaM = dot(c.rgb, toLuma);

				lowp float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
				lowp float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
				if (lumaMax - lumaMin <= lumaMax * uAmount.x) {";
		private const string FragmentShaderPart2 = @"
					gl_FragColor = color * c;";
		private const string FragmentShaderPart2Opaque = @"
					gl_FragColor = vec4(color.rgb * c.rgb, 1.0);";
		private const string FragmentShaderPart3 = @"
					return;
				}

				lowp vec2 samplingDirection = vec2(-lumaNW - lumaNE + lumaSW + lumaSE, lumaNW + lumaSW - lumaNE - lumaSE);
				lowp float samplingDirectionReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * uAmount.y, uAmount.z);
				lowp float minSamplingDirectionFactor = 1.0 / (min(abs(samplingDirection.x), abs(samplingDirection.y)) + samplingDirectionReduce);
				samplingDirection = clamp(samplingDirection * minSamplingDirectionFactor, vec2(-uAmount.w), vec2(uAmount.w)) * uTexelStep;

				lowp vec3 rgbSampleNeg = texture2D(tex1, texCoords1 + samplingDirection * -0.166666667).rgb;
				lowp vec3 rgbSamplePos = texture2D(tex1, texCoords1 + samplingDirection * 0.166666667).rgb;

				lowp vec3 rgbTwoTab = (rgbSamplePos + rgbSampleNeg) * 0.5;

				lowp vec3 rgbSampleNegOuter = texture2D(tex1, texCoords1 + samplingDirection * -0.5).rgb;
				lowp vec3 rgbSamplePosOuter = texture2D(tex1, texCoords1 + samplingDirection * 0.5).rgb;

				lowp vec3 rgbFourTab = (rgbSamplePosOuter + rgbSampleNegOuter) * 0.25 + rgbTwoTab * 0.5;

				lowp float lumaFourTab = dot(rgbFourTab, toLuma);";
		private const string FragmentShaderPart4 = @"
				if (lumaFourTab < lumaMin || lumaFourTab > lumaMax) {
					gl_FragColor = color * vec4(rgbTwoTab, c.a);
				} else {
					gl_FragColor = color * vec4(rgbFourTab, c.a);
				}
			}";
		private const string FragmentShaderPart4Opaque = @"
				if (lumaFourTab < lumaMin || lumaFourTab > lumaMax) {
					gl_FragColor = vec4(color.rgb * rgbTwoTab, 1.0);
				} else {
					gl_FragColor = vec4(color.rgb * rgbFourTab, 1.0);
				}
			}";

		private static readonly Dictionary<int, FXAAShaderProgram> instances = new Dictionary<int, FXAAShaderProgram>();

		private static int GetInstanceKey(bool opaque) => opaque ? 1 : 0;

		public static FXAAShaderProgram GetInstance(bool opaque = false)
		{
			var key = GetInstanceKey(false);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new FXAAShaderProgram(opaque));
		}

		private FXAAShaderProgram(bool opaque) : base(CreateShaders(opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool opaque)
		{
			var length =
				FragmentShaderPart1.Length +
				(!opaque ? FragmentShaderPart2.Length : FragmentShaderPart2Opaque.Length) +
				FragmentShaderPart3.Length +
				(!opaque ? FragmentShaderPart4.Length : FragmentShaderPart4Opaque.Length);
			var fragmentShader = new StringBuilder(length);
			fragmentShader.Append(FragmentShaderPart1);
			fragmentShader.Append(!opaque ? FragmentShaderPart2 : FragmentShaderPart2Opaque);
			fragmentShader.Append(FragmentShaderPart3);
			fragmentShader.Append(!opaque ? FragmentShaderPart4 : FragmentShaderPart4Opaque);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

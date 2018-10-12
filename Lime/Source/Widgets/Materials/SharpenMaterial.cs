using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class SharpenMaterial : IMaterial
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
		public bool Opaque { get; set; }

		public int PassCount => 1;

		public SharpenMaterial() : this(Blending.Alpha) { }

		public SharpenMaterial(Blending blending)
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
			PlatformRenderer.SetBlendState(!Opaque ? blending.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(SharpenShaderProgram.GetInstance(Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SharpenMaterial(blending) {
				Strength = Strength,
				Limit = Limit,
				Step = Step,
				Opaque = Opaque
			};
		}
	}

	public class SharpenShaderProgram : ShaderProgram
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
			uniform lowp vec2 step;
			uniform lowp vec3 uSharpness;

			void main() {
				lowp vec4 c = texture2D(tex1, texCoords1);
				lowp vec3 sum = vec3(0.0);
				sum	+= texture2D(tex1, texCoords1 + vec2(0.0, -step.y)).rgb;
				sum	+= texture2D(tex1, texCoords1 + vec2(0.0, step.y)).rgb;
				sum	+= texture2D(tex1, texCoords1 + vec2(-step.x, 0.0)).rgb;
				sum	+= texture2D(tex1, texCoords1 + vec2(step.x, 0.0)).rgb;

				lowp vec3 delta = uSharpness.x * c.rgb - uSharpness.y * sum;
				c.rgb += clamp(delta, -uSharpness.z, uSharpness.z);
			";
		private const string FragmentShaderPart2 = @"
				gl_FragColor = color * c;
			}";
		private const string FragmentShaderPart2Opaque = @"
				gl_FragColor = vec4(color.rgb * c.rgb, 1.0);
			}";

		private static readonly Dictionary<int, SharpenShaderProgram> instances = new Dictionary<int, SharpenShaderProgram>();

		private static int GetInstanceKey(bool opaque) => opaque ? 1 : 0;

		public static SharpenShaderProgram GetInstance(bool opaque = false)
		{
			var key = GetInstanceKey(false);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new SharpenShaderProgram(opaque));
		}

		private SharpenShaderProgram(bool opaque) : base(CreateShaders(opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool opaque)
		{
			var length = FragmentShaderPart1.Length + (!opaque ? FragmentShaderPart2.Length : FragmentShaderPart2Opaque.Length);
			var fragmentShader = new StringBuilder(length);
			fragmentShader.Append(FragmentShaderPart1);
			fragmentShader.Append(!opaque ? FragmentShaderPart2 : FragmentShaderPart2Opaque);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

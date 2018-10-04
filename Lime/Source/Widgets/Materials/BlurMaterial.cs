using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lime
{
	public class BlurMaterial : IMaterial
	{
		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector2> stepKey;
		private readonly ShaderParamKey<float> alphaCorrectionKey;

		public float Radius { get; set; } = 1f;
		public BlurShaderId BlurShaderId { get; set; } = BlurShaderId.GaussOneDimensionalWith5Samples;
		public Vector2 Step { get; set; } = Vector2.One * (1f / 128f);
		public Vector2 Dir { get; set; } = new Vector2(0, 1);
		public float AlphaCorrection { get; set; } = 1f;

		public int PassCount => 1;

		public BlurMaterial() : this(Blending.Alpha) { }

		public BlurMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			stepKey = shaderParams.GetParamKey<Vector2>("step");
			alphaCorrectionKey = shaderParams.GetParamKey<float>("inversedAlphaCorrection");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(stepKey, Dir * Radius * Step);
			shaderParams.Set(alphaCorrectionKey, 1f / AlphaCorrection);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(BlurShaderProgram.GetInstance(BlurShaderId));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new BlurMaterial(blending) {
				Radius = Radius,
				Step = Step,
				Dir = Dir
			};
		}
	}

	public enum BlurShaderId
	{
		GaussOneDimensionalWith3Samples,
		GaussOneDimensionalWith5Samples,
		GaussOneDimensionalWith7Samples,
		GaussOneDimensionalWith9Samples,
		GaussOneDimensionalWith11Samples,
		GaussOneDimensionalWith13Samples,
		GaussOneDimensionalWith15Samples
	}

	public class BlurShaderProgram : ShaderProgram
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
			}
			";

		private const string FragmentShaderPart1 = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			uniform lowp sampler2D tex1;
			uniform lowp vec2 step;
			uniform lowp float inversedAlphaCorrection;

			void main() {
				lowp vec4 sum = vec4(0.0);
			";

		private const string FragmentShaderSummNeighborFormat = "sum += texture2D(tex1, texCoords1 {1}step) * {0};\n";
		private const string FragmentShaderSummSelfFormat = "sum += texture2D(tex1, texCoords1) * {0};\n";

		private const string FragmentShaderPart2 = @"
				sum.w = pow(sum.w, inversedAlphaCorrection);
				gl_FragColor = color * sum;
			}
			";

		private static readonly Dictionary<BlurShaderId, float[]> gaussKernelsWeights = new Dictionary<BlurShaderId, float[]> {
			{
				BlurShaderId.GaussOneDimensionalWith3Samples,
				new[] { 0.315423f, 0.369153f, 0.315423f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith5Samples,
				new[] { 0.141196f, 0.22635f, 0.264907f, 0.22635f, 0.141196f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith7Samples,
				new[] { 0.056968f, 0.125109f, 0.200561f, 0.234725f, 0.200561f, 0.125109f, 0.056968f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith9Samples,
				new[] { 0.01824f, 0.054889f, 0.120545f, 0.193244f, 0.226162f, 0.193244f, 0.120545f, 0.054889f, 0.01824f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith11Samples,
				new[] { 0.004384f, 0.018081f, 0.054408f, 0.119488f, 0.19155f, 0.224179f, 0.19155f, 0.119488f, 0.054408f, 0.018081f, 0.004384f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith13Samples,
				new[] { 0.000774f, 0.004377f, 0.018053f, 0.054324f, 0.119303f, 0.191254f, 0.223832f, 0.191254f, 0.119303f, 0.054324f, 0.018053f, 0.004377f, 0.000774f }
			},
			{
				BlurShaderId.GaussOneDimensionalWith15Samples,
				new[] { 0.0001f, 0.000774f, 0.004376f, 0.018049f, 0.054313f, 0.119279f, 0.191215f, 0.223788f, 0.191215f, 0.119279f, 0.054313f, 0.018049f, 0.004376f, 0.000774f, 0.0001f }
			}
		};
		private static readonly Dictionary<BlurShaderId, BlurShaderProgram> instances = new Dictionary<BlurShaderId, BlurShaderProgram>(gaussKernelsWeights.Count);

		public static BlurShaderProgram GetInstance(BlurShaderId blurShaderId = BlurShaderId.GaussOneDimensionalWith5Samples) =>
			instances.TryGetValue(blurShaderId, out var shaderProgram) ? shaderProgram : (instances[blurShaderId] = new BlurShaderProgram(blurShaderId));

		private BlurShaderProgram(BlurShaderId blurShaderId) : base(CreateShaders(blurShaderId), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(BlurShaderId blurShaderId)
		{
			var kernelWeights = gaussKernelsWeights[blurShaderId];
			var centerKernel = kernelWeights.Length / 2;
			var fragmentShader = new StringBuilder(FragmentShaderPart1.Length + FragmentShaderPart2.Length);
			fragmentShader.Append(FragmentShaderPart1);
			for (var i = 0; i < kernelWeights.Length; i++) {
				var weight = kernelWeights[i].ToString("F6", CultureInfo.InvariantCulture);
				if (i == centerKernel) {
					fragmentShader.Append(string.Format(FragmentShaderSummSelfFormat, weight));
				} else {
					var offset = (i - centerKernel) / (float)centerKernel;
					var sign = offset < 0 ? "- " : "+ ";
					var absOffset = Mathf.Abs(offset);
					var offsetStr = Mathf.Abs(absOffset - 1f) > Mathf.ZeroTolerance ? $"{sign}{absOffset.ToString("F6", CultureInfo.InvariantCulture)}*" : sign;
					fragmentShader.Append(string.Format(FragmentShaderSummNeighborFormat, weight, offsetStr));
				}
			}
			fragmentShader.Append(FragmentShaderPart2);

			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

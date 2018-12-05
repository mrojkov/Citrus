using System;
using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class NoiseMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> brightThresholdKey;
		private readonly ShaderParamKey<float> darkThresholdKey;
		private readonly ShaderParamKey<float> softLightKey;

		public float BrightThreshold { get; set; } = 1f;
		public float DarkThreshold { get; set; }
		public float SoftLight { get; set; } = 1f;
		public bool Opaque { get; set; }

		public string Id { get; set; }
		public int PassCount => 1;

		public NoiseMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			brightThresholdKey = shaderParams.GetParamKey<float>("brightThreshold");
			darkThresholdKey = shaderParams.GetParamKey<float>("darkThreshold");
			softLightKey = shaderParams.GetParamKey<float>("softLight");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(brightThresholdKey, BrightThreshold);
			shaderParams.Set(darkThresholdKey, DarkThreshold);
			shaderParams.Set(softLightKey, SoftLight);
			PlatformRenderer.SetBlendState(!Opaque ? Blending.Alpha.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(NoiseShaderProgram.GetInstance(Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new NoiseMaterial {
			BrightThreshold = BrightThreshold,
			DarkThreshold = DarkThreshold,
			SoftLight = SoftLight
		};
	}

	public class NoiseShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform mat4 matProjection;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
				texCoords2 = inTexCoords2;
			}";

		private const string FragmentShaderPart1 = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;

			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			uniform lowp float brightThreshold;
			uniform lowp float darkThreshold;
			uniform lowp float softLight;

			lowp float blendSoftLight(lowp float base, lowp float blend) {
				return (blend<0.5) ? (2.0*base*blend+base*base*(1.0-2.0*blend)) : (sqrt(base)*(2.0*blend-1.0)+2.0*base*(1.0-blend));
			}

			lowp vec3 blendSoftLight(lowp vec3 base, lowp vec3 blend, lowp float factor) {
				return vec3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b)) * factor + base * (1.0 - factor);
			}

			void main() {
				lowp vec4 srcColor = texture2D(tex1, texCoords1);
				lowp vec4 noiseColor = texture2D(tex2, texCoords2);

				lowp vec3 luminanceVector = vec3(0.2125, 0.7154, 0.0721);
				lowp float luminance = dot(luminanceVector, noiseColor.rgb);
				lowp float brightCheck = sign(max(0.0, brightThreshold - luminance));
				lowp float darkCheck = sign(max(0.0, luminance - darkThreshold));

				lowp vec3 c = blendSoftLight(srcColor.rgb * color.rgb, noiseColor.rgb, softLight * brightCheck * darkCheck);";
		private const string FragmentShaderPart2 = @"
				gl_FragColor = vec4(c.rgb, srcColor.a * color.a);
			}";
		private const string FragmentShaderPart2Opaque = @"
				gl_FragColor = vec4(c.rgb, 1.0);
			}";

		private static readonly Dictionary<int, NoiseShaderProgram> instances = new Dictionary<int, NoiseShaderProgram>();

		private static int GetInstanceKey(bool opaque) => opaque ? 1 : 0;

		public static NoiseShaderProgram GetInstance(bool opaque = false)
		{
			var key = GetInstanceKey(false);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new NoiseShaderProgram(opaque));
		}

		private NoiseShaderProgram(bool opaque) : base(CreateShaders(opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool opaque)
		{
			var fragmentShader = new StringBuilder(FragmentShaderPart1.Length + Math.Max(FragmentShaderPart2.Length, FragmentShaderPart2Opaque.Length));
			fragmentShader.Append(FragmentShaderPart1);
			fragmentShader.Append(!opaque ? FragmentShaderPart2 : FragmentShaderPart2Opaque);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

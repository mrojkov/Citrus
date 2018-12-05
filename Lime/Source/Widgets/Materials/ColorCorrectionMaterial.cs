using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class ColorCorrectionMaterial : IMaterial
	{
		private static readonly Vector3 defaultHSL = new Vector3(0, 1, 1);
		private const float DefaultBrightness = 0f;
		private const float DefaultContrast = 1f;
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector3> hslKey;
		private readonly ShaderParamKey<float> brightnessKey;
		private readonly ShaderParamKey<float> contrastKey;

		public Vector3 HSL = defaultHSL;
		public float Brightness { get; set; } = DefaultBrightness;
		public float Contrast { get; set; } = DefaultContrast;
		public bool Opaque { get; set; }

		private bool RequiredBrightnessContrastProcess =>
			Mathf.Abs(Brightness - DefaultBrightness) >= Mathf.ZeroTolerance || Mathf.Abs(Contrast - DefaultContrast) >= Mathf.ZeroTolerance;
		private bool RequiredHSLProcess =>
			Mathf.Abs(HSL.X - defaultHSL.X) >= Mathf.ZeroTolerance || Mathf.Abs(HSL.Y - defaultHSL.Y) >= Mathf.ZeroTolerance || Mathf.Abs(HSL.Z - defaultHSL.Z) >= Mathf.ZeroTolerance;

		public string Id { get; set; }
		public int PassCount => 1;

		public ColorCorrectionMaterial() : this(Blending.Alpha) { }

		public ColorCorrectionMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			hslKey = shaderParams.GetParamKey<Vector3>("inHsl");
			brightnessKey = shaderParams.GetParamKey<float>("brightness");
			contrastKey = shaderParams.GetParamKey<float>("contrast");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(hslKey, HSL);
			shaderParams.Set(brightnessKey, Brightness);
			shaderParams.Set(contrastKey, Contrast);
			PlatformRenderer.SetBlendState(!Opaque ? blending.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(ColorCorrectionShaderProgram.GetInstance(RequiredBrightnessContrastProcess, RequiredHSLProcess, Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone() => new ColorCorrectionMaterial(blending) {
			HSL = HSL,
			Brightness = Brightness,
			Contrast = Contrast,
			Opaque = Opaque
		};
	}

	public class ColorCorrectionShaderProgram : ShaderProgram
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

		private const string FragmentShaderUniformBrightnessContrast = @"
			uniform lowp float brightness;
			uniform lowp float contrast;";
		private const string FragmentShaderUniformHSL = @"
			uniform lowp vec3 inHsl;";
		private const string FragmentShaderUniform = @"
			uniform lowp sampler2D tex1;

			varying lowp vec2 texCoords1;";
		private const string FragmentShaderHSLMethods = @"
			lowp float HueToRgb(lowp float f1, lowp float f2, lowp float hue) {
				if (hue < 0.0)
					hue += 1.0;
				else if (hue > 1.0)
					hue -= 1.0;
				lowp float res;
				if ((6.0 * hue) < 1.0)
					res = f1 + (f2 - f1) * 6.0 * hue;
				else if ((2.0 * hue) < 1.0)
					res = f2;
				else if ((3.0 * hue) < 2.0)
					res = f1 + (f2 - f1) * (0.66666666 - hue) * 6.0;
				else
					res = f1;
				return res;
			}

			lowp vec3 HslToRgb(lowp vec3 hsl) {
				lowp vec3 rgb;
				if (hsl.y == 0.0) {
					rgb = vec3(hsl.z);
				} else {
					lowp float f2;
					if (hsl.z < 0.5)
						f2 = hsl.z * (1.0 + hsl.y);
					else
						f2 = hsl.z + hsl.y - hsl.y * hsl.z;
					lowp float f1 = 2.0 * hsl.z - f2;
					rgb.r = HueToRgb(f1, f2, hsl.x + 0.33333333);
					rgb.g = HueToRgb(f1, f2, hsl.x);
					rgb.b = HueToRgb(f1, f2, hsl.x - 0.33333333);
				}
				return rgb;
			}

			lowp vec3 RgbToHsl(lowp vec3 rgb)
			{
				lowp vec4 p = (rgb.g < rgb.b) ? vec4(rgb.bg, -1.0, 0.66666666) : vec4(rgb.gb, 0.0, -0.33333333);
				lowp vec4 q = (rgb.r < p.x) ? vec4(p.xyw, rgb.r) : vec4(rgb.r, p.yzx);
				lowp float c = q.x - min(q.w, q.y);
				lowp float h = abs((q.w - q.y) / (6.0 * c + 1e-10) + q.z);
				lowp float l = q.x - c * 0.5;
				lowp float s = c / (1.0 - abs(l * 2.0 - 1.0) + 1e-10);
				return vec3(h, s, l);
			}";
		private const string FragmentShaderPart1 = @"
			void main()
			{
				lowp vec4 color = texture2D(tex1, texCoords1);";
		private const string FragmentShaderBrightnessContrast = @"
				color.rgb = (color.rgb - 0.5) * contrast + 0.5 + brightness;";
		private const string FragmentShaderHSL = @"
				lowp vec3 sourceRgb = min(color.rgb, 0.98823529); // Hack: solve problem with black artifacts
				lowp vec3 hsl = RgbToHsl(sourceRgb);
				hsl.x += inHsl.x;
				hsl.yz *= inHsl.yz;
				color.rgb = HslToRgb(hsl);";
		private const string FragmentShaderPart2 = @"
				gl_FragColor = color;
			}";
		private const string FragmentShaderPart2Opaque = @"
				gl_FragColor = vec4(color.rgb, 1.0);
			}";

		private static readonly Dictionary<int, ColorCorrectionShaderProgram> instances = new Dictionary<int, ColorCorrectionShaderProgram>();

		private static int GetInstanceKey(bool requiredBrightnessContrast, bool requiredHSL, bool opaque) =>
			(requiredBrightnessContrast ? 1 : 0) | ((requiredHSL ? 1 : 0) << 1) | ((opaque ? 1 : 0) << 2);

		public static ColorCorrectionShaderProgram GetInstance(bool requiredBrightnessContrast = false, bool requiredHSL = false, bool opaque = false)
		{
			var key = GetInstanceKey(requiredBrightnessContrast, requiredHSL, opaque);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new ColorCorrectionShaderProgram(requiredBrightnessContrast, requiredHSL, opaque));
		}

		private ColorCorrectionShaderProgram(bool requiredBrightnessContrast, bool requiredHSL, bool opaque) :
			base(CreateShaders(requiredBrightnessContrast, requiredHSL, opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool requiredBrightnessContrast, bool requiredHSL, bool opaque)
		{
			var length =
				(requiredBrightnessContrast ? FragmentShaderUniformBrightnessContrast.Length + FragmentShaderBrightnessContrast.Length : 0) +
				(requiredHSL ? FragmentShaderUniformHSL.Length + FragmentShaderHSLMethods.Length + FragmentShaderHSL.Length : 0) +
				FragmentShaderUniform.Length +
				FragmentShaderPart1.Length +
				(!opaque ? FragmentShaderPart2.Length : FragmentShaderPart2Opaque.Length);
			var fragmentShader = new StringBuilder(length);
			if (requiredBrightnessContrast) {
				fragmentShader.Append(FragmentShaderUniformBrightnessContrast);
			}
			if (requiredHSL) {
				fragmentShader.Append(FragmentShaderUniformHSL);
			}
			fragmentShader.Append(FragmentShaderUniform);
			if (requiredHSL) {
				fragmentShader.Append(FragmentShaderHSLMethods);
			}
			fragmentShader.Append(FragmentShaderPart1);
			if (requiredBrightnessContrast) {
				fragmentShader.Append(FragmentShaderBrightnessContrast);
			}
			if (requiredHSL) {
				fragmentShader.Append(FragmentShaderHSL);
			}
			fragmentShader.Append(!opaque ? FragmentShaderPart2 : FragmentShaderPart2Opaque);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

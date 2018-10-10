using System;
using System.Collections.Generic;
using System.Text;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class HSLComponent : MaterialComponent<HSLMaterial>
	{
		[YuzuMember]
		public float Hue
		{
			get => CustomMaterial.HSL.X * 360.0f;
			set => CustomMaterial.HSL.X = value / 360.0f;
		}

		[YuzuMember]
		public float Saturation
		{
			get => CustomMaterial.HSL.Y * 100.0f;
			set => CustomMaterial.HSL.Y = value / 100.0f;
		}

		[YuzuMember]
		public float Lightness
		{
			get => CustomMaterial.HSL.Z * 100.0f;
			set => CustomMaterial.HSL.Z = value / 100.0f;
		}
	}

	public class HSLMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector3> hslKey;

		public Vector3 HSL = new Vector3(0, 1, 1);
		public bool Opaque { get; set; }

		public int PassCount => 1;

		public HSLMaterial() : this(Blending.Alpha) { }

		public HSLMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			hslKey = shaderParams.GetParamKey<Vector3>("u_hsl");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(hslKey, HSL);
			PlatformRenderer.SetBlendState(!Opaque ? blending.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(HSLShaderProgram.GetInstance(Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new HSLMaterial(blending) {
				HSL = HSL,
				Opaque = Opaque
			};
		}
	}

	public class HSLShaderProgram : ShaderProgram
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
			}
			";

		private const string FragmentShaderPart1 = @"
			uniform lowp sampler2D tex1;
			uniform lowp vec3 u_hsl;

			varying lowp vec2 texCoords1;

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
			}

			void main()
			{
			    lowp vec4 color = texture2D(tex1, texCoords1);
				lowp vec3 sourceRgb = min(color.rgb, 0.98823529); // Hack: solve problem with black artifacts
			    lowp vec3 hsl = RgbToHsl(sourceRgb);
			    hsl.x += u_hsl.x;
			    hsl.yz *= u_hsl.yz;
			    lowp vec3 rgb = HslToRgb(hsl);";
		private const string FragmentShaderPart2 = @"
				gl_FragColor = vec4(rgb, color.a);
			}";
		private const string FragmentShaderPart2Opaque = @"
				gl_FragColor = vec4(rgb, 1.0);
			}";

		private static readonly Dictionary<int, HSLShaderProgram> instances = new Dictionary<int, HSLShaderProgram>();

		private static int GetInstanceKey(bool opaque) => opaque ? 1 : 0;

		public static HSLShaderProgram GetInstance(bool opaque = false)
		{
			var key = GetInstanceKey(false);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new HSLShaderProgram(opaque));
		}

		private HSLShaderProgram(bool opaque) : base(CreateShaders(opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

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

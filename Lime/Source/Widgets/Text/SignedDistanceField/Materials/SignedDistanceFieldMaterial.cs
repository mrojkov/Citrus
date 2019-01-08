using System;
using System.Collections.Generic;
using System.Text;

namespace Lime.SignedDistanceField
{
	public class SignedDistanceFieldMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<float> dilateKey;
		private readonly ShaderParamKey<float> thicknessKey;
		private readonly ShaderParamKey<Vector4> outlineColorKey;
		private readonly ShaderParamKey<float> gradientAngleKey;
		private readonly ShaderParamKey<Vector4> lightColorKey;
		private readonly ShaderParamKey<Vector2> lightAngleKey;
		private readonly ShaderParamKey<float> bevelRoundnessKey;
		private readonly ShaderParamKey<float> bevelWidthKey;
		private readonly ShaderParamKey<float> reflectionKey;

		private const int gradientTexturePixelCount = 2048;
		private Color4[] pixels;
		private int currentVersion;
		private ColorGradient gradient;

		public float Softness { get; set; } = 0f;
		public float Dilate { get; set; } = 0f;
		public float Thickness { get; set; } = 0f;
		public Color4 OutlineColor { get; set; } = Color4.Black;
		public bool GradientEnabled { get; set; }
		public ColorGradient Gradient
		{
			get => gradient;
			set {
				if (gradient != value) {
					gradient = value;
					InvalidateTextureIfNecessary(forceInvalidate: true);
				}
			}
		}
		public float GradientAngle { get; set; } = 0f;
		public Texture2D GradientTexture { get; private set; }
		public bool BevelEnabled { get; set; }
		public float LightAngle { get; set; }
		public Color4 LightColor { get; set; }
		public float BevelWidth { get; set; }
		public float ReflectionPower { get; set; }
		public float BevelRoundness { get; set; }

		public string Id { get; set; }

		public int PassCount => 1;

		public SignedDistanceFieldMaterial() : this(Blending.Alpha) { }

		public SignedDistanceFieldMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			softnessKey = shaderParams.GetParamKey<float>("softness");
			dilateKey = shaderParams.GetParamKey<float>("dilate");
			thicknessKey = shaderParams.GetParamKey<float>("thickness");
			outlineColorKey = shaderParams.GetParamKey<Vector4>("outlineColor");
			gradientAngleKey = shaderParams.GetParamKey<float>("g_angle");
			lightColorKey = shaderParams.GetParamKey<Vector4>("lightColor");
			lightAngleKey = shaderParams.GetParamKey<Vector2>("lightdir");
			bevelRoundnessKey = shaderParams.GetParamKey<float>("bevelCurvature");
			reflectionKey = shaderParams.GetParamKey<float>("reflection");
			bevelWidthKey = shaderParams.GetParamKey<float>("bevelWidth");

			pixels = new Color4[gradientTexturePixelCount];
			GradientTexture = new Texture2D {
				TextureParams = new TextureParams {
					WrapMode = TextureWrapMode.Clamp,
					MinMagFilter = TextureFilter.Linear,
				}
			};
			Gradient = new ColorGradient(Color4.White, Color4.Black);
		}

		public void Apply(int pass)
		{
			shaderParams.Set(softnessKey, Softness * 0.001f);
			shaderParams.Set(dilateKey, 0.5f - Dilate * 0.01f);
			shaderParams.Set(thicknessKey, -Thickness * 0.01f);
			shaderParams.Set(outlineColorKey, OutlineColor.ToVector4());
			shaderParams.Set(gradientAngleKey, GradientAngle * Mathf.DegToRad);
			shaderParams.Set(lightColorKey, LightColor.ToVector4());
			shaderParams.Set(bevelWidthKey, BevelWidth * 0.01f);
			shaderParams.Set(bevelRoundnessKey, BevelRoundness);
			shaderParams.Set(reflectionKey, ReflectionPower * 0.01f);

			var lightAngle = LightAngle * Mathf.DegToRad;
			shaderParams.Set(lightAngleKey, new Vector2(Mathf.Cos(lightAngle), Mathf.Sin(lightAngle)));
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShaderProgram.GetInstance(GradientEnabled, Thickness > Mathf.ZeroTolerance, BevelEnabled));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
			if (GradientEnabled && Gradient != null) {
				InvalidateTextureIfNecessary();
				PlatformRenderer.SetTextureLegacy(1, GradientTexture);
			}
		}

		private void InvalidateTextureIfNecessary(bool forceInvalidate = false)
		{
			var hash = Gradient.GetHashCode();
			if (forceInvalidate || currentVersion != hash) {
				currentVersion = hash;
				Gradient.Rasterize(ref pixels);
				GradientTexture.LoadImage(pixels, gradientTexturePixelCount, 1);
			}
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SignedDistanceFieldMaterial(blending) {
				Softness = Softness,
				Dilate = Dilate,
				Thickness = Thickness,
			};
		}
	}

	public class SDFMaterialProvider : Sprite.IMaterialProvider
	{
		public SignedDistanceFieldMaterial Material = new SignedDistanceFieldMaterial();
		public IMaterial GetMaterial(int tag) => Material;

		public Sprite.IMaterialProvider Clone() => new SDFMaterialProvider() {
			Material = Material
		};

		public Sprite ProcessSprite(Sprite s)
		{
			s.UV0T2 = Vector2.Zero;
			s.UV1T2 = Vector2.One;
			return s;
		}
	}

	public class SDFShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inColor;
			attribute vec4 inPos;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;

			uniform mat4 matProjection;

			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			varying lowp vec4 color;

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

			uniform lowp float softness;
			uniform lowp float dilate;
			uniform lowp float thickness;
			uniform lowp vec4 outlineColor;

			uniform lowp vec4 lightColor;
			uniform lowp vec2 lightdir;
			uniform lowp float bevelCurvature;
			uniform lowp float reflection;
			uniform lowp float bevelWidth;

			uniform lowp float g_angle;

			void main() {
				lowp vec3 sdf = texture2D(tex1, texCoords1).rgb;
				lowp float distance = sdf.r;
				lowp float outlineFactor = smoothstep(dilate - softness, dilate + softness, distance);
				lowp float alpha = smoothstep(dilate + thickness - softness, dilate + thickness + softness, distance);
				lowp vec4 inner_color = color;
				lowp vec4 bevel_color = outlineColor;
";
		private const string FragmentShaderGradientPart2 = @"
				float g_sin = sin(g_angle);
				float g_cos = cos(g_angle);

				vec2 gradientCoords = vec2(dot(texCoords2, vec2(g_cos, g_sin)), dot(texCoords2, vec2(-g_sin, g_cos)));
				inner_color = texture2D(tex2, gradientCoords);
";
		private const string FragmentShaderBevelPart3 = @"
				lowp float diffuse = clamp(dot(sdf.gb - vec2(0.5, 0.5), lightdir), 0.0, 1.0);
				lowp float curvature = pow(clamp(distance / bevelWidth, 0.0, 1.0), bevelCurvature);
				diffuse = mix(1.0, diffuse, curvature);
				bevel_color = mix(lightColor, outlineColor, (diffuse * (1.0 - reflection) + reflection));
";
		private const string FragmentShaderOutlinePart3 = @"
				lowp vec4 c = mix(bevel_color, inner_color, outlineFactor);
";
		private const string FragmentShaderPart3 = @"
				lowp vec4 c = inner_color;
";
		private const string FragmentShaderPart4 = @"
				gl_FragColor = vec4(c.rgb, color.a * alpha);
			}";

		private static readonly Dictionary<int, SDFShaderProgram> instances = new Dictionary<int, SDFShaderProgram>();

		private static int GetInstanceKey(bool gradient, bool outline, bool bevel) =>
			(gradient ? 1 : 0) | ((outline ? 1 : 0) << 1) | ((bevel ? 1 : 0) << 2);

		public static SDFShaderProgram GetInstance(bool gradient = false, bool outline = false, bool bevel = false)
		{
			var key = GetInstanceKey(gradient, outline, bevel);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new SDFShaderProgram(gradient, outline, bevel));
		}

		private SDFShaderProgram(bool solidColor, bool outline, bool bevel) : base(CreateShaders(solidColor, outline, bevel), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool gradient, bool outline, bool bevel)
		{
			var length =
				FragmentShaderPart1.Length +
				(gradient ? FragmentShaderGradientPart2.Length : 0) +
				(outline ? FragmentShaderOutlinePart3.Length : FragmentShaderPart3.Length) +
				(bevel ? FragmentShaderBevelPart3.Length : 0) +
				FragmentShaderPart4.Length;
			var fragmentShader = new StringBuilder(length);
			fragmentShader.Append(FragmentShaderPart1);
			if (gradient) {
				fragmentShader.Append(FragmentShaderGradientPart2);
			}
			if (outline) {
				if (bevel) {
					fragmentShader.Append(FragmentShaderBevelPart3);
				}
				fragmentShader.Append(FragmentShaderOutlinePart3);
			}
			else {
				fragmentShader.Append(FragmentShaderPart3);
			}

			fragmentShader.Append(FragmentShaderPart4);

			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

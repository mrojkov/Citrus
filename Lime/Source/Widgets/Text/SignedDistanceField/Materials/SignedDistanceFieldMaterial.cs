using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Lime.SignedDistanceField
{
	public class SignedDistanceFieldMaterial : IMaterial
	{
		private const int gradientTexturePixelCount = 256;
		private static readonly ConcurrentDictionary<int, Texture2D> gradientTexturePool = new ConcurrentDictionary<int, Texture2D>();

		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<float> dilateKey;
		private readonly ShaderParamKey<float> thicknessKey;
		private readonly ShaderParamKey<Vector4> outlineColorKey;
		private readonly ShaderParamKey<float> gradientAngleKey;

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
		}

		public void Apply(int pass)
		{
			shaderParams.Set(softnessKey, Mathf.Max(Softness * 0.001f, 0.001f));
			shaderParams.Set(dilateKey, 0.5f - Dilate * 0.01f);
			shaderParams.Set(thicknessKey, -Thickness * 0.01f);
			shaderParams.Set(outlineColorKey, OutlineColor.ToVector4());
			shaderParams.Set(gradientAngleKey, GradientAngle * Mathf.DegToRad);

			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShaderProgram.GetInstance(GradientEnabled, Thickness > Mathf.ZeroTolerance));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
			if (GradientEnabled && Gradient != null) {
				InvalidateTextureIfNecessary();
			}
		}

		private void InvalidateTextureIfNecessary(bool forceInvalidate = false)
		{
			var hash = Gradient.GetHashCode();
			if (forceInvalidate || currentVersion != hash) {
				currentVersion = hash;
				if (gradientTexturePool.TryGetValue(hash, out var texture)) {
					GradientTexture = texture;
				} else {
					var gradientTexturePixels = new Color4[gradientTexturePixelCount];
					Gradient.Rasterize(ref gradientTexturePixels);
					GradientTexture = new Texture2D {
						TextureParams = new TextureParams {
							WrapMode = TextureWrapMode.Clamp,
							MinMagFilter = TextureFilter.Linear,
						}
					};
					GradientTexture.LoadImage(gradientTexturePixels, gradientTexturePixelCount, 1);
					gradientTexturePool.GetOrAdd(hash, GradientTexture);
				}
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
			s.Texture2 = Material.GradientTexture;
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
			#extension GL_OES_standard_derivatives : enable
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;

			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;

			uniform lowp float softness;
			uniform lowp float dilate;
			uniform lowp float thickness;
			uniform lowp vec4 outlineColor;

			uniform lowp float g_angle;

			void main() {
				lowp vec3 sdf = texture2D(tex1, texCoords1).rgb;
				lowp float distance = sdf.r;
				lowp float smoothing = clamp(abs(dFdx(distance)) + abs(dFdy(distance)), 0.0001, 0.05);
				lowp float alpha = smoothstep(dilate + thickness - smoothing, dilate + thickness + smoothing, distance);
				lowp vec4 inner_color = color;
";
		private const string FragmentShaderGradientPart2 = @"
				lowp float g_sin = sin(g_angle);
				lowp float g_cos = cos(g_angle);

				lowp vec2 gradientCoords = vec2(dot(texCoords2, vec2(g_cos, g_sin)), dot(texCoords2, vec2(-g_sin, g_cos)));
				inner_color = texture2D(tex2, gradientCoords);
";
		private const string FragmentShaderOutlinePart3 = @"
				lowp float outlineFactor = smoothstep(dilate - smoothing, dilate + smoothing, distance);
				lowp vec4 c = mix(outlineColor, inner_color, outlineFactor);
";
		private const string FragmentShaderPart3 = @"
				lowp vec4 c = inner_color;
";
		private const string FragmentShaderPart4 = @"
				gl_FragColor = vec4(c.rgb, color.a * alpha);
			}";

		private static readonly Dictionary<int, SDFShaderProgram> instances = new Dictionary<int, SDFShaderProgram>();

		private static int GetInstanceKey(bool gradient, bool outline) =>
			(gradient ? 1 : 0) | ((outline ? 1 : 0) << 1);

		public static SDFShaderProgram GetInstance(bool gradient = false, bool outline = false)
		{
			var key = GetInstanceKey(gradient, outline);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new SDFShaderProgram(gradient, outline));
		}

		private SDFShaderProgram(bool solidColor, bool outline) : base(CreateShaders(solidColor, outline), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool gradient, bool outline)
		{
			var length =
				FragmentShaderPart1.Length +
				(gradient ? FragmentShaderGradientPart2.Length : 0) +
				(outline ? FragmentShaderOutlinePart3.Length : FragmentShaderPart3.Length) +
				FragmentShaderPart4.Length;
			var fragmentShader = new StringBuilder(length);
			fragmentShader.Append(FragmentShaderPart1);
			if (gradient) {
				fragmentShader.Append(FragmentShaderGradientPart2);
			}
			fragmentShader.Append(outline ? FragmentShaderOutlinePart3 : FragmentShaderPart3);
			fragmentShader.Append(FragmentShaderPart4);

			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Lime
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

		private const int gradientTexturePixelCount = 2048;
		private readonly GradientControlPoint[] cachedPoints;
		private readonly Color4[] pixels;
		private int currentVersion;
		private int currentSize;
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
		public Texture2D GradientTexture { get; private set; }

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

			cachedPoints = new GradientControlPoint[gradientTexturePixelCount];
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
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShaderProgram.GetInstance(!GradientEnabled));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
			if (Gradient != null) {
				InvalidateTextureIfNecessary();
				PlatformRenderer.SetTextureLegacy(1, GradientTexture);
			}
		}

		private void InvalidateTextureIfNecessary(bool forceInvalidate = false)
		{
			var hash = Gradient.GetHashCode();
			if (forceInvalidate || currentVersion != hash) {
				currentVersion = hash;
				Array.Clear(cachedPoints, 0, currentSize);
				currentSize = Gradient.Count;
				for (var j = 0; j < currentSize; j++) {
					cachedPoints[j] = Gradient[j];
				}
				var i = 0;
				Array.Sort(cachedPoints, 0, currentSize, Gradient);
				for (var j = 0; j < currentSize - 1; j++) {
					var lastPixel = cachedPoints[j + 1].Position * gradientTexturePixelCount;
					while (i < lastPixel) {
						var start = cachedPoints[j].Position * gradientTexturePixelCount;
						var ratio = (i - start) / (lastPixel - start);
						pixels[i++] = Color4.Lerp(ratio, cachedPoints[j].Color, cachedPoints[j + 1].Color);
					}
				}
				while (i < gradientTexturePixelCount - 1) {
					pixels[i++] = cachedPoints[currentSize - 1].Color;
				}
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
			s.Vertex2UV2 = Vector2.Zero;
			s.Vertex3UV2 = Vector2.Right;
			s.Vertex4UV2 = Vector2.One;
			s.Vertex1UV2 = Vector2.Down;
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

			void main() {
				lowp float distance = texture2D(tex1, texCoords1).r;
				lowp float outlineFactor = smoothstep(dilate - softness, dilate + softness, distance);
				lowp float alpha = smoothstep(dilate + thickness - softness, dilate + thickness + softness, distance);
";
		private const string FragmentShaderTextureColorPart2 = @"
				lowp vec4 g_color = texture2D(tex2, texCoords2);
				lowp vec4 c = mix(outlineColor, g_color, outlineFactor);
				gl_FragColor = vec4(c.rgb, c.a * alpha);
			}";
		private const string FragmentShaderSolidColorPart2 = @"
				lowp vec4 c = mix(outlineColor, color, outlineFactor);
				gl_FragColor = vec4(c.rgb, c.a * alpha);
			}";

		private static readonly Dictionary<int, SDFShaderProgram> instances = new Dictionary<int, SDFShaderProgram>();

		private static int GetInstanceKey(bool solidColor) => solidColor ? 1 : 0;

		public static SDFShaderProgram GetInstance(bool solidColor = true)
		{
			var key = GetInstanceKey(solidColor);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new SDFShaderProgram(solidColor));
		}

		private SDFShaderProgram(bool solidColor) : base(CreateShaders(solidColor), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool solidColor)
		{
			var fragmentShader = new StringBuilder(FragmentShaderPart1.Length + Math.Max(FragmentShaderTextureColorPart2.Length, FragmentShaderTextureColorPart2.Length));
			fragmentShader.Append(FragmentShaderPart1);
			fragmentShader.Append(solidColor ? FragmentShaderSolidColorPart2 : FragmentShaderTextureColorPart2);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

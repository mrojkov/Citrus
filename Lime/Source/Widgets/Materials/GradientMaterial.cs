using System;
using System.Text;
using System.Collections.Generic;

namespace Lime
{
	public class GradientMaterial : IMaterial
	{
		private const int gradientTextureWidth = 2048;
		private int currentVersion;
		private Color4[] pixels;
		private ColorGradient gradient;

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> angleParamKey;
		private readonly ShaderParamKey<float> stretchParamKey;

		public float Angle { get; set; }
		public GradientMaterialBlendMode BlendMode { get; set; } = GradientMaterialBlendMode.Normal;
		public Texture2D GradientTexture { get; private set; }
		public Blending Blending;

		public string Id { get; set; }
		public int PassCount => 1;

		public ColorGradient Gradient
		{
			get => gradient;
			set
			{
				if (gradient != value) {
					gradient = value;
					IsTextureValidated(forceInvalidate: true);
				}
			}
		}

		public GradientMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			angleParamKey = shaderParams.GetParamKey<float>("angle");
			stretchParamKey = shaderParams.GetParamKey<float>("stretch");

			pixels = new Color4[gradientTextureWidth];
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
			if (IsTextureValidated()) {
				var radianAngle = Angle * Mathf.DegToRad;
				var hull = new Rectangle(-Vector2.Half, Vector2.Half).ToQuadrangle() * Matrix32.Rotation(radianAngle);
				var stretch = hull.ToAABB().Size.X;
				shaderParams.Set(stretchParamKey, stretch);
				shaderParams.Set(angleParamKey, radianAngle);
				PlatformRenderer.SetTexture(1, GradientTexture);
				PlatformRenderer.SetBlendState(Blending.GetBlendState());
				PlatformRenderer.SetShaderParams(shaderParamsArray);
				PlatformRenderer.SetShaderProgram(GradientShaderProgram.GetInstance(BlendMode));
			}
		}

		private bool IsTextureValidated(bool forceInvalidate = false)
		{
			if (Gradient == null || Gradient.Count == 0) {
				return false;
			}

			var hash = Gradient.GetHashCode();
			if (forceInvalidate || currentVersion != hash) {
				currentVersion = hash;
				Gradient.Rasterize(ref pixels);
				GradientTexture.LoadImage(pixels, width: gradientTextureWidth, height: 1);
			}
			return true;
		}

		public void Invalidate()
		{
			IsTextureValidated(forceInvalidate: true);
		}

		public IMaterial Clone()
		{
			return (IMaterial) MemberwiseClone();
		}
	}

	public enum GradientMaterialBlendMode
	{
		Normal,
		Dissolve,
		Multiply,
		LinearBurn,
		ColorBurn,
		ColorDodge,
		Addition, // Linear Dodge
		Subtract,
		Divide,
		Difference,
		Exclusion,
		Screen,
		Overlay,
		Lighten,
		Darken
	}

	public class GradientShaderProgram : ShaderProgram
	{
		private static GradientShaderProgram instance;
		private static readonly Dictionary<int, GradientShaderProgram> instances =
			new Dictionary<int, GradientShaderProgram>(Enum.GetNames(typeof(GradientMaterialBlendMode)).Length);

		private static int GetInstanceKey(GradientMaterialBlendMode blend) => (int)blend;

		public static GradientShaderProgram GetInstance(GradientMaterialBlendMode blend)
		{
			var key = GetInstanceKey(blend);
			return
				instances.TryGetValue(key, out var shaderProgram)
				? shaderProgram
				: (instances[key] = new GradientShaderProgram(blend));
		}

		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inColor;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;

			uniform mat4 matProjection;

			varying vec4 color;
			varying vec2 uv1;
			varying vec2 uv2;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				uv1 = inTexCoords1;
				uv2 = inTexCoords2;
			}";

		private const string FragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 uv1;
			varying lowp vec2 uv2;

			uniform sampler2D tex1;
			uniform sampler2D tex2;
			uniform lowp float angle;
			uniform lowp float stretch;

			const lowp vec2 offset = vec2(0.5);
			const lowp vec4 one = vec4(1.0);
			const lowp vec3 one3 = vec3(1.0);
			const lowp vec3 half3 = vec3(0.5);

			void main()
			{
				lowp float s = sin(angle);
				lowp float c = cos(angle);
				lowp vec2 uv = (uv2 - offset) / stretch;
				lowp vec2 gradientUV = vec2(dot(uv, vec2(c, s)), dot(uv, vec2(-s, c))) + offset;
				lowp vec4 c1 = texture2D(tex2, gradientUV);
			";

		private const string FragmentFetchOriginalTexture = @"
				lowp vec4 c2 =  texture2D(tex1, uv1);
			";

		private const string FragmentBlendNormal = @"
				gl_FragColor = color * c1;
			}";

		private const string FragmentBlendDissolve = @"
				lowp float rnd = fract(sin(dot(uv1, vec2(12.9898, 78.233))) * 43758.5453);
				gl_FragColor = color * vec4(c1.rgb * step(rnd, c1.a) + c2.rgb * step(rnd, 1.0 - c1.a), c1.a * c2.a);
			}";

		private const string FragmentBlendLinearBurn = @"
				gl_FragColor = color * (c2 + c1 - one);
			}";

		private const string FragmentBlendColorBurn = @"
				gl_FragColor = color * (one - (one - c2) / c1);
			}";

		private const string FragmentBlendColorDodge = @"
				gl_FragColor = color * c2 / (one - c1);
			}";

		private const string FragmentBlendMultiply = @"
				gl_FragColor = color * (c2 * c1);
			}";

		private const string FragmentBlendAddition = @"
				gl_FragColor = color * (c2 + c1);
			}";

		private const string FragmentBlendSubtract = @"
				gl_FragColor = color * (c2 - c1);
			}";

		private const string FragmentBlendDivide = @"
				gl_FragColor = color * (c2 / c1);
			}";

		private const string FragmentBlendDifference = @"
				gl_FragColor = color * vec4(abs(c2.rgb - c1.rgb), c1.a * c2.a);
			}";

		private const string FragmentBlendExclusion = @"
				gl_FragColor = color * vec4(c1.rgb + c2.rgb - 2.0 * c1.rgb * c2.rgb, c1.a * c2.a);
			}";

		private const string FragmentBlendScreen = @"
				gl_FragColor = color * vec4(one3 - (one3 - c2.rgb) * (one3 - c1.rgb), c1.a * c2.a);
			}";

		private const string FragmentBlendOverlay = @"
				lowp vec3 cond = step(half3, c2.rgb);
				gl_FragColor = color * vec4(
						(one3 - cond) * (2.0 * c1.rgb * c2.rgb) +
						cond * (one3 - 2.0 * (one3 - c2.rgb) * (one3 - c1.rgb)),
					c1.a * c2.a);
			}";

		private const string FragmentBlendLighten = @"
				gl_FragColor = color * vec4(max(c1.rgb, c2.rgb), c1.a * c2.a);
			}";

		private const string FragmentBlendDarken = @"
				gl_FragColor = color * vec4(min(c1.rgb, c2.rgb), c1.a * c2.a);
			}";

		// More about blending: https://photoblogstop.com/photoshop/photoshop-blend-modes-explained#BlendModeDescriptions

		private GradientShaderProgram(GradientMaterialBlendMode blend)
			: base(CreateShaders(blend), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(GradientMaterialBlendMode blend)
		{
			var fragmentShader = new StringBuilder(FragmentShader, FragmentShader.Length + FragmentBlendNormal.Length);

			if (blend == GradientMaterialBlendMode.Normal) {
				fragmentShader.Append(FragmentBlendNormal);
			} else {
				fragmentShader.Append(FragmentFetchOriginalTexture);
				switch (blend) {
					case GradientMaterialBlendMode.Dissolve:
						fragmentShader.Append(FragmentBlendDissolve);
						break;
					case GradientMaterialBlendMode.Multiply:
						fragmentShader.Append(FragmentBlendMultiply);
						break;
					case GradientMaterialBlendMode.LinearBurn:
						fragmentShader.Append(FragmentBlendLinearBurn);
						break;
					case GradientMaterialBlendMode.ColorBurn:
						fragmentShader.Append(FragmentBlendColorBurn);
						break;
					case GradientMaterialBlendMode.ColorDodge:
						fragmentShader.Append(FragmentBlendColorDodge);
						break;
					case GradientMaterialBlendMode.Addition:
						fragmentShader.Append(FragmentBlendAddition);
						break;
					case GradientMaterialBlendMode.Subtract:
						fragmentShader.Append(FragmentBlendSubtract);
						break;
					case GradientMaterialBlendMode.Divide:
						fragmentShader.Append(FragmentBlendDivide);
						break;
					case GradientMaterialBlendMode.Difference:
						fragmentShader.Append(FragmentBlendDifference);
						break;
					case GradientMaterialBlendMode.Exclusion:
						fragmentShader.Append(FragmentBlendExclusion);
						break;
					case GradientMaterialBlendMode.Screen:
						fragmentShader.Append(FragmentBlendScreen);
						break;
					case GradientMaterialBlendMode.Overlay:
						fragmentShader.Append(FragmentBlendOverlay);
						break;
					case GradientMaterialBlendMode.Lighten:
						fragmentShader.Append(FragmentBlendLighten);
						break;
					case GradientMaterialBlendMode.Darken:
						fragmentShader.Append(FragmentBlendDarken);
						break;
					default:
						throw new NotSupportedException("Unsupported blending mode");
				}
			}

			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

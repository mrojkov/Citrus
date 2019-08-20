using System;

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
		public Blending Blending { get; set; } = Blending.Inherited;
		public Texture2D GradientTexture { get; private set; }

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
				PlatformRenderer.SetBlendState(Blending.GetBlendState());
				PlatformRenderer.SetShaderParams(shaderParamsArray);
				PlatformRenderer.SetShaderProgram(GradientShaderProgram.Instance);
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

		public class GradientShaderProgram : ShaderProgram
		{
			private static GradientShaderProgram instance;
			public static ShaderProgram Instance => instance ?? (instance = new GradientShaderProgram());

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

				void main()
				{
					lowp float s = sin(angle);
					lowp float c = cos(angle);
					lowp vec2 offset = vec2(0.5);
					lowp vec2 uv = (uv2 - offset) / stretch;
					lowp vec2 gradientCoords = vec2(dot(uv, vec2(c, s)), dot(uv, vec2(-s, c))) + offset;
					// multiplication below can be replaced with add/sub/etc for more blending features
					gl_FragColor = texture2D(tex1, uv1) * texture2D(tex2, gradientCoords);
				}";

			private GradientShaderProgram()
				: base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

			private static Shader[] CreateShaders()
			{
				return new Shader[] {
					new VertexShader(VertexShader),
					new FragmentShader(FragmentShader)
				};
			}
		}
	}
}

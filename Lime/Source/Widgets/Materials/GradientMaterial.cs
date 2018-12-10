using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class GradientComponent : MaterialComponent<GradientMaterial>
	{
		[YuzuMember]
		public float Angle
		{
			get => CustomMaterial.Angle;
			set => CustomMaterial.Angle = value;
		}

		[YuzuMember]
		public Blending Blending
		{
			get => CustomMaterial.Blending;
			set => CustomMaterial.Blending = value;
		}

		[YuzuMember]
		public ColorGradient Gradient
		{
			get => CustomMaterial.Gradient;
			set => CustomMaterial.Gradient = value;
		}

		public GradientComponent()
		{ }
	}

	public class GradientMaterial : IMaterial
	{
		const string vertexShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			varying vec4 v_Color;
			varying vec2 v_TexCoords;
			uniform mat4 matProjection;

			void main()
			{
				gl_Position = matProjection * inPos;
				v_Color = inColor;
				v_TexCoords = inTexCoords1;
			}";

		const string fragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;
			varying vec2 v_TexCoords;

			uniform sampler2D u_GradientTexture;
			uniform float u_Angle;
			uniform float u_Stretch;

			void main()
			{
				float s = sin(u_Angle);
				float c = cos(u_Angle);
				vec2 texCoords = v_TexCoords;
				vec2 offset = vec2(0.5);
				texCoords -= offset;
				texCoords /= u_Stretch;
				vec2 gradientCoords = vec2(dot(texCoords, vec2(c, s)), dot(texCoords, vec2(-s, c))) + offset;
				gl_FragColor = texture2D(u_GradientTexture, gradientCoords);
			}";

		private const int gradientTexturePixelCount = 2048;
		private Color4[] pixels;
		private ColorGradient gradient;
		private ShaderProgram shaderProgram;
		private int currentVersion;
		public string Id { get; set; }
		public int PassCount => 1;

		public ColorGradient Gradient
		{
			get => gradient;
			set
			{
				if (gradient != value) {
					gradient = value;
					InvalidateTextureIfNecessary(forceInvalidate: true);
				}
			}
		}

		public float Angle { get; set; }

		public Blending Blending { get; set; } = Blending.Inherited;

		public Texture2D GradientTexture { get; private set; }

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> angleParamKey;
		private readonly ShaderParamKey<float> stretchParamKey;

		public GradientMaterial()
		{
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { shaderParams, Renderer.GlobalShaderParams  };
			angleParamKey = shaderParams.GetParamKey<float>("u_Angle");
			stretchParamKey = shaderParams.GetParamKey<float>("u_Stretch");
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
			if (Gradient != null) {
				InvalidateTextureIfNecessary();
				var hull = new Rectangle(-Vector2.Half, Vector2.Half).ToQuadrangle() * Matrix32.Rotation(Angle * Mathf.DegToRad);
				var stretch = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue)
					.IncludingPoint(hull.V1)
					.IncludingPoint(hull.V2)
					.IncludingPoint(hull.V3)
					.IncludingPoint(hull.V4).Size.X;
				shaderParams.Set(stretchParamKey, stretch);
				shaderParams.Set(angleParamKey, Angle * Mathf.DegToRad);
				PlatformRenderer.SetBlendState(Blending.GetBlendState());
				PlatformRenderer.SetShaderParams(shaderParamsArray);
				PlatformRenderer.SetShaderProgram(GetShaderProgram());
				PlatformRenderer.SetTextureLegacy(0, GradientTexture);
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

		private ShaderProgram GetShaderProgram()
		{
			if (shaderProgram == null) {
				shaderProgram = new ShaderProgram(
					GetShaders(),
					ShaderPrograms.Attributes.GetLocations(),
					GetSamplers());
			}
			return shaderProgram;
		}

		private static IEnumerable<ShaderProgram.Sampler> GetSamplers()
		{
			yield return new ShaderProgram.Sampler { Name = "u_GradientTexture", Stage = 0 };
		}

		private static IEnumerable<Shader> GetShaders()
		{
			yield return new VertexShader(vertexShader);
			yield return new FragmentShader(fragmentShader);
		}

		public IMaterial Clone()
		{
			return (IMaterial) MemberwiseClone();
		}

		public void Invalidate()
		{
			shaderProgram = null;
		}
	}
}

using System.Collections.Generic;

namespace Lime.SignedDistanceField
{

	public class SDFInnerShadowMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> dilateKey;
		private readonly ShaderParamKey<float> textDilateKey;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<Vector4> colorKey;
		private readonly ShaderParamKey<Vector2> offsetKey;

		public float Dilate { get; set; } = 0f;
		public float TextDilate { get; set; } = 0f;
		public float Softness { get; set; } = 0f;
		public Color4 Color { get; set; } = Color4.Black;
		public Vector2 Offset { get; set; } = new Vector2();

		public int PassCount => 1;

		public string Id { get; set; }

		public SDFInnerShadowMaterial() : this(Blending.Alpha) { }

		public SDFInnerShadowMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			dilateKey = shaderParams.GetParamKey<float>("dilate");
			textDilateKey = shaderParams.GetParamKey<float>("text_dilate");
			softnessKey = shaderParams.GetParamKey<float>("softness");
			colorKey = shaderParams.GetParamKey<Vector4>("color");
			offsetKey = shaderParams.GetParamKey<Vector2>("offset");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(dilateKey, 0.5f - Dilate * 0.01f);
			shaderParams.Set(textDilateKey, 0.5f - TextDilate * 0.01f);
			shaderParams.Set(softnessKey, Mathf.Max(Softness * 0.001f, 0.001f));
			shaderParams.Set(colorKey, Color.ToVector4());
			shaderParams.Set(offsetKey, Offset);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFInnerShadowShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SDFInnerShadowMaterial(blending) {
				Dilate = Dilate,
				TextDilate = TextDilate,
				Color = Color,
				Softness = Softness,
				Offset = Offset,
			};
		}
	}

	public class SDFInnerShadowMaterialProvider : Sprite.IMaterialProvider
	{
		public SDFInnerShadowMaterial Material = new SDFInnerShadowMaterial();
		public IMaterial GetMaterial(int tag) => Material;

		public Sprite.IMaterialProvider Clone() => new SDFInnerShadowMaterialProvider() {
			Material = Material
		};

		public Sprite ProcessSprite(Sprite s) => s;
	}

	public class SDFInnerShadowShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inColor;
			attribute vec4 inPos;
			attribute vec2 inTexCoords1;

			uniform mat4 matProjection;

			varying lowp vec2 texCoords1;
			varying lowp vec4 global_color;

			void main()
			{
				gl_Position = matProjection * inPos;
				global_color = inColor;
				texCoords1 = inTexCoords1;
			}";

		private const string FragmentShader = @"
			#extension GL_OES_standard_derivatives : enable
			varying lowp vec4 global_color;
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float text_dilate;
			uniform lowp float dilate;
			uniform lowp float softness;
			uniform lowp vec4 color;
			uniform lowp vec2 offset;

			void main() {
				lowp float textDistance = texture2D(tex1, texCoords1).r;
				lowp float textSmoothing = clamp(abs(dFdx(textDistance)) + abs(dFdy(textDistance)), 0.0001, 0.05);
				lowp float textFactor = smoothstep(text_dilate - textSmoothing, text_dilate + textSmoothing, textDistance);
				lowp float shadowDistance = texture2D(tex1, texCoords1 - offset).r;
				lowp float smoothing = clamp(abs(dFdx(shadowDistance)) + abs(dFdy(shadowDistance)), 0.0001, 0.05);
				lowp float shadowFactor = smoothstep(dilate - smoothing, dilate + smoothing, shadowDistance);
				lowp float innerShadowFactor = textFactor * (1.0 - shadowFactor);
				gl_FragColor = vec4(color.rgb, color.a * innerShadowFactor * global_color.a);
			}";

		private static SDFInnerShadowShaderProgram instance;

		public static SDFInnerShadowShaderProgram GetInstance() => instance ?? (instance = new SDFInnerShadowShaderProgram());

		private SDFInnerShadowShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

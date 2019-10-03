using System.Collections.Generic;
using Yuzu;

namespace Lime.SignedDistanceField
{

	public class SDFShadowMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> dilateKey;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<Vector4> colorKey;

		[YuzuMember]
		public Blending Blending { get; set; }
		[YuzuMember]
		public float Dilate { get; set; } = 0f;
		[YuzuMember]
		public float Softness { get; set; } = 0f;
		[YuzuMember]
		public Color4 Color { get; set; } = Color4.Black;

		public int PassCount => 1;

		public string Id { get; set; }

		public SDFShadowMaterial() : this(Blending.Alpha) { }

		public SDFShadowMaterial(Blending blending)
		{
			Blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			dilateKey = shaderParams.GetParamKey<float>("dilate");
			softnessKey = shaderParams.GetParamKey<float>("softness");
			colorKey = shaderParams.GetParamKey<Vector4>("color");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(dilateKey, 0.5f - Dilate * 0.01f);
			shaderParams.Set(softnessKey, Mathf.Max(Softness * 0.01f, 0.001f));
			shaderParams.Set(colorKey, Color.ToVector4());
			PlatformRenderer.SetBlendState(Blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShadowShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }
	}

	public class SDFShadowMaterialProvider : Sprite.IMaterialProvider
	{
		public SDFShadowMaterial Material = new SDFShadowMaterial();
		public Vector2 Offset { get; set; }
		public IMaterial GetMaterial(int tag) => Material;

		public Sprite.IMaterialProvider Clone() => new SDFShadowMaterialProvider() {
			Material = Material
		};

		public Sprite ProcessSprite(Sprite s) => s;
	}

	public class SDFShadowShaderProgram : ShaderProgram
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

			uniform lowp float dilate;
			uniform lowp float softness;
			uniform lowp vec4 color;

			void main() {
				lowp float shadowDistance = texture2D(tex1, texCoords1).r;
				lowp float smoothing = clamp(abs(dFdx(shadowDistance)) + abs(dFdy(shadowDistance)), 0.0001, 0.05);
				lowp float shadowAlpha = smoothstep(dilate - softness - smoothing, dilate + softness + smoothing, shadowDistance);
				gl_FragColor = vec4(color.rgb, color.a * shadowAlpha * global_color.a);
			}";

		private static SDFShadowShaderProgram instance;

		public static SDFShadowShaderProgram GetInstance() => instance ?? (instance = new SDFShadowShaderProgram());

		private SDFShadowShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

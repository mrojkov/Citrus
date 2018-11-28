using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class SDFUnderlayMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> dilateKey;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<Vector4> colorKey;
		private readonly ShaderParamKey<Vector2> offsetKey;

		public float Dilate { get; set; } = 0f;
		public float Softness { get; set; } = 0f;
		public Color4 Color { get; set; } = Color4.Black;
		public Vector2 Offset { get; set; } = new Vector2();

		public int PassCount => 1;

		public SDFUnderlayMaterial() : this(Blending.Alpha) { }

		public SDFUnderlayMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			dilateKey = shaderParams.GetParamKey<float>("dilate");
			softnessKey = shaderParams.GetParamKey<float>("softness");
			colorKey = shaderParams.GetParamKey<Vector4>("color");
			offsetKey = shaderParams.GetParamKey<Vector2>("offset");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(dilateKey, 0.5f - Dilate);
			shaderParams.Set(softnessKey, Softness);
			shaderParams.Set(colorKey, Color.ToVector4());
			shaderParams.Set(offsetKey, Offset / 100);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFUnderlayShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SDFUnderlayMaterial(blending) {
				Dilate = Dilate,
				Color = Color,
				Softness = Softness,
				Offset = Offset,
			};
		}
	}

	public class SDFUnderlayMaterialProvider : Sprite.IMaterialProvider
	{
		public SDFUnderlayMaterial Material = new SDFUnderlayMaterial();
		public IMaterial GetMaterial(int tag) => Material;

		public Sprite.IMaterialProvider Clone() => new SDFUnderlayMaterialProvider() {
			Material = Material
		};
	}

	public class SDFUnderlayShaderProgram : ShaderProgram
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

		private const string FragmentShader = @"
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float dilate;
			uniform lowp float softness;
			uniform lowp vec4 color;
			uniform lowp vec2 offset;

			void main() {
				lowp float shadowDistance = texture2D(tex1, texCoords1 - offset).r;
				lowp float shadowAlpha = smoothstep(dilate - softness, dilate + softness, shadowDistance);
				gl_FragColor = vec4(color.rgb, color.a * shadowAlpha);
			}";

		private static SDFUnderlayShaderProgram instance;

		public static SDFUnderlayShaderProgram GetInstance() => instance ?? (instance = new SDFUnderlayShaderProgram());

		private SDFUnderlayShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

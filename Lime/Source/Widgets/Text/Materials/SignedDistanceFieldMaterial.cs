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

		public float Softness { get; set; } = 0f;
		public float Dilate { get; set; } = 0f;
		public float Thickness { get; set; } = 0f;
		public Color4 OutlineColor { get; set; } = Color4.Black;

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
		}

		public void Apply(int pass)
		{
			shaderParams.Set(softnessKey, Softness);
			shaderParams.Set(dilateKey, 0.5f - Dilate);
			shaderParams.Set(thicknessKey, -Thickness);
			shaderParams.Set(outlineColorKey, OutlineColor.ToVector4());
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
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
	}

	public class SDFShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inColor;
			attribute vec4 inPos;
			attribute vec2 inTexCoords1;

			uniform mat4 matProjection;

			varying lowp vec2 texCoords1;
			varying lowp vec4 color;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
			}";

		private const string FragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float softness;
			uniform lowp float dilate;
			uniform lowp float thickness;
			uniform lowp vec4 outlineColor;

			void main() {
				lowp float distance = texture2D(tex1, texCoords1).r;
				lowp float outlineFactor = smoothstep(dilate - softness, dilate + softness, distance);
				lowp vec4 c = mix(outlineColor, color, outlineFactor);
				lowp float alpha = smoothstep(dilate + thickness - softness, dilate + thickness + softness, distance);
				gl_FragColor = vec4(c.rgb, c.a * alpha);
			}";

		private static SDFShaderProgram instance;

		public static SDFShaderProgram GetInstance() => instance ?? (instance = new SDFShaderProgram());

		private SDFShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

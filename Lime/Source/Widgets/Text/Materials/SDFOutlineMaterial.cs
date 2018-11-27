using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class SDFOutlineMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> thicknessKey;
		private readonly ShaderParamKey<float> softnessKey;

		public float Thickness { get; set; } = 0f;
		public float Softness { get; set; } = 0f;

		public int PassCount => 1;

		public SDFOutlineMaterial() : this(Blending.Alpha) { }

		public SDFOutlineMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			thicknessKey = shaderParams.GetParamKey<float>("thickness");
			softnessKey = shaderParams.GetParamKey<float>("softness");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(thicknessKey, Thickness);
			shaderParams.Set(softnessKey, Softness * 100f);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFOutlineShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SDFOutlineMaterial(blending) {
				Thickness = Thickness,
			};
		}
	}

	public class SDFOutlineShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inColor;
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

			uniform lowp float thickness;
			uniform lowp float softness;

			void main() {
				lowp float c = texture2D(tex1, texCoords1).r;
				lowp float c1 = (c - (.5 - thickness)) * softness;
				lowp float c2 = 1. - (c - .5) * softness;
				lowp float res = mix(c1, c2, (c-.5)*softness);
				gl_FragColor = vec4(color.rgb, color.a * res);
			}";

		private static SDFOutlineShaderProgram instance;

		public static SDFOutlineShaderProgram GetInstance() => instance ?? (instance = new SDFOutlineShaderProgram());

		private SDFOutlineShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}
	}
}

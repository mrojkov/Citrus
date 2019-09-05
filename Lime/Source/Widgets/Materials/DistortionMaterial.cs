using System.Collections.Generic;
using System.Text;
using Yuzu;

namespace Lime
{
	public class DistortionMaterial : IMaterial
	{
		private static readonly BlendState disabledBlendingState = new BlendState { Enable = false };

		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<Vector4> amountKey;

		[YuzuMember]
		public Blending Blending { get; set; }
		[YuzuMember]
		public float BarrelPincushion { get; set; }
		[YuzuMember]
		public float ChromaticAberration { get; set; }
		[YuzuMember]
		public float Red { get; set; } = 1f;
		[YuzuMember]
		public float Green { get; set; }
		[YuzuMember]
		public float Blue { get; set; } = -1f;
		[YuzuMember]
		public bool Opaque { get; set; }

		public string Id { get; set; }
		public int PassCount => 1;

		public DistortionMaterial() : this(Blending.Alpha) { }

		public DistortionMaterial(Blending blending)
		{
			Blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			amountKey = shaderParams.GetParamKey<Vector4>("uAmount");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(amountKey, new Vector4(ChromaticAberration * Red, ChromaticAberration * Green, ChromaticAberration * Blue, BarrelPincushion));
			PlatformRenderer.SetBlendState(!Opaque ? Blending.GetBlendState() : disabledBlendingState);
			PlatformRenderer.SetShaderProgram(DistortionShaderProgram.GetInstance(Opaque));
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }
	}

	public class DistortionShaderProgram : ShaderProgram
	{
		private const string VertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inColor;
			attribute vec2 inTexCoords1;

			uniform mat4 matProjection;

			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
			}";

		private const string FragmentShaderPart1 = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			uniform lowp sampler2D tex1;
			uniform lowp vec4 uAmount;

			void main() {
				lowp vec2 u = texCoords1 * 2.0 - vec2(1.0);
				lowp float radiusSquared = dot(u, u);
				lowp vec2 d = (u * (1.0 - uAmount.w * radiusSquared)) / (1.0 - 2.0 * min(uAmount.w, 0.0));
				lowp vec2 nCoord = 0.5 * d + vec2(0.5);
				lowp vec2 aberrationDir = texCoords1 - vec2(0.5);
				lowp vec4 c = vec4(
					texture2D(tex1, nCoord + uAmount.x * aberrationDir).x,
					texture2D(tex1, nCoord + uAmount.y * aberrationDir).y,
					texture2D(tex1, nCoord + uAmount.z * aberrationDir).z,
					1.0
				);
			";
		private const string FragmentShaderPart2 = @"
				gl_FragColor = color * c;
			}";
		private const string FragmentShaderPart2Opaque = @"
				gl_FragColor = vec4(color.rgb * c.rgb, 1.0);
			}";

		private static readonly Dictionary<int, DistortionShaderProgram> instances = new Dictionary<int, DistortionShaderProgram>();

		private static int GetInstanceKey(bool opaque) => opaque ? 1 : 0;

		public static DistortionShaderProgram GetInstance(bool opaque = false)
		{
			var key = GetInstanceKey(false);
			return instances.TryGetValue(key, out var shaderProgram) ? shaderProgram : (instances[key] = new DistortionShaderProgram(opaque));
		}

		private DistortionShaderProgram(bool opaque) : base(CreateShaders(opaque), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders(bool opaque)
		{
			var length = FragmentShaderPart1.Length + (!opaque ? FragmentShaderPart2.Length : FragmentShaderPart2Opaque.Length);
			var fragmentShader = new StringBuilder(length);
			fragmentShader.Append(FragmentShaderPart1);
			fragmentShader.Append(!opaque ? FragmentShaderPart2 : FragmentShaderPart2Opaque);
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(fragmentShader.ToString())
			};
		}
	}
}

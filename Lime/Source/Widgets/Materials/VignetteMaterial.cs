namespace Lime
{
	public class VignetteMaterial : IMaterial
	{
		private readonly Blending blending;
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParams shaderParams;
		private readonly ShaderParamKey<float> radiusKey;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<Vector2> uv1Key;
		private readonly ShaderParamKey<Vector2> uvOffsetKey;
		private readonly ShaderParamKey<Vector4> colorKey;

		public float Radius { get; set; } = 0.75f;
		public float Softness { get; set; } = 0.45f;
		public Vector2 UV1 { get; set; } = Vector2.One;
		public Vector2 UVOffset { get; set; } = Vector2.Half;
		public Color4 Color { get; set; } = Color4.Black;

		public string Id { get; set; }
		public int PassCount => 1;

		public VignetteMaterial() : this(Blending.Alpha) { }

		public VignetteMaterial(Blending blending)
		{
			this.blending = blending;
			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
			radiusKey = shaderParams.GetParamKey<float>("radius");
			softnessKey = shaderParams.GetParamKey<float>("softness");
			uv1Key = shaderParams.GetParamKey<Vector2>("uv1");
			uvOffsetKey = shaderParams.GetParamKey<Vector2>("uvOffset");
			colorKey = shaderParams.GetParamKey<Vector4>("vignetteColor");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(radiusKey, Radius);
			shaderParams.Set(softnessKey, Softness);
			shaderParams.Set(uv1Key, UV1);
			shaderParams.Set(uvOffsetKey, UVOffset);
			shaderParams.Set(colorKey, Color.ToVector4());
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(VignetteShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new VignetteMaterial(blending) {
				Radius = Radius,
				Softness = Softness,
				UV1 = UV1,
				UVOffset = UVOffset,
				Color = Color
			};
		}
	}

	public class VignetteShaderProgram : ShaderProgram
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

		private const string FragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;

			uniform lowp sampler2D tex1;
			uniform lowp float radius;
			uniform lowp float softness;
			uniform lowp vec2 uv1;
			uniform lowp vec2 uvOffset;
			uniform lowp vec4 vignetteColor;

			void main() {
				lowp vec4 texColor = texture2D(tex1, texCoords1);
				lowp vec2 position = texCoords1.xy * uv1 - uvOffset;
				lowp float len = length(position);
				lowp float vignette = 1.0 - smoothstep(radius, radius-softness, len);
				texColor.rgb = mix(texColor.rgb, vignetteColor.rgb, vignette * vignetteColor.a);
				texColor.a = max(texColor.a, vignette * vignetteColor.a);
				gl_FragColor = texColor * color;
			}";

		private static VignetteShaderProgram instance;

		public static VignetteShaderProgram GetInstance() => instance ?? (instance = new VignetteShaderProgram());

		private VignetteShaderProgram() : base(CreateShaders(), ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers()) { }

		private static Shader[] CreateShaders() => new Shader[] {
			new VertexShader(VertexShader),
			new FragmentShader(FragmentShader)
		};
	}
}

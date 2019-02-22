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
		private readonly ShaderParamKey<float> smoothingKey;
		private readonly ShaderParamKey<float> softnessKey;
		private readonly ShaderParamKey<Vector4> colorKey;
		private readonly ShaderParamKey<Vector2> offsetKey;

		public float FontSize { get; set; } = 0f;
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
			smoothingKey = shaderParams.GetParamKey<float>("smoothing");
			softnessKey = shaderParams.GetParamKey<float>("softness");
			colorKey = shaderParams.GetParamKey<Vector4>("color");
			offsetKey = shaderParams.GetParamKey<Vector2>("offset");
		}

		public void Apply(int pass)
		{
			shaderParams.Set(smoothingKey, Mathf.Min(1f / FontSize * 6f, 0.05f));
			shaderParams.Set(dilateKey, 0.5f - Dilate * 0.01f);
			shaderParams.Set(textDilateKey, 0.5f - TextDilate * 0.01f);
			shaderParams.Set(softnessKey, Softness * 0.001f);
			shaderParams.Set(colorKey, Color.ToVector4());
			shaderParams.Set(offsetKey, Offset / 100);
			PlatformRenderer.SetBlendState(blending.GetBlendState());
			PlatformRenderer.SetShaderProgram(SDFInnerShadowShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		public IMaterial Clone()
		{
			return new SDFShadowMaterial(blending) {
				Dilate = Dilate,
				Color = Color,
				Softness = Softness,
				Offset = Offset,
			};
		}
	}

	public class SDFInnerShadowMaterialProvider : Sprite.IMaterialProvider
	{
		internal bool Free = true;

		public SDFInnerShadowMaterial Material = new SDFInnerShadowMaterial();
		public IMaterial GetMaterial(int tag) => Material;

		public Sprite.IMaterialProvider Clone() => new SDFInnerShadowMaterialProvider() {
			Material = Material
		};

		public Sprite ProcessSprite(Sprite s) => s;

		public void Release()
		{
			if (Free) return;
			try {
				OnRelease();
			} finally {
				Free = true;
			}
		}

		protected virtual void OnRelease() { }
	}

	public static class SDFInnerShadowMaterialProviderPool<T> where T : SDFInnerShadowMaterialProvider, new()
	{
		private static T[] items = new T[1] { new T() };
		private static int index;

		public static T Acquire()
		{
			for (int i = 0; i < items.Length; i++) {
				var item = items[index++];
				if (index == items.Length)
					index = 0;
				if (item.Free) {
					item.Free = false;
					return item;
				}
			}
			System.Array.Resize(ref items, items.Length * 2);
			index = items.Length / 2;
			for (int i = index; i < items.Length; i++) {
				items[i] = new T();
			}
			items[index].Free = false;
			return items[index];
		}
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
			varying lowp vec4 global_color;
			varying lowp vec2 texCoords1;
			uniform lowp sampler2D tex1;

			uniform lowp float text_dilate;
			uniform lowp float smoothing;
			uniform lowp float dilate;
			uniform lowp float softness;
			uniform lowp vec4 color;
			uniform lowp vec2 offset;

			void main() {
				lowp float textDistance = texture2D(tex1, texCoords1).r;
				lowp float textFactor = smoothstep(text_dilate - smoothing, text_dilate + smoothing, textDistance);
				lowp float shadowDistance = texture2D(tex1, texCoords1 - offset).r;
				lowp float shadowFactor = smoothstep(dilate - softness, dilate + softness, shadowDistance);
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

using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class HSLComponent : MaterialComponent<HSLMaterial>
	{
		[YuzuMember]
		public float Hue
		{
			get => CustomMaterial.HSL.X * 360.0f;
			set => CustomMaterial.HSL.X = value / 360.0f;
		}

		[YuzuMember]
		public float Saturation
		{
			get => CustomMaterial.HSL.Y * 100.0f;
			set => CustomMaterial.HSL.Y = value / 100.0f;
		}

		[YuzuMember]
		public float Lightness
		{
			get => CustomMaterial.HSL.Z * 100.0f;
			set => CustomMaterial.HSL.Z = value / 100.0f;
		}

		public HSLComponent()
		{ }
	}

	public class HSLMaterial : IMaterial
	{
		private static string vs = @"
				uniform mat4 matProjection;

				attribute vec4 inPos;
				attribute vec2 inTexCoords1;

				varying lowp vec2 v_uv;

				void main()
				{
					gl_Position = matProjection * inPos;
					v_uv = inTexCoords1;
				}";

		private static string fs = @"
			uniform lowp sampler2D tex1;
			uniform lowp vec3 u_hsl;

			varying lowp vec2 v_uv;

			lowp vec3 HslToRgb(lowp vec3 hslColor)
			{
			    lowp vec3 rgbColor = vec3(abs(hslColor.x - 3.0) - 1.0, 2.0 - abs(hslColor.x * 6.0 - 2.0), 2.0 - abs(hslColor.x * 6.0 - 4.0));
			    lowp float saturationLightness = (1.0 - abs(2.0 * hslColor.z - 1.0)) * hslColor.y;
			    return clamp(rgbColor, vec3(0.0), vec3(1.0)) * saturationLightness + vec3(hslColor.z - saturationLightness / 2.0);
			}

			lowp vec3 RgbToHsl(lowp vec3 rgbColor)
			{
			    lowp vec4 chroma = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			    // Find max and min values.
			    lowp vec4 tmp = mix(vec4(rgbColor.bg, chroma.wz), vec4(rgbColor.gb, chroma.xy), step(rgbColor.b, rgbColor.g));
			    lowp vec4 values = mix(vec4(tmp.xyw, rgbColor.r), vec4(rgbColor.r, tmp.yzx), step(tmp.x, rgbColor.r));
			    lowp float minValue = min(values.w, values.y);
			    lowp float maxValue = values.x;
			    lowp float delta = maxValue - minValue;
			    lowp float e = 1.0e-10;
			    lowp float hue = abs(values.z + (values.w - values.y) / (6.0 * delta + e));
			    lowp float lightness = 0.5 * abs(maxValue + minValue);
			    // If max == min set saturation to 1.0.
			    lowp float saturation = mix(1.0, clamp(delta / (1.0 - abs(2.0 * lightness - 1.0) + e), 0.0, 1.0), step(e, delta));
			    return vec3(hue, saturation, lightness);
			}

			void main()
			{
			    lowp vec4 color = texture2D(tex1, v_uv);
			    lowp vec3 hslColor = RgbToHsl(color.rgb);
			    hslColor.x += u_hsl.x;
			    hslColor.yz *= u_hsl.yz;
			    lowp vec3 rgbColor = HslToRgb(hslColor);
			    gl_FragColor = vec4(rgbColor, color.a);
			}";

		private static ShaderProgram shaderProgramPass;

		public int PassCount => 1;

		public Vector3 HSL;

		public HSLMaterial()
		{
			HSL = new Vector3(0, 1, 1);

			if (shaderParams == null) {
				shaderParams = new ShaderParams();
				shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };

				shaderHueParamKey = shaderParams.GetParamKey<Vector3>("u_hsl");
			}
		}

		public IMaterial Clone()
		{
			return (IMaterial)MemberwiseClone();
		}

		public void Apply(int pass)
		{
			if (shaderProgramPass == null) {
				shaderProgramPass = new ShaderProgram(
					new Shader[] { new VertexShader(vs), new FragmentShader(fs) },
					ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers());
			}
			PlatformRenderer.SetBlendState(Blending.None.GetBlendState());
			PlatformRenderer.SetShaderProgram(shaderProgramPass);

			shaderParams.Set(shaderHueParamKey, HSL);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public void Invalidate() { }

		private static ShaderParams[] shaderParamsArray;
		private static ShaderParams shaderParams;
		private static ShaderParamKey<Vector3> shaderHueParamKey;
	}
}

using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[MutuallyExclusiveDerivedComponents]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class HslComponent : NodeComponent
	{
		private Vector3 hsl;
		
		[YuzuMember]
		public float Hue
		{
			get => hsl.X * 360.0f;
			set {
				hsl.X = value / 360.0f;
				SetHsl(hsl);
			}
		}

		[YuzuMember]
		public float Saturation
		{
			get => hsl.Y * 100.0f;
			set {
				hsl.Y = value / 100.0f;
				SetHsl(hsl);
			}
		}

		[YuzuMember]
		public float Lightness
		{
			get => hsl.Z * 100.0f;
			set {
				hsl.Z = value / 100.0f;
				SetHsl(hsl);
			}
		}

		public HslComponent()
		{
			hsl = new Vector3(0, 1, 1);
		}

		public override NodeComponent Clone()
		{
			var clone = (HslComponent)base.Clone();
			clone.Owner = null;
			return clone;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				((Image)oldOwner).CustomMaterial = null;
			}
			if (Owner != null) {
				((Image)Owner).CustomMaterial = new HslShiftMaterial();
			}
		}

		public void SetHsl(Vector3 hue)
		{
			if (Owner != null) {
				var m = ((Image)Owner).CustomMaterial;
				((HslShiftMaterial)m).SetHsl(hue);
			}
		}

		private class HslShiftMaterial : IMaterial
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

				const float inversSix = 1.0 / 6.0;

				vec3 HSLtoRGB(vec3 c)
				{
					vec3 rgb = clamp(abs(mod(c.x * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
					return c.z + c.y * (rgb - 0.5) * (1.0 - abs(2.0 * c.z - 1.0));
				}

				vec3 RGBtoHSL(vec3 c)
				{
					float h = 0.0;
					float s = 0.0;
					float l = 0.0;

					float cMin = min(c.r, min(c.g, c.b));
					float cMax = max(c.r, max(c.g, c.b));

					l = (cMax + cMin) * 0.5;

					float cDelta = cMax - cMin;
					s = mix(cDelta / (cMax + cMin), cDelta / (2.0 - (cMax + cMin)), step(0.5, l));

					float inverseDelta = 1.0 / cDelta;
					h += mix(0.0, (c.g - c.b) * inverseDelta, step(cMax, c.r));
					h += mix(0.0, 2.0 + (c.b - c.r) * inverseDelta, step(cMax, c.g)) * (1.0 - step(c.g, c.r));
					h += mix(0.0, 4.0 + (c.r - c.g) * inverseDelta, step(cMax, c.b)) * (1.0 - step(c.b, c.r)) * (1.0 - step(c.b, c.g));

					h = mix(h + 6.0, h, step(0.0, h));
					h = h * inversSix;

					h = mix(h, 0.0, step(cMax, cMin));
					return vec3(h, s, l);
				}

				void main()
				{
					vec4 color = texture2D(tex1, v_uv);
					vec3 fragHSL = RGBtoHSL(color.rgb);
					fragHSL.x += u_hsl.x;
					fragHSL.yz *= u_hsl.yz;
					fragHSL.xyz = fragHSL.xyz;
					vec3 fragRGB = HSLtoRGB(fragHSL);
					gl_FragColor = vec4(fragRGB, color.a);
				}";

			private static ShaderProgram shaderProgramPass;

			public int PassCount => 1;

			private Vector3 hsl;

			public HslShiftMaterial()
			{
				hsl = new Vector3(0, 1, 1);

				if (shaderParams == null) {
					shaderParams = new ShaderParams();
					shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };

					shaderHueParamKey = shaderParams.GetParamKey<Vector3>("u_hsl");
				}
			}

			public IMaterial Clone()
			{
				return new HslShiftMaterial();
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

				shaderParams.Set(shaderHueParamKey, hsl);
				PlatformRenderer.SetShaderParams(shaderParamsArray);
			}

			public void Invalidate() { }

			private static ShaderParams[] shaderParamsArray;
			private static ShaderParams shaderParams;
			private static ShaderParamKey<Vector3> shaderHueParamKey;

			public void SetHsl(Vector3 hsl)
			{
				this.hsl = hsl;
			}
		}
	}
}

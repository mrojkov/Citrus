using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedOwnerTypes(typeof(Image))]
	class HslComponent : AwakeBehavior
	{
		private IMaterial oldCustomMaterial;

		private Vector3 hsl;

		[TangerineInspect]
		public float Hue
		{
			get => hsl.X;
			set {
				hsl.X = value;
				SetHsl(hsl);
			}
		}

		[TangerineInspect]
		public float Saturation
		{
			get => hsl.Y;
			set {
				hsl.Y = value;
				SetHsl(hsl);
			}
		}

		[TangerineInspect]
		public float Lightness
		{
			get => hsl.Z;
			set {
				hsl.Z = value;
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
				((Image)oldOwner).CustomMaterial = oldCustomMaterial;
			}
			if (Owner != null) {
				oldCustomMaterial = ((Image)Owner).CustomMaterial;
				SetMaterial();
			}
		}

		private void SetMaterial()
		{
			if (Owner != null) {
				((Image)Owner).CustomMaterial = HslShiftMaterial.Instance;
			}
		}

		public void SetHsl(Vector3 hue)
		{
			if (Owner != null) {
				IMaterial m = ((Image)Owner).CustomMaterial;
				((HslShiftMaterial)m).SetHsl(hue);
			}
		}
	}

	public class HslShiftMaterial : IMaterial
	{
		public static readonly HslShiftMaterial Instance = new HslShiftMaterial();

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

				vec3 HSLtoRGB(vec3 c)
				{
					vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0);
					return c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
				}

				vec3 RGBtoHSL(vec3 c)
				{
					float h = 0.0;
					float s = 0.0;
					float l = 0.0;

					float r = c.r;
					float g = c.g;
					float b = c.b;

					float cMin = min(r, min(g, b));
					float cMax = max(r, max(g, b));

					l = (cMax + cMin) / 2.0;

					float cDelta = cMax - cMin;
					s = mix(cDelta / (cMax + cMin), cDelta / (2.0 - ( cMax + cMin )), step(0.0, l));

					h += mix(0, (g - b) / cDelta, step(cMax, r));
					h += mix(0, 2.0 + (b - r) / cDelta, step(cMax, g)) * (1.0 - step(g,r));
					h += mix(0, 4.0 + (r - g) / cDelta, step(cMax, b)) * (1.0 - step(b,r)) * (1.0 - step(b,g));

					h = mix(h+6, h, step(0.0, h));
					h = h / 6.0;

					h = mix(h, 0, step(cMax, cMin));
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
					gl_FragColor = vec4(fragRGB, color.w);
				}";

		private ShaderProgram shaderProgramPass;

		public int PassCount => 2;

		private Vector3 hsl;

		private HslShiftMaterial()
		{
			hsl = new Vector3(1, 1, 1);

			shaderParams = new ShaderParams();
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };

			shaderHueParamKey = shaderParams.GetParamKey<Vector3>("u_hsl");
		}

		public IMaterial Clone() => Instance;

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

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private ShaderParamKey<Vector3> shaderHueParamKey;

		public void SetHsl(Vector3 hsl)
		{
			this.hsl = hsl;
		}
	}
}

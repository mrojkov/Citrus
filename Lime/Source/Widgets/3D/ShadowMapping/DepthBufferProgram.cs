namespace Lime
{
	internal class DepthBufferProgram : ShaderProgram
	{
		private const string VertexShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			attribute vec4 a_Position;

			varying vec4 v_Color;

			uniform mat4 u_LightWorldViewProj;

			void main()
			{
				gl_Position = u_LightWorldViewProj * a_Position;
				v_Color = gl_Position;
			}
		";

		private const string FragmentShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			varying vec4 v_Color;

			void main()
			{
				gl_FragColor = vec4(v_Color.zzz, 1.0);
			}
		";

		public int LightWorldViewProjUniformId;

		public DepthBufferProgram()
			: base(GetShaders(), GetAttribLocations(), new Sampler[0])
		{ }

		protected override void InitializeUniformIds()
		{
			LightWorldViewProjUniformId = GetUniformId("u_LightWorldViewProj");
		}

		private static Shader[] GetShaders()
		{
			return new Shader[] {
				new VertexShader(VertexShader),
				new FragmentShader(FragmentShader)
			};
		}

		private static AttribLocation[] GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = ShaderPrograms.Attributes.Pos1 }
			};
		}
	}
}

namespace Lime
{
	internal class DepthBufferProgram : ShaderProgram
	{
		private const string VertexShader = @"
			#ifdef GL_ES
			precision highp float;
			#endif

			attribute vec4 a_Position;
			attribute vec4 a_BlendIndices;
			attribute vec4 a_BlendWeights;

			varying vec4 v_Color;

			uniform mat4 u_LightWorldViewProj;
			uniform mat4 u_Bones[50];

			void main()
			{
				vec4 position = a_Position;

			#ifdef SKIN_ENABLED
				mat4 skinTransform =
					u_Bones[int(a_BlendIndices.x)] * a_BlendWeights.x +
					u_Bones[int(a_BlendIndices.y)] * a_BlendWeights.y +
					u_Bones[int(a_BlendIndices.z)] * a_BlendWeights.z +
					u_Bones[int(a_BlendIndices.w)] * a_BlendWeights.w;
				position = skinTransform * position;
			#endif

				gl_Position = u_LightWorldViewProj * position;
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

		public int BonesUniformId;
		public int LightWorldViewProjUniformId;

		public DepthBufferProgram(bool skinEnabled)
			: base(GetShaders(skinEnabled), GetAttribLocations(), new Sampler[0])
		{ }

		protected override void InitializeUniformIds()
		{
			BonesUniformId = GetUniformId("u_Bones");
			LightWorldViewProjUniformId = GetUniformId("u_LightWorldViewProj");
		}

		private static Shader[] GetShaders(bool skinEnabled)
		{
			var vertexShader = VertexShader;
			if (skinEnabled) {
				vertexShader = "#define SKIN_ENABLED\n" + vertexShader;
			}
			
			return new Shader[] {
				new VertexShader(vertexShader),
				new FragmentShader(FragmentShader)
			};
		}

		private static AttribLocation[] GetAttribLocations()
		{
			return new AttribLocation[] {
				new AttribLocation { Name = "a_Position", Index = ShaderPrograms.Attributes.Pos1 },
				new AttribLocation { Name = "a_BlendIndices", Index = ShaderPrograms.Attributes.BlendIndices },
				new AttribLocation { Name = "a_BlendWeights", Index = ShaderPrograms.Attributes.BlendWeights },
			};
		}
	}
}

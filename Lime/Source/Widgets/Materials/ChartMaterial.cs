namespace Lime
{
	public class ChartMaterial : IMaterial
	{
		private readonly ShaderParams[] shaderParamsArray;
		private readonly ShaderParamKey<Matrix44> matrixKey;
		private readonly ShaderParamKey<Vector4> colorsKeys;

		public Matrix44 Matrix = Matrix44.Identity;
		public Vector4[] Colors;

		public string Id { get; set; }
		public int PassCount => 1;

		public ChartMaterial()
		{
			shaderParamsArray = new[] { new ShaderParams(), new ShaderParams() };
			matrixKey = shaderParamsArray[0].GetParamKey<Matrix44>("wvp");
			colorsKeys = shaderParamsArray[1].GetParamKey<Vector4>("colors");
		}

		public void Apply(int pass)
		{
			PlatformRenderer.SetShaderProgram(ShaderProgram.Instance);
			shaderParamsArray[0].Set(matrixKey, Matrix);
			shaderParamsArray[1].Set(colorsKeys, Colors, Colors.Length);
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}

		public IMaterial Clone() => new ChartMaterial() { Colors = Colors };

		public void Invalidate() { }

		public class ShaderProgram : Lime.ShaderProgram
		{
			private static ShaderProgram instance;
			public static ShaderProgram Instance => instance ?? (instance = new ShaderProgram());

			public static readonly AttribLocation[] AttribLocations;
			public static readonly int[] MeshAttribLocations;

			private const string VertexShader = @"
				uniform mat4 wvp;
				attribute vec3 inPos; // z is color index
				varying lowp float colorIndex;
				void main() {
					colorIndex = inPos.z;
					gl_Position = wvp * vec4(inPos.xy, 0, 1);
				}";

			private const string FragmentShader = @"
				uniform lowp vec4 colors[13];
				varying lowp float colorIndex;
				void main() { gl_FragColor = colors[int(colorIndex)]; }";

			static ShaderProgram()
			{
				AttribLocations = new[] { new AttribLocation() { Index = 0, Name = "inPos" } };
				MeshAttribLocations = new[] { 0 };
			}

			private ShaderProgram() : base(CreateShaders(), AttribLocations, new Sampler[0]) { }

			private static Shader[] CreateShaders()
			{
				return new Shader[] {
					new VertexShader(VertexShader),
					new FragmentShader(FragmentShader)
				};
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace Lime
{
	[Flags]
	public enum ShaderOptions
	{
		None = 0,
		PremultiplyAlpha = 1,
		VertexAnimation = 2,
		CutOutTextureBlending = 4,
		Count = 3,
	}

	public class ShaderPrograms
	{
		public static class Attributes
		{
			public const int Pos1 = 0;
			public const int Color1 = 1;

			public const int UV1 = 2;
			public const int UV2 = 3;
			public const int UV3 = 4;
			public const int UV4 = 5;
			public const int BlendIndices = 6;
			public const int BlendWeights = 7;

			public const int Pos2 = 8;
			public const int Color2 = 9;
			public const int Normal = 10;

			public static IEnumerable<ShaderProgram.AttribLocation> GetLocations()
			{
				return new ShaderProgram.AttribLocation[] {
					new ShaderProgram.AttribLocation { Index = Pos1, Name = "inPos" },
					new ShaderProgram.AttribLocation { Index = Color1, Name = "inColor" },
					new ShaderProgram.AttribLocation { Index = UV1, Name = "inTexCoords1" },
					new ShaderProgram.AttribLocation { Index = UV2, Name = "inTexCoords2" },
					new ShaderProgram.AttribLocation { Index = UV3, Name = "inTexCoords3" },
					new ShaderProgram.AttribLocation { Index = UV4, Name = "inTexCoords4" },
					new ShaderProgram.AttribLocation { Index = BlendIndices, Name = "inBlendIndices" },
					new ShaderProgram.AttribLocation { Index = BlendWeights, Name = "inBlendWeights" },
					new ShaderProgram.AttribLocation { Index = Pos2, Name = "inPos2" },
					new ShaderProgram.AttribLocation { Index = Color2, Name = "inColor2" },
				};
			}
		}

		class CustomShaderProgram
		{
			private ShaderProgram[] programs;
			private string vertexShader;
			private string fragmentShader;
			private IEnumerable<ShaderProgram.AttribLocation> attribLocations;
			private IEnumerable<ShaderProgram.Sampler> samplers;

			public CustomShaderProgram(string vertexShader, string fragmentShader, IEnumerable<ShaderProgram.AttribLocation> attribLocations, IEnumerable<ShaderProgram.Sampler> samplers)
			{
				this.vertexShader = vertexShader;
				this.fragmentShader = fragmentShader;
				this.attribLocations = attribLocations;
				this.samplers = samplers;
				this.programs = new ShaderProgram[1 << (int)ShaderOptions.Count];
			}

			public ShaderProgram GetProgram(ShaderOptions options)
			{
				var program = programs[(int)options];
				if (program == null) {
					var preamble = CreateShaderPreamble(options);
					programs[(int)options] = program = new ShaderProgram(
						new Shader[] {
							new VertexShader(preamble + vertexShader),
							new FragmentShader(preamble + fragmentShader)
						},
						attribLocations, samplers);
				}
				return program;
			}

			private string CreateShaderPreamble(ShaderOptions options)
			{
				string result = "";
				int bit = 1;
				while (options != ShaderOptions.None) {
					if (((int)options & 1) == 1) {
						var name = Enum.GetName(typeof(ShaderOptions), (ShaderOptions)bit);
						result += "#define " + name + "\n";
					}
					bit <<= 1;
					options = (ShaderOptions)((int)options >> 1);
				}
				return result;
			}
		}

		public static ShaderPrograms Instance = new ShaderPrograms();

		private ShaderPrograms()
		{
			colorOnlyBlendingProgram = CreateShaderProgram(oneTextureVertexShader, colorOnlyFragmentShader);
			oneTextureBlengingProgram = CreateShaderProgram(oneTextureVertexShader, oneTextureFragmentShader);
			twoTexturesBlengingProgram = CreateShaderProgram(twoTexturesVertexShader, twoTexturesFragmentShader);
			silhuetteBlendingProgram = CreateShaderProgram(oneTextureVertexShader, silhouetteFragmentShader);
			twoTexturesSilhuetteBlendingProgram = CreateShaderProgram(twoTexturesVertexShader, twoTexturesSilhouetteFragmentShader);
			inversedSilhuetteBlendingProgram = CreateShaderProgram(oneTextureVertexShader, inversedSilhouetteFragmentShader);
		}

		public ShaderProgram GetShaderProgram(ShaderId shader, ITexture texture1, ITexture texture2, ShaderOptions options)
		{
			int numTextures = texture2 != null ? 2 : (texture1 != null ? 1 : 0);
			return GetShaderProgram(shader, numTextures, options);
		}

		public ShaderProgram GetShaderProgram(ShaderId shader, int numTextures, ShaderOptions options)
		{
			return GetShaderProgram(shader, numTextures).GetProgram(options);
		}

		private CustomShaderProgram GetShaderProgram(ShaderId shader, int numTextures)
		{
			if (shader == ShaderId.Diffuse || shader == ShaderId.Inherited) {
				if (numTextures == 1) {
					return oneTextureBlengingProgram;
				} else if (numTextures == 2) {
					return twoTexturesBlengingProgram;
				}
			} else if (shader == ShaderId.Silhuette) {
				if (numTextures == 1) {
					return silhuetteBlendingProgram;
				} else if (numTextures == 2) {
					return twoTexturesSilhuetteBlendingProgram;
				}
			} else if (shader == ShaderId.InversedSilhuette && numTextures == 1) {
				return inversedSilhuetteBlendingProgram;
			}
			return colorOnlyBlendingProgram;
		}

		readonly string oneTextureVertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform mat4 matProjection;
			uniform mat4 globalTransform;
			uniform vec4 globalColor;
			uniform highp float morphKoeff;
			void main()
			{
				#ifdef VertexAnimation
					gl_Position = matProjection * (globalTransform * vec4((1.0 - morphKoeff) * inPos + morphKoeff * inPos2));
					color = ((1.0 - morphKoeff) * inColor + morphKoeff * inColor2) * globalColor;
				#else
					gl_Position = matProjection * inPos;
					color = inColor;
				#endif
				texCoords = inTexCoords1;
			}";

		readonly string twoTexturesVertexShader = @"
			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform mat4 matProjection;
			uniform mat4 globalTransform;
			uniform vec4 globalColor;
			uniform highp float morphKoeff;
			void main()
			{
				#ifdef VertexAnimation
					gl_Position = matProjection * (globalTransform * vec4((1.0 - morphKoeff) * inPos + morphKoeff * inPos2));
					color = ((1.0 - morphKoeff) * inColor + morphKoeff * inColor2) * globalColor;
				#else
					gl_Position = matProjection * inPos;
					color = inColor;
				#endif
				texCoords1 = inTexCoords1;
				texCoords2 = inTexCoords2;
			}";

		readonly string colorOnlyFragmentShader = @"
			varying lowp vec4 color;
			void main()
			{
				gl_FragColor = color;
			}";

		readonly string oneTextureFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform lowp sampler2D tex1;
			void main()
			{
				gl_FragColor = color * texture2D(tex1, texCoords);
				#ifdef PremultiplyAlpha
					gl_FragColor.rgb *= gl_FragColor.a;
				#endif
			}";

		readonly string twoTexturesFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			void main()
			{
				#ifdef CutOutTextureBlending
					gl_FragColor = color * (texture2D(tex1, texCoords1) - vec4(0.0, 0.0, 0.0, texture2D(tex2, texCoords2).a));
				#else
					gl_FragColor = color * texture2D(tex1, texCoords1) * texture2D(tex2, texCoords2);
				#endif
				#ifdef PremultiplyAlpha
					gl_FragColor.rgb *= gl_FragColor.a;
				#endif
			}";

		readonly string silhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform lowp sampler2D tex1;
			void main()
			{
				gl_FragColor = color * vec4(1.0, 1.0, 1.0, texture2D(tex1, texCoords).a);
			}";

		readonly string twoTexturesSilhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			void main()
			{
				#ifdef CutOutTextureBlending
					gl_FragColor = color * (vec4(1.0, 1.0, 1.0, texture2D(tex1, texCoords1).a - texture2D(tex2, texCoords2).a));
				#else
					gl_FragColor = color * texture2D(tex1, texCoords1) * vec4(1.0, 1.0, 1.0, texture2D(tex2, texCoords2).a);
				#endif
			}";

		readonly string inversedSilhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform lowp sampler2D tex1;
			void main()
			{
				gl_FragColor = color * vec4(1.0, 1.0, 1.0, 1.0 - texture2D(tex1, texCoords).a);
			}";

		private readonly CustomShaderProgram colorOnlyBlendingProgram;
		private readonly CustomShaderProgram oneTextureBlengingProgram;
		private readonly CustomShaderProgram twoTexturesBlengingProgram;
		private readonly CustomShaderProgram silhuetteBlendingProgram;
		private readonly CustomShaderProgram twoTexturesSilhuetteBlendingProgram;
		private readonly CustomShaderProgram inversedSilhuetteBlendingProgram;

		private static CustomShaderProgram CreateShaderProgram(string vertexShader, string fragmentShader)
		{
			return new CustomShaderProgram(vertexShader, fragmentShader, Attributes.GetLocations(), GetSamplers());
		}

		public static IEnumerable<ShaderProgram.Sampler> GetSamplers()
		{
			return new ShaderProgram.Sampler[] {
				new ShaderProgram.Sampler { Name = "tex1", Stage = 0 },
				new ShaderProgram.Sampler { Name = "tex2", Stage = 1 },
			};
		}

		/// <summary>
		/// Colorizes <see cref="SimpleText"/>'s or <see cref="RichText"/>'s grayscale font.
		/// </summary>
		/// <remarks>
		/// To apply this shader you need:
		/// 1. Data/Fonts/GradientMap.png file with size of 256x256 and ARGB8 compression.
		/// 2. Use <see cref="Node.Tag"/> field of <see cref="SimpleText"/> or <see cref="TextStyle"/>
		///    to specify color row from this file.
		/// </remarks>
		public class ColorfulTextShaderProgram : ShaderProgram
		{
			private static string vertexShaderText = @"
			attribute vec4 inPos;
			attribute vec4 inPos2;
			attribute vec4 inColor;
			attribute vec4 inColor2;
			attribute vec2 inTexCoords1;
			attribute vec2 inTexCoords2;
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform mat4 matProjection;
			uniform mat4 globalTransform;
			uniform vec4 globalColor;
			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;
				texCoords1 = inTexCoords1;
				texCoords2 = inTexCoords2;
			}";

			private static string fragmentShaderText = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			uniform lowp float colorIndex;
			void main()
			{
				lowp vec4 t1 = texture2D(tex1, texCoords1);
				lowp vec4 t2 = texture2D(tex2, vec2(t1.x, colorIndex));
				gl_FragColor = color * vec4(t2.rgb, t1.a * t2.a);
			}";

			private static ColorfulTextShaderProgram instance;

			public static ColorfulTextShaderProgram GetInstance()
			{
				if (instance == null) {
					instance = new ColorfulTextShaderProgram();
				}
				return instance;
			}

			public ColorfulTextShaderProgram()
				: base(GetShaders(), Attributes.GetLocations(), GetSamplers())
			{ }

			private static IEnumerable<Shader> GetShaders()
			{
				return new Shader[] {
					new VertexShader(vertexShaderText),
					new FragmentShader(fragmentShaderText)
				};
			}

			public const int GradientMapTextureSize = 256;

			public static float StyleIndexToColorIndex(int styleIndex)
			{
				return (styleIndex * 2 + 1) / (GradientMapTextureSize * 2.0f);
			}

			private static ITexture fontGradientTexture;

			public static ITexture GradientRampTexture => fontGradientTexture = fontGradientTexture ?? new SerializableTexture("Fonts/GradientMap");
		}

		public class DashedLineShaderProgram : ShaderProgram
		{
			private const string vertexShaderText = @"
				attribute vec4 inPos;
				attribute vec4 inColor;
				attribute vec2 inTexCoords1;
				attribute vec2 inTexCoords2;
				varying lowp vec4 color;
				varying lowp vec2 texCoords1;
				varying lowp vec2 texCoords2;
				uniform mat4 matProjection;
				void main()
				{
					gl_Position = matProjection * inPos;
					color = inColor;
					texCoords1 = inTexCoords1;
					texCoords2 = inTexCoords2;
				}";

			private const string fragmentShaderText = @"
				varying lowp vec4 color;
				varying lowp vec2 texCoords1;
				varying lowp vec2 texCoords2;
				uniform lowp sampler2D tex1;
				void main()
				{
					int segment = int(texCoords1.x);
					gl_FragColor = color;
					gl_FragColor.a *= 1.0 - mod(float(segment), 2.0);
					
					lowp float u = texCoords1.x - float(segment);
					lowp float v = texCoords1.y;
					
					if (u < texCoords2.x)
						gl_FragColor.a *= u / texCoords2.x;
					else if (u > 1.0 - texCoords2.x)
						gl_FragColor.a *= (1.0 - u) / texCoords2.x;

					if (v < texCoords2.y)
						gl_FragColor.a *= v / texCoords2.y;
					else if (v > 1.0 - texCoords2.y)
						gl_FragColor.a *= (1.0 - v) / texCoords2.y;
				}";

			private static DashedLineShaderProgram instance;

			private DashedLineShaderProgram()
				: base(GetShaders(), Attributes.GetLocations(), GetSamplers())
			{ }

			private static IEnumerable<Shader> GetShaders()
			{
				return new Shader[] {
					new VertexShader(vertexShaderText),
					new FragmentShader(fragmentShaderText)
				};
			}

			public static DashedLineShaderProgram GetInstance()
			{
				if (instance == null) {
					instance = new DashedLineShaderProgram();
				}
				return instance;
			}
		}
	}
}

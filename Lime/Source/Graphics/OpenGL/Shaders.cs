#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	[Flags]
	public enum ShaderFlags
	{
		None = 0,
		UseAlphaTexture1 = 1,
		UseAlphaTexture2 = 2,
		PremultiplyAlpha = 4,
		Count = 3
	}

	public class ShaderPrograms
	{
		class MultiShaderProgram
		{
			private ShaderProgram[] programs;

			public MultiShaderProgram(string vertexShader, string fragmentShader, IEnumerable<ShaderProgram.AttribLocation> attribLocations, IEnumerable<ShaderProgram.Sampler> samplers, ShaderFlags mask)
			{
				var maxEntries = 1 << (int)ShaderFlags.Count;
				programs = new ShaderProgram[maxEntries];
				for (int i = 0; i < maxEntries; i++) {
					if ((i & (int)mask) == i) {
						programs[i] = new ShaderProgram(
							new Shader[] { 
								new VertexShader(vertexShader), 
								new FragmentShader(AddHeaderWithDefines((ShaderFlags)i, fragmentShader))
							},
							attribLocations, samplers);
					}
				}
			}

			private string AddHeaderWithDefines(ShaderFlags shaderFlags, string shader)
			{
				for (int i = 0; i < (int)ShaderFlags.Count; i++) {
					int bit = 1 << i;
					if (((int)shaderFlags & bit) != 0) {
						var name = Enum.GetName(typeof(ShaderFlags), (ShaderFlags)bit);
						shader = "#define " + name + "\n" + shader;
					}
				}
				return shader;
			}

			public ShaderProgram GetProgram(ShaderFlags flags)
			{
				return programs[(int)flags];
			}
		}

		public static ShaderPrograms Instance = new ShaderPrograms();

		private ShaderPrograms()
		{
			colorOnlyBlendingProgram = CreateShaderProgram(oneTextureVertexShader, colorOnlyFragmentShader, ShaderFlags.None);
			oneTextureBlengingProgram = CreateShaderProgram(oneTextureVertexShader, oneTextureFragmentShader,
				ShaderFlags.UseAlphaTexture1 | ShaderFlags.PremultiplyAlpha);
			twoTexturesBlengingProgram = CreateShaderProgram(twoTexturesVertexShader, twoTexturesFragmentShader,
				ShaderFlags.UseAlphaTexture1 | ShaderFlags.UseAlphaTexture2 | ShaderFlags.PremultiplyAlpha);
			silhuetteBlendingProgram = CreateShaderProgram(oneTextureVertexShader, silhouetteFragmentShader,
				ShaderFlags.UseAlphaTexture1);
			twoTexturesSilhuetteBlendingProgram = CreateShaderProgram(twoTexturesVertexShader, twoTexturesSilhouetteFragmentShader, ShaderFlags.UseAlphaTexture1 | ShaderFlags.UseAlphaTexture2);
			inversedSilhuetteBlendingProgram = CreateShaderProgram(oneTextureVertexShader, inversedSilhouetteFragmentShader, ShaderFlags.UseAlphaTexture1);
		}

		public ShaderProgram GetShaderProgram(ShaderId shader, int numTextures, ShaderFlags flags)
		{
			return GetShaderProgram(shader, numTextures).GetProgram(flags);
		}

		private MultiShaderProgram GetShaderProgram(ShaderId shader, int numTextures)
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
			attribute vec4 inColor;
			attribute vec2 inTexCoords1;
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform mat4 matProjection;
			void main()
			{
				gl_Position = matProjection * inPos;
				color = inColor;					
				texCoords = inTexCoords1;
			}";

		readonly string twoTexturesVertexShader = @"
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
			uniform lowp sampler2D tex1a;
			void main()
			{
				lowp vec4 t1 = texture2D(tex1, texCoords);
				$ifdef UseAlphaTexture1
					t1.a = texture2D(tex1a, texCoords).r;
				$endif
				gl_FragColor = color * t1;
				$ifdef PremultiplyAlpha
					gl_FragColor.rgb *= gl_FragColor.a;
				$endif
			}";

		readonly string twoTexturesFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex2;
			uniform lowp sampler2D tex1a;
			uniform lowp sampler2D tex2a;
			void main()
			{
				lowp vec4 t1 = texture2D(tex1, texCoords1);
				lowp vec4 t2 = texture2D(tex2, texCoords2);
				$ifdef UseAlphaTexture1
					t1.a = texture2D(tex1a, texCoords1).r;
				$endif
				$ifdef UseAlphaTexture2
					t2.a = texture2D(tex2a, texCoords2).r;
				$endif
				gl_FragColor = color * t1 * t2;
				$ifdef PremultiplyAlpha
					gl_FragColor.rgb *= gl_FragColor.a;
				$endif
			}";

		readonly string silhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex1a;
			void main()
			{
				$ifdef UseAlphaTexture1
					lowp float a = texture2D(tex1a, texCoords).r;
				$else
					lowp float a = texture2D(tex1, texCoords).a;
				$endif
				gl_FragColor = color * vec4(1.0, 1.0, 1.0, a);
			}";

		readonly string twoTexturesSilhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords1;
			varying lowp vec2 texCoords2;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex1a;
			uniform lowp sampler2D tex2;
			uniform lowp sampler2D tex2a;
			void main()
			{
				lowp vec4 t1 = texture2D(tex1, texCoords1);
				$ifdef UseAlphaTexture1
					t1.a = texture2D(tex1a, texCoords1).r;
				$endif
				$ifdef UseAlphaTexture2
					lowp float a2 = texture2D(tex2a, texCoords2).r;
				$else
					lowp float a2 = texture2D(tex2, texCoords2).a;
				$endif
				gl_FragColor = t1 * color * vec4(1.0, 1.0, 1.0, a2);
			}";

		readonly string inversedSilhouetteFragmentShader = @"
			varying lowp vec4 color;
			varying lowp vec2 texCoords;
			uniform lowp sampler2D tex1;
			uniform lowp sampler2D tex1a;
			void main()
			{
				$ifdef UseAlphaTexture1
					lowp float a = 1.0 - texture2D(tex1a, texCoords).r;
				$else
					lowp float a = 1.0 - texture2D(tex1, texCoords).a;
				$endif
				gl_FragColor = color * vec4(1.0, 1.0, 1.0, a);
			}";

		private readonly MultiShaderProgram colorOnlyBlendingProgram;
		private readonly MultiShaderProgram oneTextureBlengingProgram;
		private readonly MultiShaderProgram twoTexturesBlengingProgram;
		private readonly MultiShaderProgram silhuetteBlendingProgram;
		private readonly MultiShaderProgram twoTexturesSilhuetteBlendingProgram;
		private readonly MultiShaderProgram inversedSilhuetteBlendingProgram;

		private static MultiShaderProgram CreateShaderProgram(string vertexShader, string fragmentShader, ShaderFlags mask)
		{
			// #ifdef - breaks Unity3D compiler
			fragmentShader = fragmentShader.Replace('$', '#');
			return new MultiShaderProgram(
				vertexShader, fragmentShader, PlatformGeometryBuffer.Attributes.GetLocations(), 
				GetSamplers(), mask);
		}

		public static IEnumerable<ShaderProgram.Sampler> GetSamplers()
		{
			return new ShaderProgram.Sampler[] {
				new ShaderProgram.Sampler { Name = "tex1", Stage = 0 },
				new ShaderProgram.Sampler { Name = "tex2", Stage = 1 },
				new ShaderProgram.Sampler { Name = "tex1a", Stage = 2 },
				new ShaderProgram.Sampler { Name = "tex2a", Stage = 3 }
			};
		}
	}
}
#endif
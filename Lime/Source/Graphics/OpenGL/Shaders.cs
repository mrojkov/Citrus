#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	public class ShaderPrograms
	{
		public static ShaderPrograms Instance = new ShaderPrograms();

		private ShaderPrograms()
		{
			colorOnlyBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, colorOnlyFragmentShader);
			oneTextureBlengingProgram = CreateBlendingProgram(oneTextureVertexShader, oneTextureFragmentShader);
			twoTexturesBlengingProgram = CreateBlendingProgram(twoTexturesVertexShader, twoTexturesFragmentShader);
			silhuetteBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, silhouetteFragmentShader);
			twoTexturesSilhuetteBlendingProgram = CreateBlendingProgram(twoTexturesVertexShader, twoTexturesSilhouetteFragmentShader);
			inversedSilhuetteBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, inversedSilhouetteFragmentShader);
		}

		public ShaderProgram GetShaderProgram(ShaderId shader, int numTextures)
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

		readonly Shader oneTextureVertexShader = new VertexShader(
			"attribute vec4 inPos;			" +	
			"attribute vec4 inColor;		" +
			"attribute vec2 inTexCoords1;	" + 
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform mat4 matProjection;	" +
			"void main()					" +
			"{											" +
			"	gl_Position = matProjection * inPos;	" +
			"	color = inColor;						" +
			"	texCoords = inTexCoords1;				" +
			"}"
		);

		readonly Shader twoTexturesVertexShader = new VertexShader(
			"attribute vec4 inPos;			" +
			"attribute vec4 inColor;		" +
			"attribute vec2 inTexCoords1;	" +
			"attribute vec2 inTexCoords2;	" +
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;	" +
			"varying lowp vec2 texCoords2;	" +
			"uniform mat4 matProjection;	" +
			"void main()					" +
			"{											" +
			"	gl_Position = matProjection * inPos;	" +
			"	color = inColor;						" +
			"	texCoords1 = inTexCoords1;				" +
			"	texCoords2 = inTexCoords2;				" +
			"}"
		);

		readonly Shader colorOnlyFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = color;		" +
			"}"
		);

		readonly Shader oneTextureFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform lowp sampler2D tex1;	" +
			"uniform bool useAlphaTexture1;	" +
			"uniform lowp sampler2D tex1a;	" +
			"void main()					" +
			"{								" +
			"	lowp vec4 t1 = texture2D(tex1, texCoords); " +
			"	if (useAlphaTexture1)			" +
			"		t1.a = texture2D(tex1a, texCoords).r; " +
			"	gl_FragColor = color * t1;	" +
			"}"
		);

		readonly Shader twoTexturesFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;	" +
			"varying lowp vec2 texCoords2;	" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex2;	" +
			"uniform bool useAlphaTexture1;	" +
			"uniform bool useAlphaTexture2;	" +
			"uniform lowp sampler2D tex1a;	" +
			"uniform lowp sampler2D tex2a;	" +
			"void main()					" +
			"{								" +
			"	lowp vec4 t1 = texture2D(tex1, texCoords1);	" +
			"	lowp vec4 t2 = texture2D(tex2, texCoords2);	" +
			"	if (useAlphaTexture1)			" +
			"		t1.a = texture2D(tex1a, texCoords1).r; " +
			"	if (useAlphaTexture2)			" +
			"		t2.a = texture2D(tex2a, texCoords2).r; " +
			"	gl_FragColor = color * t1 * t2; " +
			"}"
		);

		readonly Shader silhouetteFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex1a;	" +
			"uniform bool useAlphaTexture1;	" +
			"void main()					" +
			"{								" +
			"	lowp float a = useAlphaTexture1 ? texture2D(tex1a, texCoords).r : texture2D(tex1, texCoords).a; " +
			"	gl_FragColor = color * vec4(a, a, a, a);	" +
			"}"
		);

		readonly Shader twoTexturesSilhouetteFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;	" +
			"varying lowp vec2 texCoords2;	" +
			"uniform bool useAlphaTexture1;	" +
			"uniform bool useAlphaTexture2;	" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex1a;	" +
			"uniform lowp sampler2D tex2;	" +
			"uniform lowp sampler2D tex2a;	" +
			"void main()					" +
			"{								" +
			"	lowp vec4 t1 = texture2D(tex1, texCoords1);	" +
			"	if (useAlphaTexture1)			" +
			"		t1.a = texture2D(tex1a, texCoords1).r; " +
			"	lowp float a2 = useAlphaTexture2 ? texture2D(tex2a, texCoords2).r : texture2D(tex2, texCoords2).a; " +
			"	gl_FragColor = t1 * color * vec4(a2, a2, a2, a2);" +
			"}"
		);

		readonly Shader inversedSilhouetteFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform bool useAlphaTexture1;	" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex1a;	" +
			"void main()					" +
			"{								" +
			"	lowp float a = 1.0 - (useAlphaTexture1 ? texture2D(tex1a, texCoords).r : texture2D(tex1, texCoords).a); " +
			"	gl_FragColor = color * vec4(a, a, a, a); " +
			"}"
		);

		private readonly ShaderProgram colorOnlyBlendingProgram;
		private readonly ShaderProgram oneTextureBlengingProgram;
		private readonly ShaderProgram twoTexturesBlengingProgram;
		private readonly ShaderProgram silhuetteBlendingProgram;
		private readonly ShaderProgram twoTexturesSilhuetteBlendingProgram;
		private readonly ShaderProgram inversedSilhuetteBlendingProgram;

		private static ShaderProgram CreateBlendingProgram(Shader vertexShader, Shader fragmentShader)
		{
			var p = new ShaderProgram(
				new Shader[] { vertexShader, fragmentShader }, 
				PlatformMesh.Attributes.GetLocations(),
				GetSamplers()
			);
			return p;
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
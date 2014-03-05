using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	static class ShaderPrograms
	{
		public static ShaderProgram GetShaderProgram(Blending blending, int numTextures)
		{
			if (blending == Blending.Silhuette) {
				return silhuetteBlendingProgram;
			}
			if (numTextures == 0) {
				return colorOnlyBlendingProgram;
			} else if (numTextures == 1) {
				return oneTextureBlengingProgram;
			} else {
				return twoTexturesBlengingProgram;
			}
		}

		static readonly Shader oneTextureVertexShader = new VertexShader(
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

		static readonly Shader twoTexturesVertexShader = new VertexShader(
			"attribute vec4 inPos;			" +
			"attribute vec4 inColor;		" +
			"attribute vec2 inTexCoords1;	" +
			"attribute vec2 inTexCoords2;	" +
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;" +
			"varying lowp vec2 texCoords2;" +
			"uniform mat4 matProjection;	" +
			"void main()					" +
			"{											" +
			"	gl_Position = matProjection * inPos;	" +
			"	color = inColor;						" +
			"	texCoords1 = inTexCoords1;				" +
			"	texCoords2 = inTexCoords2;				" +
			"}"
		);

		static readonly Shader colorOnlyFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = color;		" +
			"}"
		);

		static readonly Shader oneTextureFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform lowp sampler2D tex1;	" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = color * texture2D(tex1, texCoords); " +
			"}"
		);

		static readonly Shader twoTexturesFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;" +
			"varying lowp vec2 texCoords2;" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex2;	" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = color * texture2D(tex1, texCoords1) * texture2D(tex2, texCoords2); " +
			"}"
		);

		static readonly Shader silhouetteFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords;	" +
			"uniform lowp sampler2D tex;	" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = vec4(color.rgb, color.a * texture2D(tex, texCoords).a);	" +
			"}");

		private static readonly ShaderProgram colorOnlyBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, colorOnlyFragmentShader);
		private static readonly ShaderProgram oneTextureBlengingProgram = CreateBlendingProgram(oneTextureVertexShader, oneTextureFragmentShader);
		private static readonly ShaderProgram twoTexturesBlengingProgram = CreateBlendingProgram(twoTexturesVertexShader, twoTexturesFragmentShader);
		private static readonly ShaderProgram silhuetteBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, silhouetteFragmentShader);

		private static ShaderProgram CreateBlendingProgram(Shader vertexShader, Shader fragmentShader)
		{
			var p = new ShaderProgram();
			p.AttachShader(vertexShader);
			p.AttachShader(fragmentShader);
			p.BindAttribLocation(Renderer.Attributes.Position, "inPos");
			p.BindAttribLocation(Renderer.Attributes.UV1, "inTexCoords1");
			p.BindAttribLocation(Renderer.Attributes.UV2, "inTexCoords2");
			p.BindAttribLocation(Renderer.Attributes.Color, "inColor");
			p.Link();
			p.BindSampler("tex1", 0);
			p.BindSampler("tex2", 1);
			return p;
		}
	}
}

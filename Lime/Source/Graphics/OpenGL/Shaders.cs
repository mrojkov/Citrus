using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum ShaderId
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		Inherited,
		[ProtoEnum]
		Default,
		[ProtoEnum]
		Silhuette
	}

	static class ShaderPrograms
	{
		public static ShaderProgram GetShaderProgram(ShaderId shader, int numTextures)
		{
			if (shader == ShaderId.Default || shader == ShaderId.Inherited) {
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
			}
			return colorOnlyBlendingProgram;
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
			"	lowp float a = color.a * texture2D(tex, texCoords).a; " +
			"	gl_FragColor = vec4(color.rgb * a, a);	" +
			"}");

		static readonly Shader twoTexturesSilhouetteFragmentShader = new FragmentShader(
			"varying lowp vec4 color;		" +
			"varying lowp vec2 texCoords1;" +
			"varying lowp vec2 texCoords2;" +
			"uniform lowp sampler2D tex1;	" +
			"uniform lowp sampler2D tex2;	" +
			"void main()					" +
			"{								" +
			"	gl_FragColor = texture2D(tex1, texCoords1) * vec4(color.rgb, color.a * texture2D(tex2, texCoords2).a);" +
			"}"
		);

		private static readonly ShaderProgram colorOnlyBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, colorOnlyFragmentShader);
		private static readonly ShaderProgram oneTextureBlengingProgram = CreateBlendingProgram(oneTextureVertexShader, oneTextureFragmentShader);
		private static readonly ShaderProgram twoTexturesBlengingProgram = CreateBlendingProgram(twoTexturesVertexShader, twoTexturesFragmentShader);
		private static readonly ShaderProgram silhuetteBlendingProgram = CreateBlendingProgram(oneTextureVertexShader, silhouetteFragmentShader);
		private static readonly ShaderProgram twoTexturesSilhuetteBlendingProgram = CreateBlendingProgram(twoTexturesVertexShader, twoTexturesSilhouetteFragmentShader);

		private static ShaderProgram CreateBlendingProgram(Shader vertexShader, Shader fragmentShader)
		{
			var p = new ShaderProgram();
			p.AttachShader(vertexShader);
			p.AttachShader(fragmentShader);
			VertexBuffer.Attributes.BindLocations(p);
			p.Link();
			p.BindSampler("tex1", 0);
			p.BindSampler("tex2", 1);
			return p;
		}
	}
}

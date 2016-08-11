using System.IO;
using Lime;
using System.Net;
using System.Collections.Generic;

namespace Orange
{
	public partial class HotFontImporter
	{
		HotLexer lexer;
		Size textureSize;

		void ParseFontCharProperty(ref FontChar fontChar, string name)
		{
			switch(name) {
			case "CharCode":
				fontChar.Char = (char)lexer.ParseInt();
				break;
			case "TexPosition":
				fontChar.UV0 = lexer.ParseVector2() / (Vector2)textureSize;
				break;
			case "TexSize":
				Vector2 size = lexer.ParseVector2();
				fontChar.Width = size.X;
				fontChar.Height = size.Y;
				fontChar.UV1 = fontChar.UV0 + size / (Vector2)textureSize;
				break;
			case "ACWidths":
				fontChar.ACWidths = lexer.ParseVector2();
				break;
			case "TextureIndex":
				fontChar.TextureIndex = lexer.ParseInt();
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, fontChar);
			}
		}

		FontChar ParseFontChar()
		{
			string type = lexer.ParseQuotedString();
			if (type != "Hot::FontChar")
				throw new Exception("Unknown type of object '{0}'", type);
			var fontChar = new FontChar();
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseFontCharProperty(ref fontChar, lexer.ParseWord());
			lexer.ParseToken('}');
			return fontChar;
		}

		void ParseFontCharPairProperty(ref FontPair fontCharPair, string name)
		{
			switch(name) {
			case "CharCodeL":
				fontCharPair.A = (char)lexer.ParseInt();
				break;
			case "CharCodeR":
				fontCharPair.B = (char)lexer.ParseInt();
				break;
			case "Delta":
				fontCharPair.Kerning = lexer.ParseFloat();
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, fontCharPair);
			}
		}

		struct FontPair
		{
			public char A;
			public char B;
			public float Kerning;
		}

		FontPair ParseFontCharPair()
		{
			string type = lexer.ParseQuotedString();
			if (type != "Hot::FontCharPair")
				throw new Exception("Unknown type of object '{0}'", type);
			var fontCharPair = new FontPair();
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseFontCharPairProperty(ref fontCharPair, lexer.ParseWord());
			lexer.ParseToken('}');
			return fontCharPair;
		}

		void ParseFontProperty(Font font, string name)
		{
			switch(name) {
			case "Characters":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']')
					(font.Chars as ICollection<FontChar>).Add(ParseFontChar());
				lexer.ParseToken(']');
				break;
			case "Pairs":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					var pair = ParseFontCharPair();
					// conforming to FontCharCollection interface which doesn't really care
					// about height argument for this particular case of `Font`
					FontChar c = (font.Chars as FontCharCollection).Get(pair.A, 666);
					if (c.KerningPairs == null) {
						c.KerningPairs = new List<KerningPair>();
					}
					c.KerningPairs.Add(new KerningPair { Char = pair.B, Kerning = pair.Kerning });
				}
				lexer.ParseToken(']');
				break;
			case "About":
				font.About = lexer.ParseQuotedString();
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, font);
			}
		}

		public Font ParseFont(Stream stream)
		{
			var path = Serialization.GetCurrentOperation().SerializationPath;
			this.textureSize = GetTextureSize(path);
			using(var reader = new StreamReader(stream)) {
				string text = reader.ReadToEnd();
				lexer = new HotLexer(text);
			}
			var type = lexer.ParseQuotedString();
			if (type != "Hot::Font")
				throw new Exception("Unknown type of object '{0}'", type);
			var font = new Font();
			AddFontTextures(font, path);
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseFontProperty(font, lexer.ParseWord());
			lexer.ParseToken('}');
			return font;
		}

		private static void AddFontTextures(Font font, string path)
		{
			for (var i = 0; ; i++) {
				var texturePath = Path.ChangeExtension(path, null);
				var index = (i == 0) ? "" : i.ToString("00");
				var texturePng = Path.ChangeExtension(path, null) + index + ".png";
				if (!AssetsBundle.Instance.FileExists(texturePng)) {
					break;
				}
				font.Textures.Add(new SerializableTexture(texturePath + index));
			}
		}

		private static Size GetTextureSize(string srcPath)
		{
			var fontPngFile = Path.ChangeExtension(srcPath, ".png");
			Size size;
			bool hasAlpha;
			if (!GetPngFileInfo(fontPngFile, out size.Width, out size.Height, out hasAlpha)) {
				throw new Lime.Exception("Font doesn't have an appropriate png texture file");
			}
			return size;
		}

		static bool GetPngFileInfo(string path, out int width, out int height, out bool hasAlpha)
		{
			width = height = 0;
			hasAlpha = false;
			using(var stream = AssetsBundle.Instance.OpenFile(path)) {
				using(var reader = new BinaryReader(stream)) {
					byte[] sign = reader.ReadBytes(8); // PNG signature
					if (sign[1] != 'P' || sign[2] != 'N' || sign[3] != 'G')
						return false;
					reader.ReadBytes(4);
					reader.ReadBytes(4); // 'IHDR'
					width = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					height = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					reader.ReadByte(); // color depth
					int colorType = reader.ReadByte();
					hasAlpha = (colorType == 4) ||(colorType == 6);
				}
			}
			return true;
		}
	}
}

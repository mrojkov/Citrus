using System.IO;
using Lime;
using System.Collections.Generic;

namespace Orange
{
	public class HotFontDeserializer : Yuzu.Deserializer.AbstractReaderDeserializer
	{
		Stream stream;

		public HotFontDeserializer(Stream stream)
		{
			this.stream = stream;
		}

		public override object FromReaderInt()
		{
			return new Orange.HotFontImporter().ParseFont(stream);
		}

		public override object FromReaderInt(object obj)
		{
			return new Orange.HotFontImporter().ParseFont(stream);
		}

		public override T FromReaderInt<T>()
		{
			return (T)(object)new Orange.HotFontImporter().ParseFont(stream);
		}
	}

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

		public Font ParseFont(string srcPath, string dstPath)
		{
			this.textureSize = GetTextureSize(srcPath);
			this.lexer = CreateLexer(srcPath);
			var type = lexer.ParseQuotedString();
			if (type != "Hot::Font")
				throw new Exception("Unknown type of object '{0}'", type);
			var font = new Font();
			AddFontTextures(font, srcPath, dstPath);
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseFontProperty(font, lexer.ParseWord());
			lexer.ParseToken('}');
			return font;
		}

		public Font ParseFont(Stream stream, Font font = null)
		{
			var path = Serialization.GetCurrentSerializationPath();
			this.textureSize = GetTextureSize(path);
			using(var reader = new StreamReader(stream)) {
				string text = reader.ReadToEnd();
				lexer = new HotLexer("", text, true);
			}
			var type = lexer.ParseQuotedString();
			if (type != "Hot::Font")
				throw new Exception("Unknown type of object '{0}'", type);
			if (font == null) {
				font = new Font();
			}
			AddFontTextures(font, path);
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseFontProperty(font, lexer.ParseWord());
			lexer.ParseToken('}');
			return font;
		}

		private static void AddFontTextures(Font font, string srcPath, string dstPath)
		{
			for (var i = 0; ; i++) {
				var texturePath = Path.ChangeExtension(dstPath, null);
				var index = (i == 0) ? "" : i.ToString("00");
				var texturePng = Path.ChangeExtension(srcPath, null) + index + ".png";
				if (!File.Exists(texturePng)) {
					break;
				}
				font.Textures.Add(new SerializableTexture(texturePath + index));
			}
		}

		private static void AddFontTextures(Font font, string path)
		{
			for (var i = 0; ; i++) {
				var texturePath = Path.ChangeExtension(Path.GetFileName(path), null);
				var index = (i == 0) ? "" : i.ToString("00");
				var texturePng = Path.ChangeExtension(path, null) + index + ".png";
				if (!AssetBundle.Instance.FileExists(texturePng)) {
					break;
				}
				font.Textures.Add(new SerializableTexture(texturePath + index));
			}
		}

		private static HotLexer CreateLexer(string path)
		{
			using (Stream stream = new FileStream(path, FileMode.Open)) {
				using (TextReader reader = new StreamReader(stream)) {
					var text = reader.ReadToEnd();
					return new HotLexer(path, text, false);
				}
			}
		}

		private static Size GetTextureSize(string srcPath)
		{
			var fontPngFile = Path.ChangeExtension(srcPath, ".png");
			Size size;
			bool hasAlpha;
			if (!TextureConverterUtils.GetPngFileInfo(fontPngFile, out size.Width, out size.Height, out hasAlpha)) {
				throw new Lime.Exception("Font doesn't have an appropriate png texture file");
			}
			return size;
		}
	}
}

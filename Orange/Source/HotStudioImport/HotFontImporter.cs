using System.IO;
using Lime;

namespace Orange
{
	public partial class HotFontImporter
	{
		HotLexer lexer;
		Size textureSize;

		public HotFontImporter (string path)
		{
			using (Stream stream = new FileStream(path, FileMode.Open)) {
				using (TextReader reader = new StreamReader(stream)) {
					string text = reader.ReadToEnd ();
					lexer = new HotLexer (path, text);
				}
			}
		}

		void ParseFontCharProperty (ref FontChar fontChar, string name)
		{
			switch (name) {
			case "CharCode":
				fontChar.Char = (char)lexer.ParseInt ();
				break;
			case "TexPosition":
				fontChar.UV0 = lexer.ParseVector2 () / (Vector2)textureSize;
				break;
			case "TexSize":
				fontChar.Size = lexer.ParseVector2 ();
				fontChar.UV1 = fontChar.UV0 + fontChar.Size / (Vector2)textureSize;
				break;
			case "ACWidths":
				fontChar.ACWidths = lexer.ParseVector2 ();
				break;
			default:
				throw new Exception ("Unknown property '{0}'. Parsing: {1}", name, fontChar);
			}
		}

		public FontChar ParseFontChar ()
		{
			string type = lexer.ParseQuotedString ();
			if (type != "Hot::FontChar")
				throw new Exception ("Unknown type of object '{0}'", type);
			var fontChar = new FontChar ();
			lexer.ParseToken ('{');
			while (lexer.PeekChar() != '}')
				ParseFontCharProperty (ref fontChar, lexer.ParseWord ());
			lexer.ParseToken ('}');
			return fontChar;
		}

		void ParseFontCharPairProperty (ref FontPair fontCharPair, string name)
		{
			switch (name) {
			case "CharCodeL":
				fontCharPair.A = (char)lexer.ParseInt ();
				break;
			case "CharCodeR":
				fontCharPair.B = (char)lexer.ParseInt ();
				break;
			case "Delta":
				fontCharPair.Delta = lexer.ParseFloat ();
				break;
			default:
				throw new Exception ("Unknown property '{0}'. Parsing: {1}", name, fontCharPair);
			}
		}

		public FontPair ParseFontCharPair ()
		{
			string type = lexer.ParseQuotedString ();
			if (type != "Hot::FontCharPair")
				throw new Exception ("Unknown type of object '{0}'", type);
			var fontCharPair = new FontPair ();
			lexer.ParseToken ('{');
			while (lexer.PeekChar() != '}')
				ParseFontCharPairProperty (ref fontCharPair, lexer.ParseWord ());
			lexer.ParseToken ('}');
			return fontCharPair;
		}

		void ParseFontProperty (Font font, string name)
		{
			switch (name) {
			case "Characters":
				lexer.ParseToken ('[');
				while (lexer.PeekChar() != ']')
					font.Chars.Add (ParseFontChar ());
				lexer.ParseToken (']');
				break;
			case "Pairs":
				lexer.ParseToken ('[');
				while (lexer.PeekChar() != ']')
					font.Pairs.Add (ParseFontCharPair ());
				lexer.ParseToken (']');
				break;
			default:
				throw new Exception ("Unknown property '{0}'. Parsing: {1}", name, font);
			}
		}

		public Font ParseFont (Size textureSize)
		{
			this.textureSize = textureSize;
			string type = lexer.ParseQuotedString ();
			if (type != "Hot::Font")
				throw new Exception ("Unknown type of object '{0}'", type);
			var font = new Font ();
			lexer.ParseToken ('{');
			while (lexer.PeekChar() != '}')
				ParseFontProperty (font, lexer.ParseWord ());
			lexer.ParseToken ('}');
			return font;
		}
	}
}

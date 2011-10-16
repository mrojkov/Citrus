using System.IO;
using Lime;
using Lemon;

namespace Orange
{
	public partial class FontImporter
	{
		Lexer lexer;
		string path;

		public FontImporter (string path)
		{
			this.path = path; 
			using (Stream stream = new FileStream(path, FileMode.Open)) {
				using (TextReader reader = new StreamReader(stream)) {
					string text = reader.ReadToEnd ();
					lexer = new Lexer (path, text);
				}
			}
		}
                    
		void ParseFontCharProperty (ref FontChar fontChar, string name)
		{
			switch (name) {
			case "CharCode":
				fontChar.Code = (uint)lexer.ParseInt ();
				break;
			case "TexPosition":
				fontChar.Position = lexer.ParseVector2 ();
				break;
			case "TexSize":
				fontChar.Size = lexer.ParseVector2 ();
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
				fontCharPair.Code1 = (uint)lexer.ParseInt ();
				break;
			case "CharCodeR":
				fontCharPair.Code2 = (uint)lexer.ParseInt ();
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

		public Font ParseFont ()
		{
			string type = lexer.ParseQuotedString ();
			if (type != "Hot::Font")
				throw new Exception ("Unknown type of object '{0}'", type);
			var font = new Font ();
			lexer.ParseToken ('{');
			while (lexer.PeekChar() != '}')
				ParseFontProperty (font, lexer.ParseWord ());
			lexer.ParseToken ('}');
			string texturePath = Path.ChangeExtension (path, ".png");
			font.Texture = new PersistentTexture (texturePath);
			return font;
		}
	}
}

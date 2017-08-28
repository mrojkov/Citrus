using System;
using System.Globalization;
using System.IO;
using System.Text;
using Lime;

namespace Orange
{
	public class HotLexer
	{
		private string sourcePath;
		private string text;
		private int position = 0;
		private readonly bool isTangerine;
		NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;

		public HotLexer(string sourcePath, string text, bool isTangerine)
		{
			this.sourcePath = sourcePath;
			this.text = text;
			this.isTangerine = isTangerine;
		}

		string GetSubstring(int position, int length)
		{
			int x = Math.Min(text.Length, length + position) - position;
			return text.Substring(position, x);
		}

		public float ParseFloat()
		{
			SkipWhitespace();
			int p = position;
			var negative = PeekByte() == '-';
			if (negative) {
				ReadByte();
			}
			while (Char.IsDigit((char)PeekByte())) {
				ReadByte();
			}
			if (PeekByte() == '.')
				ReadByte();
			while (Char.IsDigit((char)PeekByte())) {
				ReadByte();
			}
			string number = GetSubstring(p, position - p);
			var f = float.Parse(number, numberFormat);
			if (negative && f == 0f) {
				// Store negative zero for the lesser diff with the exported scene.
				f = -float.Epsilon;
			}
			return f;
		}

		public int ParseInt()
		{
			SkipWhitespace();
			int p = position;
			if (PeekByte() == '-')
				ReadByte();
			while (Char.IsDigit((char)PeekByte())) {
				ReadByte();
			}
			string number = GetSubstring(p, position - p);
			return Int32.Parse(number);
		}

		public string ParseQuotedString()
		{
			SkipWhitespace();
			char quote = PeekChar();
			if (quote != '\'' && quote != '\"')
				throw new Lime.Exception("Illegal symbol found near quoted string");
			ReadByte();
			var sb = new StringBuilder();
			while (true) {
				if (PeekByte() < 0)
					throw new Lime.Exception("Unterminated string");
				if (PeekByte() == quote) {
					ReadByte();
					break;
				}
				if (PeekByte() == '\\') {
					ReadByte();
					switch(ReadByte()) {
					case '\\':
						sb.Append('\\');
						break;
					case 'n':
						sb.Append('\n');
						break;
					case 't':
						sb.Append('\t');
						break;
					case 'r':
						sb.Append('\r');
						break;
					case '\"':
						sb.Append('\"');
						break;
					case '\'':
						sb.Append('\'');
						break;
					default:
						throw new Lime.Exception("Invalid escape sequence");
					}
				} else if (PeekByte() == '\n')
					throw new Lime.Exception("Illegal EOL found inside quoted string");
				else
					sb.Append((char)ReadByte());
			}
			return sb.ToString();
		}

		public string ReadLine()
		{
			var sb = new StringBuilder();
			while (!EndOfStream()) {
				if (PeekByte() == '\r') {
					ReadByte();
					if (PeekByte() == '\n') {
						ReadByte();
					}
					break;
				}
				if (PeekByte() == '\n') {
					ReadByte();
					break;
				}
				sb.Append((char)ReadByte());
			}
			return sb.Length > 0 ? sb.ToString() : null;
		}

		public uint ParseHex()
		{
			SkipWhitespace();
			if (ReadByte() != '0' || ReadByte() != 'x')
				throw new Lime.Exception("Incorrect hexadecimal number format");
			int p = position;
			while (true) {
				char c = Char.ToUpper((char)PeekByte());
				if (!Char.IsDigit(c) &&(c < 'A' || c > 'F'))
					break;
				ReadByte();
			}
			string number = GetSubstring(p, position - p);
			uint result = 0;
			for (int i = number.Length - 1, k = 0; i >= 0; --i, k += 4) {
				int c = number[i];
				int d = (c < 'A') ? '0' :('A' - 10);
				result += (uint)(c - d) << k;
			}
			return result;
		}

		public bool ParseBool()
		{
			string word = ParseWord();
			if (word == "true" || word == "1")
				return true;
			if (word == "false" || word == "0")
				return false;
			throw new Lime.Exception("Incorrect boolean value format");
		}

		public string ParseWord()
		{
			SkipWhitespace();
			var sb = new StringBuilder();
			while ((PeekByte() == '.' || Char.IsLetterOrDigit((char)PeekByte()) && !EndOfStream()))
				sb.Append((char)ReadByte());
			return sb.ToString();
		}

		public string PeekWord()
		{
			int p = position;
			string result = ParseWord();
			position = p;
			return result;
		}

		public void ParseToken(char token)
		{
			SkipWhitespace();
			if (ReadByte() != token)
				throw new Lime.Exception("Couldn't get expected '{0}'", token);
		}

		public void ParseExpectedToken(string token)
		{
			SkipWhitespace();
			for (int i = 0; i < token.Length; ++i)
				if (ReadByte() != token[i])
					throw new Lime.Exception("Couldn't get expected '{0}'", token);
		}

		public string PeekString()
		{
			int p = position;
			string result = ParseQuotedString();
			position = p;
			return result;
		}

		public char PeekChar()
		{
			int p = position;
			SkipWhitespace();
			int ch = ReadByte();
			if (ch < 0)
				throw new Lime.Exception("Unexpected end of file ocurred");
			position = p;
			return (char)ch;
		}

		public void SkipWhitespace()
		{
			bool comment = false;
			while (true) {
				int c = PeekByte();
				if (c < 0)
					break;
				if (comment) {
					if (c == '\n')
						comment = false;
				} else if (c == '#')
					comment = true;
				else if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
					break;
				ReadByte();
			}
		}

		public bool EndOfStream()
		{
			return position == text.Length;
		}

		int PeekByte()
		{
			return EndOfStream() ? -1 : text[position];
		}

		int ReadByte()
		{
			return EndOfStream() ? -1 : text[position++];
		}

		public Vector2 ParseVector2()
		{
			Vector2 v = new Vector2();
			ParseToken('[');
			v.X = ParseFloat();
			v.Y = ParseFloat();
			ParseToken(']');
			return v;
		}

		public Color4 ParseColor4()
		{
			uint rgba = ParseHex();
			var color = new Color4();
			color.B = (byte)(rgba & 0xFF);
			color.G = (byte)((rgba >> 8) & 0xFF);
			color.R = (byte)((rgba >> 16) & 0xFF);
			color.A = (byte)((rgba >> 24) & 0xFF);
			return color;
		}

		public NumericRange ParseNumericRange()
		{
			NumericRange v = new NumericRange();
			ParseToken('[');
			v.Median = ParseFloat();
			v.Dispersion = ParseFloat();
			ParseToken(']');
			return v;
		}

		public SkinningWeights ParseSkinningWeights()
		{
			SkinningWeights v = new SkinningWeights();
			ParseToken('[');
			v.Bone0.Index = ParseInt();
			v.Bone0.Weight = ParseFloat();
			v.Bone1.Index = ParseInt();
			v.Bone1.Weight = ParseFloat();
			v.Bone2.Index = ParseInt();
			v.Bone2.Weight = ParseFloat();
			v.Bone3.Index = ParseInt();
			v.Bone3.Weight = ParseFloat();
			ParseToken(']');
			if (v.Bone0.Weight == 0 && v.Bone1.Weight == 0 && v.Bone2.Weight == 0 && v.Bone3.Weight == 0)
				return null;
			return v;
		}

		public string ParsePath()
		{
			string path = ParseQuotedString();
			if (string.IsNullOrEmpty(path) || path[0] == '#')
				return path;
			if (!isTangerine) {
				if (path[0] == '/' || path[0] == '\\')
					path = path.Substring(1);
				else {
					string d = Path.GetDirectoryName(sourcePath);
					path = Path.Combine(d, path);
				}
			}
			path = Path.ChangeExtension(path, null);
			path = path.Replace('\\', '/');
			return path;
		}
	}
}

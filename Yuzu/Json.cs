using System;
using System.Text;

namespace Yuzu
{
	public class JsonSerializeOptions
	{
		public string FieldSeparator = "\n";
		public string Indent = "\t";
	};

	public class JsonSerializer : AbstractSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		public override void ToWriter(object obj)
		{
			Writer.Write('{');
			Writer.Write('\n');
			var first = true;
			foreach (var f in obj.GetType().GetFields()) {
				if (!first) {
					Writer.Write(',');
					WriteStr(JsonOptions.FieldSeparator);
				}
				first = false;
				WriteStr(JsonOptions.Indent);
				Writer.Write('"');
				WriteStr(f.Name);
				Writer.Write('"');
				Writer.Write(':');
				var t = f.FieldType;
				if (t == typeof(int)) {
					WriteStr(f.GetValue(obj).ToString());
				}
				else if (t == typeof(string)) {
					Writer.Write('"');
					WriteStr(f.GetValue(obj).ToString());
					Writer.Write('"');
				}
				else {
					throw new NotImplementedException(t.Name);
				}
			}
			if (!first)
				Writer.Write('\n');
			Writer.Write('}');
		}
	};

	public class JsonDeserializer : AbstractDeserializer
	{
		public static JsonDeserializer Instance = new JsonDeserializer();

		private char? buf;

		private char Next()
		{
			if (!buf.HasValue)
				return Reader.ReadChar();
			var result = buf.Value;
			buf = null;
			return result;
		}

		private void PutBack(char ch)
		{
			if (buf.HasValue)
				throw new YuzuException();
			buf = ch;
		}

		private char SkipSpaces()
		{
			char ch;
			do {
				ch = Next();
			} while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r');
			return ch;
		}

		private char Require(params char[] chars)
		{
			var ch = SkipSpaces();
			if (Array.IndexOf(chars, ch) < 0)
				throw new YuzuException();
			return ch;
		}

		private char JsonUnquote(char ch)
		{
			switch (ch) {
				case '"':
					return '"';
				case '\\':
					return '\\';
				case 'n':
					return '\n';
				case 't':
					return '\t';
			}
			throw new YuzuException();
		}

		private string RequireString()
		{
			var result = "";
			Require('"');
			while (true) {
				var ch = Next();
				if (ch == '"')
					break;
				if (ch == '\\')
					ch = JsonUnquote(Reader.ReadChar());
				result += ch;
			}
			return result;
		}

		private int RequireInt()
		{
			var result = "";
			var ch = SkipSpaces();
			while ('0' <= ch && ch <= '9') {
				result += ch;
				ch = Next();
			}
			PutBack(ch);
			return int.Parse(result);
		}

		public override void FromReader(object obj)
		{
			var first = true;
			Require('{');
			foreach (var f in obj.GetType().GetFields()) {
				if (!first) {
					Require(',');
				}
				first = false;
				var s = RequireString();
				Require(':');
				var t = f.FieldType;
				if (t == typeof(int)) {
					f.SetValue(obj, RequireInt());
				}
				else if (t == typeof(string)) {
					f.SetValue(obj, RequireString());
				}
				else {
					throw new NotImplementedException(t.Name);
				}
			}
		}
	}
}

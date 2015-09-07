using System;
using System.Text;

namespace Yuzu
{
	public class JsonSerializeOptions
	{
		public string FieldSeparator = "\n";
		public string Indent = "\t";
	};

	public class JsonSerializer : AbstractWriterSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		protected override void ToWriter(object obj)
		{
			writer.Write('{');
			writer.Write('\n');
			var first = true;
			foreach (var f in obj.GetType().GetFields()) {
				if (!first) {
					writer.Write(',');
					WriteStr(JsonOptions.FieldSeparator);
				}
				first = false;
				WriteStr(JsonOptions.Indent);
				writer.Write('"');
				WriteStr(f.Name);
				writer.Write('"');
				writer.Write(':');
				var t = f.FieldType;
				if (t == typeof(int)) {
					WriteStr(f.GetValue(obj).ToString());
				}
				else if (t == typeof(string)) {
					writer.Write('"');
					WriteStr(f.GetValue(obj).ToString());
					writer.Write('"');
				}
				else {
					throw new NotImplementedException(t.Name);
				}
			}
			if (!first)
				writer.Write('\n');
			writer.Write('}');
		}
	}

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

		private string GetNextName(bool first)
		{
			var ch = SkipSpaces();
			if (ch == ',') {
				if (first)
					throw new YuzuException();
				ch = SkipSpaces();
			}
			PutBack(ch);
			if (ch == '}')
				return "";
			var result = RequireString();
			Require(':');
			return result;
		}

		public override void FromReader(object obj)
		{
			buf = null;
			Require('{');
			string name = GetNextName(true);
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				if (yi.Name != name) {
					if (!yi.IsOptional)
						throw new YuzuException();
					continue;
				}
				var t = yi.FieldInfo.FieldType;
				if (t == typeof(int)) {
					yi.FieldInfo.SetValue(obj, RequireInt());
				}
				else if (t == typeof(string)) {
					yi.FieldInfo.SetValue(obj, RequireString());
				}
				else {
					throw new NotImplementedException(t.Name);
				}
				name = GetNextName(false);
			}
		}
	}
	public class JsonDeserializerGenerator
	{
		public static JsonDeserializerGenerator Instance = new JsonDeserializerGenerator();

		private int indent = 0;

		public void PutPart(string s)
		{
			Console.Write(s);
		}

		public void Put(string s)
		{
			if (s == "}\n")
				indent -= 1;
			for (int i = 0; i < indent; ++i)
				PutPart("\t");
			PutPart(s);
			if (s == "{\n")
				indent += 1;
		}

		public void PutF(string format, params object[] p)
		{
			Put(String.Format(format, p));
		}

		public void Generate<T>()
		{
			PutF("class {0}_JsonDeserializer : JsonDeserializer\n", typeof(T).Name);
			Put("{\n");
			Put("public override void FromReader(object obj)\n");
			Put("{\n");
			Put("buf = null;\n");
			Put("Require('{');\n");
			Put("var name = GetNextName(true);\n");
			PutF("var result = ({0})obj;\n", typeof(T).Name);
			foreach (var yi in Utils.GetYuzuItems(typeof(T), new CommonOptions())) {
				if (yi.IsOptional)
					PutF("if (\"{0}\" == name) result.{0} = ", yi.Name);
				else {
					PutF("if (\"{0}\" != name) throw new YuzuException();\n", yi.Name);
					PutF("result.{0} = ", yi.Name);
				}
				if (yi.Type == typeof(int)) {
					PutPart("RequireInt()\n");
				}
				else if (yi.Type == typeof(string)) {
					PutPart("RequireString()\n");
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
				}
				Put("name = GetNextName(false);\n");
			}
			Put("}\n");
			Put("}\n");
		}
	}
}

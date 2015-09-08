using System;
using System.Reflection;
using System.IO;

namespace Yuzu
{
	public class JsonSerializeOptions
	{
		public string FieldSeparator = "\n";
		public string Indent = "\t";
		public string ClassTag = "class";
	};

	public class JsonSerializer : AbstractWriterSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		private void WriteName(string name, ref bool isFirst) {
			if (!isFirst) {
				writer.Write(',');
				WriteStr(JsonOptions.FieldSeparator);
			}
			isFirst = false;
			WriteStr(JsonOptions.Indent);
			writer.Write('"');
			WriteStr(name);
			writer.Write('"');
			writer.Write(':');
		}

		protected override void ToWriter(object obj)
		{
			writer.Write('{');
			writer.Write('\n');
			var isFirst = true;
			if (Options.ClassNames) {
				WriteName(JsonOptions.ClassTag, ref isFirst);
				writer.Write('"');
				WriteStr(obj.GetType().FullName);
				writer.Write('"');
			}
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				WriteName(yi.Name, ref isFirst);
				if (yi.Type == typeof(int)) {
					WriteStr(yi.GetValue(obj).ToString());
				}
				else if (yi.Type == typeof(string)) {
					writer.Write('"');
					WriteStr(yi.GetValue(obj).ToString());
					writer.Write('"');
				}
				else if (yi.Type.IsClass) {
					ToWriter(yi.GetValue(obj));
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
				}
			}
			if (!isFirst)
				writer.Write('\n');
			writer.Write('}');
		}
	}

	public class JsonDeserializer : AbstractReaderDeserializer
	{
		public static JsonDeserializer Instance = new JsonDeserializer();
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		public JsonDeserializer()
		{
			Options.Assembly = Assembly.GetCallingAssembly();
		}

		protected char? buf;

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

		protected char Require(params char[] chars)
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

		protected string RequireString()
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

		protected int RequireInt()
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

		protected string GetNextName(bool first)
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

		protected object Make(string typeName)
		{
			return Activator.CreateInstance(Options.Assembly.GetType(typeName));
		}

		protected JsonDeserializerGenBase MakeDeserializer(string typeName)
		{
			return (JsonDeserializerGenBase)(Make(RequireString() + "_JsonDeserializer"));
		}

		protected virtual void ReadFields(object obj, string name)
		{
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				if (yi.Name != name) {
					if (!yi.IsOptional)
						throw new YuzuException();
					continue;
				}

				if (yi.Type == typeof(int)) {
					yi.SetValue(obj, RequireInt());
				}
				else if (yi.Type == typeof(string)) {
					yi.SetValue(obj, RequireString());
				}
				else if (yi.Type.IsClass) {
					object value;
					if (Options.ClassNames)
						value = FromReaderInt();
					else {
						value = Activator.CreateInstance(yi.Type);
						FromReaderInt(value);
					}
					yi.SetValue(obj, value);
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
				}
				name = GetNextName(false);
			}
			Require('}');
		}

		public override object FromReaderInt()
		{
			if (!Options.ClassNames)
				throw new YuzuException();
			buf = null;
			Require('{');
			if (GetNextName(true) != JsonOptions.ClassTag)
				throw new YuzuException();
			var obj = Make(RequireString());
			ReadFields(obj, GetNextName(false));
			return obj;
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			string name = GetNextName(true);
			if (Options.ClassNames) {
				if (name != JsonOptions.ClassTag)
					throw new YuzuException();
				var className = RequireString();
				if (className != obj.GetType().FullName)
					throw new YuzuException();
				name = GetNextName(false);
			}
			ReadFields(obj, name);
			return obj;
		}
	}

	public abstract class JsonDeserializerGenBase : JsonDeserializer
	{
		public abstract object FromReaderIntPartial(string name);
	}

	public class JsonDeserializerGenerator: JsonDeserializer
	{
		public static new JsonDeserializerGenerator Instance = new JsonDeserializerGenerator();

		private int indent = 0;
		public StreamWriter GenWriter;

		public void PutPart(string s)
		{
			GenWriter.Write(s.Replace("\n", "\r\n"));
		}

		public void Put(string s)
		{
			if (s == "}\n")
				indent -= 1;
			if (s != "\n")
				for (int i = 0; i < indent; ++i)
					PutPart(JsonOptions.Indent);
			PutPart(s);
			if (s.EndsWith("{\n"))
				indent += 1;
		}

		public void PutF(string format, params object[] p)
		{
			Put(String.Format(format, p));
		}

		public void GenerateHeader(string namespaceName)
		{
			Put("using Yuzu;\n");
			Put("\n");
			PutF("namespace {0}\n", namespaceName);
			Put("{\n");
			Put("\n");
		}

		public void GenerateFooter()
		{
			Put("}\n");
		}

		public void Generate<T>()
		{
			PutF("class {0}_JsonDeserializer : JsonDeserializerGenBase\n", typeof(T).Name);
			Put("{\n");

			PutF("public {0}_JsonDeserializer()\n", typeof(T).Name);
			Put("{\n");
			foreach (var f in Options.GetType().GetFields()) {
				var v = Utils.CodeValueFormat(f.GetValue(Options));
				if (v != "") // TODO
					PutF("Options.{0} = {1};\n", f.Name, v);
			}
			foreach (var f in JsonOptions.GetType().GetFields()) {
				var v = Utils.CodeValueFormat(f.GetValue(JsonOptions));
				if (v != "") // TODO
					PutF("JsonOptions.{0} = {1};\n", f.Name, v);
			}
			Put("}\n");
			Put("\n");

			Put("public override object FromReaderInt()\n");
			Put("{\n");
			// Since deserializer is dynamically constructed anyway, it is too late to determine object type here.
			PutF("return FromReaderInt(new {0}());\n", typeof(T).Name);
			Put("}\n");
			Put("\n");

			Put("public override object FromReaderInt(object obj)\n");
			Put("{\n");
			Put("buf = null;\n");
			Put("Require('{');\n");
			Put("var name = GetNextName(true);\n");
			if (Options.ClassNames) {
				Put("if (name != JsonOptions.ClassTag) throw new YuzuException();\n");
				PutF("if (RequireString() != \"{0}\") throw new YuzuException();\n", typeof(T).FullName);
				Put("name = GetNextName(false);\n");
			}
			Put("ReadFields(obj, name);\n");
			Put("return obj;\n");
			Put("}\n");
			Put("\n");

			Put("public override object FromReaderIntPartial(string name)\n");
			Put("{\n");
			PutF("var obj = new {0}();\n", typeof(T).Name);
			Put("ReadFields(obj, name);\n");
			Put("return obj;\n");
			Put("}\n");
			Put("\n");

			Put("private new void ReadFields(object obj, string name)\n");
			Put("{\n");
			PutF("var result = ({0})obj;\n", typeof(T).Name);
			foreach (var yi in Utils.GetYuzuItems(typeof(T), Options)) {
				if (yi.IsOptional) {
					PutF("if (\"{0}\" == name) {{\n", yi.Name);
					PutF("result.{0} = ", yi.Name);
				}
				else {
					PutF("if (\"{0}\" != name) throw new YuzuException();\n", yi.Name);
					PutF("result.{0} = ", yi.Name);
				}
				if (yi.Type == typeof(int)) {
					PutPart("RequireInt();\n");
				}
				else if (yi.Type == typeof(string)) {
					PutPart("RequireString();\n");
				}
				else if (yi.Type.IsClass) {
					if (Options.ClassNames) {
						PutPart(String.Format(
							"({0})MakeDeserializer(RequireString()).FromReader(Reader);\n", yi.Type.Name));
					}
					else {
						PutPart(String.Format("new {0}();\n", yi.Type.Name));
						PutF(
							"(new {0}_JsonDeserializer()).FromReader(result.{1}, Reader);\n",
							yi.Type.Name, yi.Name);
					}
				}
				else {
					throw new NotImplementedException(yi.Type.Name);
				}
				Put("name = GetNextName(false);\n");
				if (yi.IsOptional)
					Put("}\n");
			}
			Put("Require('}');\n");
			Put("}\n");
			Put("}\n");
			Put("\n");
		}

		public override object FromReaderInt()
		{
			if (!Options.ClassNames)
				throw new YuzuException();
			buf = null;
			Require('{');
			if (GetNextName(true) != JsonOptions.ClassTag)
				throw new YuzuException();
			return MakeDeserializer(RequireString()).FromReaderIntPartial(GetNextName(false));
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			string name = GetNextName(true);
			if (Options.ClassNames) {
				if (name != JsonOptions.ClassTag)
					throw new YuzuException();
				var className = RequireString();
				if (className != obj.GetType().FullName)
					throw new YuzuException();
				name = GetNextName(false);
			}
			ReadFields(obj, name);
			return obj;
		}
	}
}

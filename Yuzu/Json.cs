using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;

namespace Yuzu
{
	public class JsonSerializeOptions
	{
		public string FieldSeparator = "\n";
		public string Indent = "\t";
		public string ClassTag = "class";
		public bool EnumAsString = false;
	};

	public class JsonSerializer : AbstractWriterSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		private void WriteInt(object obj)
		{
			WriteStr(obj.ToString());
		}

		private void WriteEnumAsInt(object obj)
		{
			WriteStr(((int)obj).ToString());
		}

		private void WriteString(object obj)
		{
			writer.Write('"');
			WriteStr(obj.ToString());
			writer.Write('"');
		}

		private void WriteList<T>(List<T> list)
		{
			var wf = GetWriteFunc(typeof(T));
			writer.Write('[');
			if (list.Count > 0) {
				var isFirst = true;
				foreach (var elem in list) {
					if (!isFirst)
						writer.Write(',');
					isFirst = false;
					WriteStr(JsonOptions.FieldSeparator);
					wf(elem);
				}
				WriteStr(JsonOptions.FieldSeparator);
			}
			writer.Write(']');
		}

		private Action<object> GetWriteFunc(Type t)
		{
			if (t == typeof(int))
				return WriteInt;
			if (t == typeof(string))
				return WriteString;
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return WriteString;
				else
					return WriteEnumAsInt;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteList", t);
				return obj => m.Invoke(this, new object[] { obj });
			}
			if (t.IsClass)
				return ToWriter;
			throw new NotImplementedException(t.Name);
		}

		private void WriteName(string name, ref bool isFirst)
		{
			if (!isFirst) {
				writer.Write(',');
				WriteStr(JsonOptions.FieldSeparator);
			}
			isFirst = false;
			WriteStr(JsonOptions.Indent);
			WriteString(name);
			writer.Write(':');
		}

		protected override void ToWriter(object obj)
		{
			writer.Write('{');
			WriteStr(JsonOptions.FieldSeparator);
			var isFirst = true;
			if (Options.ClassNames) {
				WriteName(JsonOptions.ClassTag, ref isFirst);
				WriteString(obj.GetType().FullName);
			}
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				WriteName(yi.Name, ref isFirst);
				GetWriteFunc(yi.Type)(yi.GetValue(obj));
			}
			if (!isFirst)
				WriteStr(JsonOptions.FieldSeparator);
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
			char ch = Next();
			while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
				ch = Reader.ReadChar();
			return ch;
		}

		protected char SkipSpacesCarefully()
		{
			if (buf.HasValue)
				throw new YuzuException();
			while (true) {
				var v = Reader.PeekChar();
				if (v < 0)
					return '\0';
				var ch = (char)v;
				if (ch != ' ' && ch != '\t' || ch != '\n' || ch != '\r')
					return ch;
				Reader.ReadChar();
			}
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

		// Optimization: avoid re-creating StringBuilder.
		private StringBuilder requireStringResult = new StringBuilder();

		protected string RequireString()
		{
			requireStringResult.Clear();
			Require('"');
			while (true) {
				// Optimization: buf is guaranteed to be empty after Require, so no need to call Next.
				var ch = Reader.ReadChar();
				if (ch == '"')
					break;
				if (ch == '\\')
					ch = JsonUnquote(Reader.ReadChar());
				requireStringResult.Append(ch);
			}
			return requireStringResult.ToString();
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

		protected JsonDeserializerGenBase MakeDeserializer()
		{
			return (JsonDeserializerGenBase)(Make(RequireString() + "_JsonDeserializer"));
		}

		private void ReadList<T>(List<T> list)
		{
			do {
				list.Add((T)ReadValue(typeof(T)));
			} while (Require(']', ',') == ',');
		}

		private object ReadValue(Type t)
		{
			if (t == typeof(int))
				return RequireInt();
			if (t == typeof(string))
				return RequireString();
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return Enum.Parse(t, RequireString());
				else
					return Enum.ToObject(t, RequireInt());
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
				var list = Activator.CreateInstance(t);
				Require('[');
				// ReadValue might invoke a new serializer, so we must not rely on PutBack.
				if (SkipSpacesCarefully() == ']')
					Require(']');
				else
					Utils.GetPrivateCovariantGeneric(GetType(), "ReadList", t).
						Invoke(this, new object[] { list });
				return list;
			}
			if (t.IsClass) {
				if (Options.ClassNames)
					return FromReaderInt();
				var value = Activator.CreateInstance(t);
				FromReaderInt(value);
				return value;
			}
			throw new NotImplementedException(t.Name);
		}

		protected virtual void ReadFields(object obj, string name)
		{
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				if (yi.Name != name) {
					if (!yi.IsOptional)
						throw new YuzuException();
					continue;
				}
				yi.SetValue(obj, ReadValue(yi.Type));
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
		protected string className;

		public override object FromReaderInt()
		{
			if (!Options.ClassNames)
				throw new YuzuException();
			buf = null;
			Require('{');
			if (GetNextName(true) != JsonOptions.ClassTag)
				throw new YuzuException();
			var d = MakeDeserializer();
			d.Reader = Reader;
			Require(',');
			return d.FromReaderIntPartial(GetNextName(false));
		}

		public override object FromReaderInt(object obj)
		{
			buf = null;
			Require('{');
			var name = GetNextName(true);
			if (Options.ClassNames) {
				if (name != JsonOptions.ClassTag)
					throw new YuzuException();
				if (RequireString() != className)
					throw new YuzuException();
				name = GetNextName(false);
			}
			ReadFields(obj, name);
			return obj;
		}

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
			if (s.StartsWith("}")) // "}\n" or "} while"
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
			Put("using System;\n");
			Put("using System.Collections.Generic;\n");
			Put("using System.Reflection;\n");
			Put("\n");
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

		private int tempCount = 0;

		private string GetTypeSpec(Type t)
		{
			return t.IsGenericType ?
				String.Format("{0}<{1}>",
					t.Name.Remove(t.Name.IndexOf('`')),
					String.Join(",", t.GetGenericArguments().Select(GetTypeSpec))) :
				t.Name;
		}

		private void GenerateValue(Type t, string name)
		{
			if (t == typeof(int)) {
				PutPart("RequireInt();\n");
			}
			else if (t == typeof(string)) {
				PutPart("RequireString();\n");
			}
			else if (t.IsEnum) {
				PutPart(String.Format(
					JsonOptions.EnumAsString ?
						"({0})Enum.Parse(typeof({0}), RequireString());\n" :
						"({0})RequireInt();\n",
					t.Name));
			}
			else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
				PutPart(String.Format("new {0}();\n", GetTypeSpec(t)));
				Put("Require('[');\n");
				Put("if (SkipSpacesCarefully() == ']') {\n");
				Put("Require(']');\n");
				Put("}\n");
				Put("else {\n");
				Put("do {\n");
				tempCount += 1;
				var tempName = "tmp" + tempCount.ToString();
				PutF("var {0} = ", tempName);
				GenerateValue(t.GetGenericArguments()[0], tempName);
				PutF("{0}.Add({1});\n", name, tempName);
				Put("} while (Require(']', ',') == ',');\n");
				Put("}\n");
			}
			else if (t.IsClass) {
				if (Options.ClassNames) {
					PutPart(String.Format("({0})base.FromReaderInt();\n", t.Name));
				}
				else {
					PutPart(String.Format("new {0}();\n", t.Name));
					PutF("{0}_JsonDeserializer.Instance.FromReader({1}, Reader);\n", t.Name, name);
				}
			}
			else {
				throw new NotImplementedException(t.Name);
			}
		}

		public void Generate<T>()
		{
			PutF("class {0}_JsonDeserializer : JsonDeserializerGenBase\n", typeof(T).Name);
			Put("{\n");

			PutF("public static new {0}_JsonDeserializer Instance = new {0}_JsonDeserializer();\n", typeof(T).Name);
			Put("\n");

			PutF("public {0}_JsonDeserializer()\n", typeof(T).Name);
			Put("{\n");
			PutF("className = \"{0}\";\n", typeof(T).FullName);
			PutF("Options.Assembly = Assembly.Load(\"{0}\");\n", typeof(T).Assembly.FullName);
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

			Put("public override object FromReaderIntPartial(string name)\n");
			Put("{\n");
			PutF("var obj = new {0}();\n", typeof(T).Name);
			Put("ReadFields(obj, name);\n");
			Put("return obj;\n");
			Put("}\n");
			Put("\n");

			Put("protected override void ReadFields(object obj, string name)\n");
			Put("{\n");
			PutF("var result = ({0})obj;\n", typeof(T).Name);
			tempCount = 0;
			foreach (var yi in Utils.GetYuzuItems(typeof(T), Options)) {
				if (yi.IsOptional) {
					PutF("if (\"{0}\" == name) {{\n", yi.Name);
					PutF("result.{0} = ", yi.Name);
				}
				else {
					PutF("if (\"{0}\" != name) throw new YuzuException();\n", yi.Name);
					PutF("result.{0} = ", yi.Name);
				}
				GenerateValue(yi.Type, "result." + yi.Name);
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
			var d = MakeDeserializer();
			d.Reader = Reader;
			Require(',');
			return d.FromReaderIntPartial(GetNextName(false));
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

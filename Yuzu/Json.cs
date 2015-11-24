using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Yuzu
{
	public class JsonSerializeOptions
	{
		public string FieldSeparator = "\n";
		public string Indent = "\t";
		public string ClassTag = "class";
		public bool EnumAsString = false;
		public bool ArrayLengthPrefix = false;
		public bool IgnoreCompact = false;
	};

	public class JsonSerializer : AbstractWriterSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		private void WriteInt(object obj)
		{
			WriteStr(obj.ToString());
		}

		private void WriteDouble(object obj)
		{
			WriteStr(((double)obj).ToString(CultureInfo.InvariantCulture));
		}

		private void WriteSingle(object obj)
		{
			WriteStr(((float)obj).ToString(CultureInfo.InvariantCulture));
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

		private void WriteBool(object obj)
		{
			WriteStr((bool)obj ? "true" : "false");
		}

		private void WriteList<T>(List<T> list)
		{
			if (list == null) {
				WriteStr("null");
				return;
			}
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

		private void WriteDictionary<T>(Dictionary<string, T> dict)
		{
			if (dict == null) {
				WriteStr("null");
				return;
			}
			var wf = GetWriteFunc(typeof(T));
			writer.Write('{');
			if (dict.Count > 0) {
				WriteStr(JsonOptions.FieldSeparator);
				var isFirst = true;
				foreach (var elem in dict) {
					WriteName(elem.Key, ref isFirst);
					wf(elem.Value);
				}
				WriteStr(JsonOptions.FieldSeparator);
			}
			writer.Write('}');
		}

		private void WriteArray<T>(T[] array)
		{
			var wf = GetWriteFunc(typeof(T));
			writer.Write('[');
			if (array.Length > 0) {
				if (JsonOptions.ArrayLengthPrefix)
					WriteStr(array.Length.ToString());
				var isFirst = !JsonOptions.ArrayLengthPrefix;
				foreach (var elem in array) {
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
			if (t == typeof(int) || t == typeof(uint))
				return WriteInt;
			if (t == typeof(double))
				return WriteDouble;
			if (t == typeof(float))
				return WriteSingle;
			if (t == typeof(string))
				return WriteString;
			if (t == typeof(bool))
				return WriteBool;
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return WriteString;
				else
					return WriteEnumAsInt;
			}
			if (t.IsGenericType)
			{
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(List<>)) {
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteList", t);
					return obj => m.Invoke(this, new object[] { obj });
				}
				if (g == typeof(Dictionary<,>) && t.GetGenericArguments()[0] == typeof(string)) {
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteDictionary", t, argNumber: 1);
					return obj => m.Invoke(this, new object[] { obj });
				}
			}
			if (t.IsArray) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteArray", t);
				return obj => m.Invoke(this, new object[] { obj });
			}
			if (Utils.IsStruct(t) || t.IsClass) {
				if (Utils.IsCompact(t, Options) && !JsonOptions.IgnoreCompact)
					return ToWriterCompact;
				else
					return ToWriter;
			}
			throw new NotImplementedException(t.Name);
		}

		private void WriteSep(ref bool isFirst)
		{
			if (!isFirst) {
				writer.Write(',');
				WriteStr(JsonOptions.FieldSeparator);
			}
			isFirst = false;
		}

		private void WriteName(string name, ref bool isFirst)
		{
			WriteSep(ref isFirst);
			WriteStr(JsonOptions.Indent);
			WriteString(name);
			writer.Write(':');
		}

		protected override void ToWriter(object obj)
		{
			if (obj == null) {
				WriteStr("null");
				return;
			}
			writer.Write('{');
			WriteStr(JsonOptions.FieldSeparator);
			var isFirst = true;
			var t = obj.GetType();
			if (Options.ClassNames && !Utils.IsStruct(t)) {
				WriteName(JsonOptions.ClassTag, ref isFirst);
				WriteString(t.FullName);
			}
			foreach (var yi in Utils.GetYuzuItems(t, Options)) {
				var value = yi.GetValue(obj);
				if (yi.SerializeIf != null && !yi.SerializeIf(obj, value))
					continue;
				WriteName(yi.Tag(Options), ref isFirst);
				GetWriteFunc(yi.Type)(value);
			}
			if (!isFirst)
				WriteStr(JsonOptions.FieldSeparator);
			writer.Write('}');
		}

		private void ToWriterCompact(object obj)
		{
			writer.Write('[');
			WriteStr(JsonOptions.FieldSeparator);
			var isFirst = true;
			var t = obj.GetType();
			if (Options.ClassNames && !Utils.IsStruct(t)) {
				WriteSep(ref isFirst);
				WriteString(t.FullName);
			}
			foreach (var yi in Utils.GetYuzuItems(t, Options)) {
				WriteSep(ref isFirst);
				GetWriteFunc(yi.Type)(yi.GetValue(obj));
			}
			if (!isFirst)
				WriteStr(JsonOptions.FieldSeparator);
			writer.Write(']');
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

		private char? buf;

		public override void Initialize() { buf = null; }

		private char Next()
		{
			if (!buf.HasValue)
				return Reader.ReadChar();
			var result = buf.Value;
			buf = null;
			return result;
		}

		protected YuzuException Error(string message, params object[] args)
		{
			return new YuzuException(
				String.Format(message, args), new YuzuPosition(Reader.BaseStream.Position));
		}

		protected void KillBuf()
		{
			if (buf != null)
				throw Error("Unconsumed character: {0}", buf);
		}

		private void PutBack(char ch)
		{
			if (buf.HasValue)
				throw new YuzuAssert();
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
				throw new YuzuAssert();
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
			if(Array.IndexOf(chars, ch) < 0)
				throw Error("Expected '{0}' but found '{1}'", String.Join("','", chars), ch);
			return ch;
		}

		protected void Require(string s)
		{
			foreach (var ch in s) {
				var r = Reader.ReadChar();
				if (r != ch)
					throw Error("Expected '{0}', but found '{1}'", ch, r);
			}
		}

		private char JsonUnescape(char ch)
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
			throw Error("Unexpected escape chararcter: '{0}'", ch);
		}

		// Optimization: avoid re-creating StringBuilder.
		private StringBuilder sb = new StringBuilder();

		protected string RequireString()
		{
			sb.Clear();
			Require('"');
			while (true) {
				// Optimization: buf is guaranteed to be empty after Require, so no need to call Next.
				var ch = Reader.ReadChar();
				if (ch == '"')
					break;
				if (ch == '\\')
					ch = JsonUnescape(Reader.ReadChar());
				sb.Append(ch);
			}
			return sb.ToString();
		}

		protected bool RequireBool()
		{
			var ch = SkipSpaces();
			if (ch == 't') {
				Require("rue");
				return true;
			}
			if (ch == 'f') {
				Require("alse");
				return false;
			}
			throw Error("Expected 'true' or 'false', but found: {0}", ch);
		}

		protected uint RequireUInt()
		{
			var ch = SkipSpaces();
			uint result = 0;
			while ('0' <= ch && ch <= '9') {
				checked { result = result * 10 + (uint)ch - (uint)'0'; }
				ch = Reader.ReadChar();
			}
			PutBack(ch);
			return result;
		}

		protected int RequireInt()
		{
			var ch = SkipSpaces();
			int sign = 1;
			if (ch == '-') {
				sign = -1;
				ch = Reader.ReadChar();
			}
			int result = 0;
			while ('0' <= ch && ch <= '9') {
				checked { result = result * 10 + (int)ch - (int)'0'; }
				ch = Reader.ReadChar();
			}
			PutBack(ch);
			return sign * result;
		}

		private string ParseFloat()
		{
			// Optimization: Do not extract helper methods.
			sb.Clear();
			var ch = SkipSpaces();
			if (ch == '-') {
				sb.Append(ch);
				ch = Reader.ReadChar();
			}
			while ('0' <= ch && ch <= '9') {
				sb.Append(ch);
				ch = Reader.ReadChar();
			}
			if (ch == '.') {
				sb.Append(ch);
				ch = Reader.ReadChar();
				while ('0' <= ch && ch <= '9') {
					sb.Append(ch);
					ch = Reader.ReadChar();
				}
			}
			if (ch == 'e'|| ch == 'E') {
				sb.Append(ch);
				ch = Reader.ReadChar();
				if (ch == '+' || ch == '-') {
					sb.Append(ch);
					ch = Reader.ReadChar();
				}
				while ('0' <= ch && ch <= '9') {
					sb.Append(ch);
					ch = Reader.ReadChar();
				}
			}
			PutBack(ch);
			return sb.ToString();
		}

		protected double RequireDouble()
		{
			return Double.Parse(ParseFloat(), CultureInfo.InvariantCulture);
		}

		protected float RequireSingle()
		{
			return Single.Parse(ParseFloat(), CultureInfo.InvariantCulture);
		}

		protected string GetNextName(bool first)
		{
			var ch = SkipSpaces();
			if (ch == ',') {
				if (first)
					throw Error("Expected name, but got ','");
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
			var t = Options.Assembly.GetType(typeName);
			if (t == null)
				throw new YuzuAssert("Type not found: " + typeName);
			return Activator.CreateInstance(t);
		}

		protected bool RequireOrNull(char ch)
		{
			if (Require(ch, 'n') == ch)
				return false;
			Require("ull");
			return true;
		}

		protected char RequireBracketOrNull()
		{
			var ch = Require('{', '[', 'n');
			if (ch == 'n')
				Require("ull");
			return ch;
		}

		private List<T> ReadList<T>()
		{
			if (RequireOrNull('[')) return null;
			var list = new List<T>();
			// ReadValue might invoke a new serializer, so we must not rely on PutBack.
			if (SkipSpacesCarefully() == ']')
				Require(']');
			else {
				var rf = ReadValueFunc(typeof(T));
				do {
					list.Add((T)rf());
				} while (Require(']', ',') == ',');
			}
			return list;
		}

		private Dictionary<string, T> ReadDictionary<T>()
		{
			if (RequireOrNull('{')) return null;
			var dict = new Dictionary<string, T>();
			// ReadValue might invoke a new serializer, so we must not rely on PutBack.
			if (SkipSpacesCarefully() == '}')
				Require('}');
			else {
				var rf = ReadValueFunc(typeof(T));
				do {
					var key = RequireString();
					Require(':');
					dict.Add(key, (T)rf());
				} while (Require('}', ',') == ',');
			}
			return dict;
		}

		private T[] ReadArray<T>()
		{
			return ReadList<T>().ToArray();
		}

		private T[] ReadArrayWithLengthPrefix<T>()
		{
			Require('[');
			// ReadValue might invoke a new serializer, so we must not rely on PutBack.
			if (SkipSpacesCarefully() == ']') {
				Require(']');
				return new T[0];
			}
			var array = new T[RequireUInt()];
			var rf = ReadValueFunc(typeof(T));
			for (int i = 0; i < array.Length; ++i) {
				Require(',');
				array[i] = (T)rf();
			}
			Require(']');
			return array;
		}

		private object ReadObject() {
			var ch = SkipSpaces();
			PutBack(ch);
			switch (ch) {
				case '\"':
					return RequireString();
				case 't': case 'f':
					return RequireBool();
				case 'n':
					Next();
					Require("ull");
					return null;
				case '{':
					return ReadDictionary<object>();
				case '[':
					return ReadList<object>();
				default:
					return RequireDouble();
			}
		}

		// Optimization: Avoid creating trivial closures.
		private object RequireIntObj() { return RequireInt(); }
		private object RequireStringObj() { return RequireString(); }
		private object RequireBoolObj() { return RequireBool(); }
		private object RequireUIntObj() { return RequireUInt(); }
		private object RequireSingleObj() { return RequireSingle(); }
		private object RequireDoubleObj() { return RequireDouble(); }

		private Func<object> ReadValueFunc(Type t)
		{
			if (t == typeof(int))
				return RequireIntObj;
			if (t == typeof(uint))
				return RequireUIntObj;
			if (t == typeof(string))
				return RequireStringObj;
			if (t == typeof(bool))
				return RequireBoolObj;
			if (t == typeof(float))
				return RequireSingleObj;
			if (t == typeof(double))
				return RequireDoubleObj;
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return () => Enum.Parse(t, RequireString());
				else
					return () => Enum.ToObject(t, RequireInt());
			}
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(List<>)) {
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadList", t);
					return () => m.Invoke(this, new object[] { });
				}
				if (g == typeof(Dictionary<,>)) {
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadDictionary", t, argNumber: 1);
					return () => m.Invoke(this, new object[] { });
				}
			}
			if (t.IsArray) {
				var n = JsonOptions.ArrayLengthPrefix ? "ReadArrayWithLengthPrefix" : "ReadArray";
				var m = Utils.GetPrivateCovariantGeneric(GetType(), n, t);
				return () => m.Invoke(this, new object[] { });
			}
			if (t == typeof(object))
				return ReadObject;
			if (t.IsClass && Options.ClassNames)
				return FromReaderInt;
			if (t.IsClass && !Options.ClassNames || Utils.IsStruct(t))
				return () => FromReaderInt(Activator.CreateInstance(t));
			throw new NotImplementedException(t.Name);
		}

		protected virtual object ReadFields(object obj, string name)
		{
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				var cmp = String.CompareOrdinal(yi.Tag(Options), name);
				if (Options.IgnoreNewFields && Options.TagMode != TagMode.Names)
					while (cmp > 0 && name != "") {
						ReadObject();
						name = GetNextName(false);
						cmp = String.CompareOrdinal(yi.Tag(Options), name);
					}
				if (cmp != 0) {
					if (!yi.IsOptional)
						throw Error("Expected field '{0}', but found '{1}'", yi.NameTagged(Options), name);
					continue;
				}
				yi.SetValue(obj, ReadValueFunc(yi.Type)());
				name = GetNextName(false);
			}
			if (Options.IgnoreNewFields)
				while (name != "") {
					ReadObject();
					name = GetNextName(false);
				}
			Require('}');
			return obj;
		}

		protected virtual object ReadFieldsCompact(object obj)
		{
			if (!Utils.IsCompact(obj.GetType(), Options))
				throw Error("Attempt to read non-compact type '{0}' from compact format", obj.GetType().Name);
			bool isFirst = true;
			foreach (var yi in Utils.GetYuzuItems(obj.GetType(), Options)) {
				if (!isFirst)
					Require(',');
				isFirst = false;
				yi.SetValue(obj, ReadValueFunc(yi.Type)());
			}
			Require(']');
			return obj;
		}

		private void CheckClassTag(string name)
		{
			if (name != JsonOptions.ClassTag)
				throw Error("Expected class tag, but found '{0}'", name);
		}

		private void CheckSameTypeAsTag(object obj)
		{
			var typeName = obj.GetType().FullName;
			var tag = RequireString();
			if (typeName != tag)
				throw Error("Reading object of type '{0}', but found class tag '{1}'", typeName, tag);
		}

		public override object FromReaderInt()
		{
			if (!Options.ClassNames)
				throw new YuzuException("Attempt to read unspecified type without class name");
			KillBuf();
			switch (RequireBracketOrNull()) {
				case 'n': return null;
				case '{':
					CheckClassTag(GetNextName(true));
					return ReadFields(Make(RequireString()), GetNextName(false));
				case '[':
					return ReadFieldsCompact(Make(RequireString()));
				default:
					throw new YuzuAssert();
			}
		}

		public override object FromReaderInt(object obj)
		{
			KillBuf();
			switch (RequireBracketOrNull()) {
				case 'n':
					return null;
				case '{':
					string name = GetNextName(true);
					if (Options.ClassNames) {
						CheckClassTag(name);
						CheckSameTypeAsTag(obj);
						name = GetNextName(false);
					}
					return ReadFields(obj, name);
				case '[':
					if (Options.ClassNames)
						CheckSameTypeAsTag(obj);
					return ReadFieldsCompact(obj);
				default:
					throw new YuzuAssert();
			}
		}

		private JsonDeserializerGenBase MakeDeserializer(string className)
		{
			var result = (JsonDeserializerGenBase)(Make(className + "_JsonDeserializer"));
			result.Reader = Reader;
			return result;
		}

		protected object FromReaderIntGenerated()
		{
			if (!Options.ClassNames)
				throw new YuzuException("Attempt to read unspecified type without class name");
			KillBuf();
			Require('{');
			CheckClassTag(GetNextName(true));
			var d = MakeDeserializer(RequireString());
			Require(',');
			return d.FromReaderIntPartial(GetNextName(false));
		}

		protected object FromReaderIntGenerated(object obj)
		{
			KillBuf();
			Require('{');
			string name = GetNextName(true);
			if (Options.ClassNames) {
				CheckClassTag(name);
				CheckSameTypeAsTag(obj);
				name = GetNextName(false);
			}
			return MakeDeserializer(obj.GetType().FullName).ReadFields(obj, name);
		}
	}
}

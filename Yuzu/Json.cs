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
		private int generation = 0;
		public int Generation { get { return generation; } }

		public string FieldSeparator = "\n";
		public string Indent = "\t";
		public string ClassTag = "class";

		private bool enumAsString = false;
		public bool EnumAsString { get { return enumAsString; } set { enumAsString = value; generation++; } }

		public bool ArrayLengthPrefix = false;

		private bool ignoreCompact = false;
		public bool IgnoreCompact { get { return ignoreCompact; } set { ignoreCompact = value; generation++; } }

		public string DateFormat = "O";
		public string TimeSpanFormat = "c";
	};

	internal static class JsonEscapeData
	{
		public static char[] unescapeChars = new char['t' + 1];
		public static char[] escapeChars = new char['\\' + 1];
		public static int[] hexDigits = new int['f' + 1];
		public static char[] digitHex = new char[16];

		// Optimization: array access is slightly faster than two or more sequential comparisons.
		static JsonEscapeData()
		{
			for (int i = 0; i < hexDigits.Length; ++i)
				hexDigits[i] = -1;
			for (int i = 0; i < 10; ++i) {
				hexDigits[i + '0'] = i;
				digitHex[i] = (char)(i + '0');
			}
			for (int i = 0; i < 6; ++i) {
				hexDigits[i + 'a'] = hexDigits[i + 'A'] = i + 10;
				digitHex[i + 10] = (char)(i + 'a');
			}
			unescapeChars['"'] = '"';
			unescapeChars['\\'] = '\\';
			unescapeChars['/'] = '/';
			unescapeChars['b'] = '\b';
			unescapeChars['f'] = '\f';
			unescapeChars['n'] = '\n';
			unescapeChars['r'] = '\r';
			unescapeChars['t'] = '\t';

			escapeChars['"'] = '"';
			escapeChars['\\'] = '\\';
			escapeChars['/'] = '/';
			escapeChars['\b'] = 'b';
			escapeChars['\f'] = 'f';
			escapeChars['\n'] = 'n';
			escapeChars['\r'] = 'r';
			escapeChars['\t'] = 't';
		}
	}

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

		private void WriteUnescapedString(object obj)
		{
			writer.Write('"');
			writer.Write(Encoding.UTF8.GetBytes(obj.ToString()));
			writer.Write('"');
		}

		private void WriteEscapedString(object obj)
		{
			writer.Write('"');
			foreach (var ch in obj.ToString()) {
				var escape = ch <= '\\' ? JsonEscapeData.escapeChars[ch] : '\0';
				if (escape > 0) {
					writer.Write('\\');
					writer.Write(escape);
				}
				else if (ch < ' ') {
					writer.Write('\\');
					writer.Write('u');
					for (int i = 3 * 4; i >= 0; i -= 4)
						writer.Write(JsonEscapeData.digitHex[ch >> i & 0xf]);
				}
				else {
					writer.Write(ch);
				}
			}
			writer.Write('"');
		}

		private void WriteBool(object obj)
		{
			WriteStr((bool)obj ? "true" : "false");
		}

		private void WriteDateTime(object obj)
		{
			var s = ((DateTime)obj).ToString(JsonOptions.DateFormat, CultureInfo.InvariantCulture);
			// 'Roundtrip' format is guaranteed to be ASCII-clean.
			if (JsonOptions.DateFormat == "O")
				WriteUnescapedString(s);
			else
				WriteEscapedString(s);
		}

		private void WriteTimeSpan(object obj)
		{
			var s = ((TimeSpan)obj).ToString(JsonOptions.TimeSpanFormat, CultureInfo.InvariantCulture);
			// 'Constant' format is guaranteed to be ASCII-clean.
			if (JsonOptions.DateFormat == "c")
				WriteUnescapedString(s);
			else
				WriteEscapedString(s);
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

		private void WriteDictionary<K, V>(Dictionary<K, V> dict)
		{
			if (dict == null) {
				WriteStr("null");
				return;
			}
			var wf = GetWriteFunc(typeof(V));
			writer.Write('{');
			if (dict.Count > 0) {
				WriteStr(JsonOptions.FieldSeparator);
				var isFirst = true;
				foreach (var elem in dict) {
					WriteSep(ref isFirst);
					WriteStr(JsonOptions.Indent);
					// TODO: Option to not escape dictionary keys.
					WriteEscapedString(elem.Key.ToString());
					writer.Write(':');
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

		private Dictionary<Type, Action<object>> writerCache = new Dictionary<Type, Action<object>>();
		private int jsonOptionsGeneration = 0;

		private Action<object> GetWriteFunc(Type t)
		{
			if (jsonOptionsGeneration != JsonOptions.Generation) {
				writerCache.Clear();
				jsonOptionsGeneration = JsonOptions.Generation;
			}

			Action<object> result;
			if (writerCache.TryGetValue(t, out result))
				return result;
			result = MakeWriteFunc(t);
			writerCache[t] = result;
			return result;
		}

		private Action<object> MakeWriteFunc(Type t)
		{
			if (t == typeof(int) || t == typeof(uint) || t == typeof(byte) || t == typeof(sbyte))
				return WriteInt;
			if (t == typeof(double))
				return WriteDouble;
			if (t == typeof(float))
				return WriteSingle;
			if (t == typeof(string))
				return WriteEscapedString;
			if (t == typeof(bool))
				return WriteBool;
			if (t == typeof(DateTime))
				return WriteDateTime;
			if (t == typeof(TimeSpan))
				return WriteTimeSpan;
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return WriteUnescapedString;
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
				if (g == typeof(Dictionary<,>)) {
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), "WriteDictionary", t);
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
			WriteUnescapedString(name);
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
				WriteUnescapedString(t.FullName);
			}
			foreach (var yi in Meta.Get(t, Options).Items) {
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
			if (obj == null) {
				WriteStr("null");
				return;
			}
			writer.Write('[');
			WriteStr(JsonOptions.FieldSeparator);
			var isFirst = true;
			var t = obj.GetType();
			if (Options.ClassNames && !Utils.IsStruct(t)) {
				WriteSep(ref isFirst);
				WriteUnescapedString(t.FullName);
			}
			foreach (var yi in Meta.Get(t, Options).Items) {
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
				if (ch != ' ' && ch != '\t' && ch != '\n' && ch != '\r')
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

		protected string RequireUnescapedString()
		{
			sb.Clear();
			Require('"');
			while (true) {
				var ch = Reader.ReadChar();
				if (ch == '"')
					break;
				sb.Append(ch);
			}
			return sb.ToString();
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
				if (ch == '\\') {
					ch = Reader.ReadChar();
					if (ch == 'u') {
						int code = 0;
						for (int i = 0; i < 4; ++i) {
							ch = Reader.ReadChar();
							int h = ch <= 'f' ? JsonEscapeData.hexDigits[ch] : -1;
							if (h < 0)
								throw Error("Bad hexadecimal digit in unicode escape: '{0}'", ch);
							code = code * 16 + h;
						}
						ch = (char)code;
					}
					else {
						var escaped = ch <= 't' ? JsonEscapeData.unescapeChars[ch] : '\0';
						if (escaped == 0)
							throw Error("Unexpected escape chararcter: '{0}'", ch);
						ch = escaped;
					}
				}
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

		protected DateTime RequireDateTime()
		{
			var s = JsonOptions.DateFormat == "O" ? RequireUnescapedString() : RequireString();
			return DateTime.ParseExact(s, JsonOptions.DateFormat, CultureInfo.InvariantCulture);
		}

		protected TimeSpan RequireTimeSpan()
		{
			var s = JsonOptions.TimeSpanFormat == "c" ? RequireUnescapedString() : RequireString();
			return TimeSpan.ParseExact(s, JsonOptions.TimeSpanFormat, CultureInfo.InvariantCulture);
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
			var result = RequireUnescapedString();
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

		protected static Dictionary<Type, Func<string, object>> keyParsers = new Dictionary<Type, Func<string, object>> {
			{ typeof(int), s => int.Parse(s) },
			{ typeof(string), s => s },
		};

		public static void RegisterKeyParser(Type t, Func<string, object> parser)
		{
			keyParsers.Add(t, parser);
		}

		private Dictionary<K, V> ReadDictionary<K, V>()
		{
			if (RequireOrNull('{')) return null;
			var dict = new Dictionary<K, V>();
			// ReadValue might invoke a new serializer, so we must not rely on PutBack.
			if (SkipSpacesCarefully() == '}')
				Require('}');
			else {
				Func<string, object> rk;
				if (typeof(K).IsEnum)
					rk = s => Enum.Parse(typeof(K), s);
				else if (!keyParsers.TryGetValue(typeof(K), out rk))
					throw new YuzuAssert("Unable to find key parser for type: " + typeof(K).Name);

				var rf = ReadValueFunc(typeof(V));
				do {
					var key = RequireString();
					Require(':');
					dict.Add((K)rk(key), (V)rf());
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
					return ReadDictionary<string, object>();
				case '[':
					return ReadList<object>();
				default:
					return RequireDouble();
			}
		}

		// Optimization: Avoid creating trivial closures.
		private object RequireIntObj() { return RequireInt(); }
		private object RequireUIntObj() { return RequireUInt(); }
		private object RequireSByteObj() { return (sbyte)RequireInt(); }
		private object RequireByteObj() { return (byte)RequireInt(); }
		private object RequireStringObj() { return RequireString(); }
		private object RequireBoolObj() { return RequireBool(); }
		private object RequireSingleObj() { return RequireSingle(); }
		private object RequireDoubleObj() { return RequireDouble(); }
		private object RequireDateTimeObj() { return RequireDateTime(); }
		private object RequireTimeSpanObj() { return RequireTimeSpan(); }

		private Func<object> ReadValueFunc(Type t)
		{
			if (t == typeof(int))
				return RequireIntObj;
			if (t == typeof(uint))
				return RequireUIntObj;
			if (t == typeof(sbyte))
				return RequireSByteObj;
			if (t == typeof(byte))
				return RequireByteObj;
			if (t == typeof(string))
				return RequireStringObj;
			if (t == typeof(bool))
				return RequireBoolObj;
			if (t == typeof(float))
				return RequireSingleObj;
			if (t == typeof(double))
				return RequireDoubleObj;
			if (t == typeof(DateTime))
				return RequireDateTimeObj;
			if (t == typeof(TimeSpan))
				return RequireTimeSpanObj;
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
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), "ReadDictionary", t);
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

		protected void IgnoreNewFieldsTail(string name)
		{
			while (name != "") {
				ReadObject();
				name = GetNextName(false);
			}
		}

		protected int IgnoreNewFields(string tag, ref string name)
		{
			var cmp = String.CompareOrdinal(tag, name);
			if (Options.IgnoreNewFields && Options.TagMode != TagMode.Names)
				while (cmp > 0 && name != "") {
					ReadObject();
					name = GetNextName(false);
					cmp = String.CompareOrdinal(tag, name);
				}
			return cmp;
		}

		protected virtual object ReadFields(object obj, string name)
		{
			// Optimization: duplicate loop to extract options check.
			if (Options.IgnoreNewFields && Options.TagMode != TagMode.Names) {
				foreach (var yi in Meta.Get(obj.GetType(), Options).Items) {
					if (IgnoreNewFields(yi.Tag(Options), ref name) != 0) {
						if (!yi.IsOptional)
							throw Error("Expected field '{0}', but found '{1}'", yi.NameTagged(Options), name);
						continue;
					}
					yi.SetValue(obj, ReadValueFunc(yi.Type)());
					name = GetNextName(false);
				}
			}
			else {
				foreach (var yi in Meta.Get(obj.GetType(), Options).Items) {
					if (yi.Tag(Options) != name) {
						if (!yi.IsOptional)
							throw Error("Expected field '{0}', but found '{1}'", yi.NameTagged(Options), name);
						continue;
					}
					yi.SetValue(obj, ReadValueFunc(yi.Type)());
					name = GetNextName(false);
				}
			}
			if (Options.IgnoreNewFields)
				IgnoreNewFieldsTail(name);
			Require('}');
			return obj;
		}

		protected virtual object ReadFieldsCompact(object obj)
		{
			if (!Utils.IsCompact(obj.GetType(), Options))
				throw Error("Attempt to read non-compact type '{0}' from compact format", obj.GetType().Name);
			bool isFirst = true;
			foreach (var yi in Meta.Get(obj.GetType(), Options).Items) {
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
			// HACK: We can not modify the object we are given, so return a new one instead.
			if (obj.GetType() == typeof(object))
				return ReadObject();
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

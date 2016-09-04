using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Json
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

		private bool saveRootClass = false;
		public bool SaveRootClass { get { return saveRootClass; } set { saveRootClass = value; generation++; } }

		private bool ignoreCompact = false;
		public bool IgnoreCompact { get { return ignoreCompact; } set { ignoreCompact = value; generation++; } }

		public string DateFormat = "O";
		public string TimeSpanFormat = "c";

		private bool int64AsString = false;
		public bool Int64AsString { get { return int64AsString; } set { int64AsString = value; generation++; } }

		private bool decimalAsString = false;
		public bool DecimalAsString { get { return decimalAsString; } set { decimalAsString = value; generation++; } }

		public bool Unordered = false;
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

		private int depth = 0;

		private void WriteIndent()
		{
			if (JsonOptions.Indent == String.Empty)
				return;
			for (int i = 0; i < depth; ++i)
				WriteStr(JsonOptions.Indent);
		}

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

		private void WriteDecimal(object obj)
		{
			WriteStr(((decimal)obj).ToString(CultureInfo.InvariantCulture));
		}

		private void WriteDecimalAsString(object obj)
		{
			WriteUnescapedString(((decimal)obj).ToString(CultureInfo.InvariantCulture));
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

		private void WriteNullableEscapedString(object obj)
		{
			if (obj == null) {
				WriteStr("null");
				return;
			}
			WriteEscapedString(obj);
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

		private void WriteCollection<T>(ICollection<T> list)
		{
			if (list == null) {
				WriteStr("null");
				return;
			}
			var wf = GetWriteFunc(typeof(T));
			writer.Write('[');
			if (list.Count > 0) {
				try {
					depth += 1;
					var isFirst = true;
					foreach (var elem in list) {
						if (!isFirst)
							writer.Write(',');
						isFirst = false;
						WriteStr(JsonOptions.FieldSeparator);
						WriteIndent();
						wf(elem);
					}
					WriteStr(JsonOptions.FieldSeparator);
				}
				finally {
					depth -= 1;
				}
			}
			WriteIndent();
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
				try {
					depth += 1;
					WriteStr(JsonOptions.FieldSeparator);
					var isFirst = true;
					foreach (var elem in dict) {
						WriteSep(ref isFirst);
						WriteIndent();
						// TODO: Option to not escape dictionary keys.
						WriteEscapedString(elem.Key.ToString());
						writer.Write(':');
						wf(elem.Value);
					}
					WriteStr(JsonOptions.FieldSeparator);
				}
				finally {
					depth -= 1;
				}
			}
			WriteIndent();
			writer.Write('}');
		}

		private void WriteArray<T>(T[] array)
		{
			if (array == null) {
				WriteStr("null");
				return;
			}
			var wf = GetWriteFunc(typeof(T));
			writer.Write('[');
			if (array.Length > 0) {
				try {
					depth += 1;
					if (JsonOptions.ArrayLengthPrefix) {
						WriteIndent();
						WriteStr(array.Length.ToString());
					}
					var isFirst = !JsonOptions.ArrayLengthPrefix;
					foreach (var elem in array) {
						if (!isFirst)
							writer.Write(',');
						isFirst = false;
						WriteStr(JsonOptions.FieldSeparator);
						WriteIndent();
						wf(elem);
					}
					WriteStr(JsonOptions.FieldSeparator);
				}
				finally {
					depth -= 1;
				}
			}
			WriteIndent();
			writer.Write(']');
		}

		// List<object>
		private void WriteAny(object obj)
		{
			var t = obj.GetType();
			if (t == typeof(object))
				throw new YuzuException("WriteAny");
			if (t.IsClass || Utils.IsStruct(t))
				WriteObject<object>(obj);
			else
				GetWriteFunc(t)(obj);
		}

		private Stack<object> objStack = new Stack<object>();

		private void WriteAction(object obj)
		{
			if (obj == null) {
				WriteStr("null");
				return;
			}
			var a = obj as MulticastDelegate;
			if (a.Target != objStack.Peek())
				throw new NotImplementedException();
			WriteUnescapedString(a.Method.Name);
		}

		private void WriteNullable(object obj, Action<object> normalWrite)
		{
			if (obj == null)
				WriteStr("null");
			else
				normalWrite(obj);
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
			if (
				t == typeof(int) || t == typeof(uint) ||
				t == typeof(byte) || t == typeof(sbyte) ||
				t == typeof(short) || t == typeof(ushort)
			)
				return WriteInt;
			if (t == typeof(long) || t == typeof(ulong)) {
				if (JsonOptions.Int64AsString)
					return WriteUnescapedString;
				else
					return WriteInt;
			}
			if (t == typeof(double))
				return WriteDouble;
			if (t == typeof(decimal))
				if (JsonOptions.DecimalAsString)
					return WriteDecimalAsString;
				else
					return WriteDecimal;
			if (t == typeof(float))
				return WriteSingle;
			if (t == typeof(char))
				return WriteEscapedString;
			if (t == typeof(string))
				return WriteNullableEscapedString;
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
			if (t == typeof(object))
				return WriteAny;
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(Dictionary<,>)) {
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), "WriteDictionary", t);
					return obj => m.Invoke(this, new object[] { obj });
				}
				if (g == typeof(Action<>)) {
					return WriteAction;
				}
				if (g == typeof(Nullable<>)) {
					var w = GetWriteFunc(t.GetGenericArguments()[0]);
					return obj => WriteNullable(obj, w);
				}
			}
			if (t.IsArray) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteArray", t);
				return obj => m.Invoke(this, new object[] { obj });
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				Meta.Get(t, Options); // Check for serializable fields.
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "WriteCollection", icoll);
				return obj => m.Invoke(this, new object[] { obj });
			}
			if (Utils.IsStruct(t) || t.IsClass || t.IsInterface) {
				var name = Meta.Get(t, Options).IsCompact && !JsonOptions.IgnoreCompact ?
					"WriteObjectCompact" : "WriteObject";
				var m = Utils.GetPrivateGeneric(GetType(), name, t);
				return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), this, m);
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
			WriteIndent();
			WriteUnescapedString(name);
			writer.Write(':');
		}

		private void WriteObject<T>(object obj)
		{
			if (obj == null) {
				WriteStr("null");
				return;
			}
			writer.Write('{');
			WriteStr(JsonOptions.FieldSeparator);
			objStack.Push(obj);
			try {
				depth += 1;
				var isFirst = true;
				var actualType = obj.GetType();
				if (typeof(T) != actualType || objStack.Count == 1 && JsonOptions.SaveRootClass) {
					WriteName(JsonOptions.ClassTag, ref isFirst);
					WriteUnescapedString(TypeSerializer.Serialize(actualType));
				}
				foreach (var yi in Meta.Get(actualType, Options).Items) {
					var value = yi.GetValue(obj);
					if (yi.SerializeIf != null && !yi.SerializeIf(obj, value))
						continue;
					WriteName(yi.Tag(Options), ref isFirst);
					GetWriteFunc(yi.Type)(value);
				}
				if (!isFirst)
					WriteStr(JsonOptions.FieldSeparator);
			}
			finally {
				depth -= 1;
				objStack.Pop();
			}
			WriteIndent();
			writer.Write('}');
		}

		private void WriteObjectCompact<T>(object obj)
		{
			if (obj == null) {
				WriteStr("null");
				return;
			}
			writer.Write('[');
			WriteStr(JsonOptions.FieldSeparator);
			var isFirst = true;
			var actualType = obj.GetType();
			if (typeof(T) != actualType)
				throw new YuzuException(String.Format(
					"Attempt to write compact type {0} instead of {1}", actualType.Name, typeof(T).Name));
			objStack.Push(obj);
			try {
				depth += 1;
				foreach (var yi in Meta.Get(actualType, Options).Items) {
					WriteSep(ref isFirst);
					WriteIndent();
					GetWriteFunc(yi.Type)(yi.GetValue(obj));
				}
			}
			finally {
				depth -= 1;
				objStack.Pop();
			};
			if (!isFirst)
				WriteStr(JsonOptions.FieldSeparator);
			WriteIndent();
			writer.Write(']');
		}

		protected override void ToWriter(object obj) { GetWriteFunc(obj.GetType())(obj); }
	}

}

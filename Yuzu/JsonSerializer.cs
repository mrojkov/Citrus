using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

using Yuzu.Grisu;
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

		private int maxOnelineFields = 0;
		public int MaxOnelineFields { get { return maxOnelineFields; } set { maxOnelineFields = value; generation++; } }

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

		private bool comments = false;
		public bool Comments { get { return comments; } set { comments = value; generation++; } }

		public bool BOM = false;
	};

	public class JsonSerializer : AbstractWriterSerializer
	{
		public JsonSerializeOptions JsonOptions = new JsonSerializeOptions();

		public JsonSerializer() { InitWriters(); }

		private int depth = 0;

		private byte[] nullBytes = new byte[] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };

		private Dictionary<string, byte[]> strCache = new Dictionary<string, byte[]>();
		private byte[] StrToBytesCached(string s)
		{
			byte[] b;
			if (!strCache.TryGetValue(s, out b)) {
				b = Encoding.UTF8.GetBytes(s);
				strCache[s] = b;
			}
			return b;
		}

		private void WriteStr(string s) => writer.Write(Encoding.UTF8.GetBytes(s));
		private void WriteStrCached(string s) => writer.Write(StrToBytesCached(s));

		private void WriteFieldSeparator()
		{
			if (JsonOptions.FieldSeparator != String.Empty)
				WriteStrCached(JsonOptions.FieldSeparator);
		}

		private void WriteIndent()
		{
			if (JsonOptions.Indent == String.Empty)
				return;
			var b = StrToBytesCached(JsonOptions.Indent);
			for (int i = 0; i < depth; ++i)
				writer.Write(b);
		}

		private void WriteInt(object obj) => JsonIntWriter.WriteInt(writer, obj);
		private void WriteUInt(object obj) => JsonIntWriter.WriteUInt(writer, obj);
		private void WriteLong(object obj) => JsonIntWriter.WriteLong(writer, obj);
		private void WriteULong(object obj) => JsonIntWriter.WriteULong(writer, obj);

		private void WriteLongAsString(object obj)
		{
			writer.Write((byte)'"');
			JsonIntWriter.WriteLong(writer, obj);
			writer.Write((byte)'"');
		}

		private void WriteULongAsString(object obj)
		{
			writer.Write((byte)'"');
			JsonIntWriter.WriteULong(writer, obj);
			writer.Write((byte)'"');
		}

		//WriteStr(((double)obj).ToString("R", CultureInfo.InvariantCulture));
		private void WriteDouble(object obj) => DoubleWriter.Write((double)obj, writer);

		//WriteStr(((float)obj).ToString("R", CultureInfo.InvariantCulture));
		private void WriteSingle(object obj) => DoubleWriter.Write((float)obj, writer);

		private void WriteDecimal(object obj) =>
			WriteStr(((decimal)obj).ToString(CultureInfo.InvariantCulture));

		private void WriteDecimalAsString(object obj) =>
			WriteUnescapedString(((decimal)obj).ToString(CultureInfo.InvariantCulture));

		private void WriteEnumAsInt(object obj) => WriteStrCached(((int)obj).ToString());

		private void WriteUnescapedString(object obj)
		{
			writer.Write((byte)'"');
			WriteStrCached(obj.ToString());
			writer.Write((byte)'"');
		}

		private void WriteEscapedString(object obj)
		{
			writer.Write((byte)'"');
			foreach (var ch in obj.ToString()) {
				var escape = ch <= '\\' ? JsonEscapeData.escapeChars[ch] : '\0';
				if (escape > 0) {
					writer.Write((byte)'\\');
					writer.Write(escape);
				}
				else if (ch < ' ') {
					writer.Write((byte)'\\');
					writer.Write((byte)'u');
					for (int i = 3 * 4; i >= 0; i -= 4)
						writer.Write(JsonEscapeData.digitHex[ch >> i & 0xf]);
				}
				else {
					writer.Write(ch);
				}
			}
			writer.Write((byte)'"');
		}

		private void WriteNullableEscapedString(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			WriteEscapedString(obj);
		}

		private void WriteBool(object obj) => WriteStrCached((bool)obj ? "true" : "false");

		private static byte[] localTimeZone = Encoding.ASCII.GetBytes(DateTime.Now.ToString("%K"));

		private void WriteDateTime(object obj)
		{
			var d = (DateTime)obj;
			// 'Roundtrip' format is guaranteed to be ASCII-clean.
			if (JsonOptions.DateFormat == "O") {
				writer.Write((byte)'"');
				JsonIntWriter.WriteInt4Digits(writer, d.Year);
				writer.Write((byte)'-');
				JsonIntWriter.WriteInt2Digits(writer, d.Month);
				writer.Write((byte)'-');
				JsonIntWriter.WriteInt2Digits(writer, d.Day);
				writer.Write((byte)'T');
				JsonIntWriter.WriteInt2Digits(writer, d.Hour);
				writer.Write((byte)':');
				JsonIntWriter.WriteInt2Digits(writer, d.Minute);
				writer.Write((byte)':');
				JsonIntWriter.WriteInt2Digits(writer, d.Second);
				writer.Write((byte)'.');
				JsonIntWriter.WriteInt7Digits(writer, (int)(d.Ticks % TimeSpan.TicksPerSecond));
				switch (d.Kind) {
					case DateTimeKind.Local:
						writer.Write(localTimeZone);
						break;
					case DateTimeKind.Unspecified:
						break;
					case DateTimeKind.Utc:
						writer.Write((byte)'Z');
						break;
				}
				writer.Write((byte)'"');
			}
			else
				WriteEscapedString(d.ToString(JsonOptions.DateFormat, CultureInfo.InvariantCulture));
		}

		private void WriteTimeSpan(object obj)
		{
			var t = (TimeSpan)obj;
			// 'Constant' format is guaranteed to be ASCII-clean.
			if (JsonOptions.TimeSpanFormat == "c") {
				writer.Write((byte)'"');
				if (t.Ticks < 0) {
					writer.Write((byte)'-');
					t = t.Duration();
				}
				var d = t.Days;
				if (d > 0) {
					JsonIntWriter.WriteInt(writer, d);
					writer.Write((byte)'.');
				}
				JsonIntWriter.WriteInt2Digits(writer, t.Hours);
				writer.Write((byte)':');
				JsonIntWriter.WriteInt2Digits(writer, t.Minutes);
				writer.Write((byte)':');
				JsonIntWriter.WriteInt2Digits(writer, t.Seconds);
				var f = (int)(t.Ticks % TimeSpan.TicksPerSecond);
				if (f > 0) {
					writer.Write((byte)'.');
					JsonIntWriter.WriteInt7Digits(writer, f);
				}
				writer.Write((byte)'"');
			}
			else
				WriteEscapedString(t.ToString(JsonOptions.TimeSpanFormat, CultureInfo.InvariantCulture));
		}

		private void WriteIEnumerable<T>(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var list = (IEnumerable<T>)obj;
			var wf = GetWriteFunc(typeof(T));
			writer.Write((byte)'[');
			try {
				depth += 1;
				var isFirst = true;
				foreach (var elem in list) {
					if (!isFirst)
						writer.Write((byte)',');
					isFirst = false;
					WriteFieldSeparator();
					WriteIndent();
					wf(elem);
				}
				if (!isFirst)
					WriteFieldSeparator();
			}
			finally {
				depth -= 1;
			}
			WriteIndent();
			writer.Write((byte)']');
		}

		private void WriteDictionary<K, V>(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var dict = (Dictionary<K, V>)obj;
			var wf = GetWriteFunc(typeof(V));
			writer.Write((byte)'{');
			if (dict.Count > 0) {
				try {
					depth += 1;
					WriteFieldSeparator();
					var isFirst = true;
					foreach (var elem in dict) {
						WriteSep(ref isFirst);
						WriteIndent();
						// TODO: Option to not escape dictionary keys.
						WriteEscapedString(elem.Key.ToString());
						writer.Write((byte)':');
						wf(elem.Value);
					}
					WriteFieldSeparator();
				}
				finally {
					depth -= 1;
				}
			}
			WriteIndent();
			writer.Write((byte)'}');
		}

		private void WriteArray<T>(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var array = (T[])obj;
			var wf = GetWriteFunc(typeof(T));
			writer.Write((byte)'[');
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
							writer.Write((byte)',');
						isFirst = false;
						WriteFieldSeparator();
						WriteIndent();
						wf(elem);
					}
					WriteFieldSeparator();
				}
				finally {
					depth -= 1;
				}
			}
			WriteIndent();
			writer.Write((byte)']');
		}

		// List<object>
		private void WriteAny(object obj)
		{
			var t = obj.GetType();
			if (t == typeof(object))
				throw new YuzuException("WriteAny of unknown type");
			if (IsUserObject(t)) {
				var meta = Meta.Get(t, Options);
				var sg = meta.Surrogate;
				Action<object> surrogateWriter = GetSurrogateWriter(meta);
				if (sg.FuncTo == null || (sg.FuncIf != null && !sg.FuncIf(obj)))
					// Ignore compact since class name is always required.
					WriteObject(obj, null);
				else
					surrogateWriter(sg.FuncTo(obj));
			}
			else
				GetWriteFunc(t)(obj);
		}

		private Stack<object> objStack = new Stack<object>();

		private void WriteAction(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
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
				writer.Write(nullBytes);
			else
				normalWrite(obj);
		}

		private void WriteUnknown(object obj)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var u = (YuzuUnknown)obj;
			writer.Write((byte)'{');
			WriteFieldSeparator();
			objStack.Push(obj);
			try {
				depth += 1;
				var isFirst = true;
				WriteName(JsonOptions.ClassTag, ref isFirst);
				WriteUnescapedString(u.ClassTag);
				foreach (var f in u.Fields) {
					WriteName(f.Key, ref isFirst);
					GetWriteFunc(f.Value.GetType())(f.Value);
				}
				if (!isFirst)
					WriteFieldSeparator();
			}
			finally {
				depth -= 1;
				objStack.Pop();
			}
			WriteIndent();
			writer.Write((byte)'}');
		}

		private bool IsOneline(Meta meta)
		{
			var r = meta.CountPrimitiveChildren(Options);
			return r != Meta.FoundNonPrimitive && r <= JsonOptions.MaxOnelineFields;
		}

		private Action<object> MakeDelegate(MethodInfo m) =>
			(Action<object>)Delegate.CreateDelegate(typeof(Action<object>), this, m);

		private Dictionary<Type, Action<object>> writerCache = new Dictionary<Type, Action<object>>();
		private int jsonOptionsGeneration = 0;

		private void InitWriters()
		{
			writerCache[typeof(sbyte)] = WriteInt;
			writerCache[typeof(byte)] = WriteUInt;
			writerCache[typeof(short)] = WriteInt;
			writerCache[typeof(ushort)] = WriteUInt;
			writerCache[typeof(int)] = WriteInt;
			writerCache[typeof(uint)] = WriteUInt;
			if (JsonOptions.Int64AsString) {
				writerCache[typeof(long)] = WriteLongAsString;
				writerCache[typeof(ulong)] = WriteULongAsString;
			}
			else {
				writerCache[typeof(long)] = WriteLong;
				writerCache[typeof(ulong)] = WriteULong;
			}
			writerCache[typeof(bool)] = WriteBool;
			writerCache[typeof(char)] = WriteEscapedString;
			writerCache[typeof(float)] = WriteSingle;
			writerCache[typeof(double)] = WriteDouble;
			if (JsonOptions.DecimalAsString)
				writerCache[typeof(decimal)] = WriteDecimalAsString;
			else
				writerCache[typeof(decimal)] = WriteDecimal;
			writerCache[typeof(DateTime)] = WriteDateTime;
			writerCache[typeof(TimeSpan)] = WriteTimeSpan;
			writerCache[typeof(string)] = WriteNullableEscapedString;
			writerCache[typeof(object)] = WriteAny;
			writerCache[typeof(YuzuUnknown)] = WriteUnknown;
		}

		private Action<object> GetWriteFunc(Type t)
		{
			if (jsonOptionsGeneration != JsonOptions.Generation) {
				writerCache.Clear();
				InitWriters();
				jsonOptionsGeneration = JsonOptions.Generation;
			}

			Action<object> result;
			if (writerCache.TryGetValue(t, out result))
				return result;
			result = MakeWriteFunc(t);
			writerCache[t] = result;
			return result;
		}

		// This condition must be equivalent to MakeWriteFunc returning WriteObject*.
		private bool IsUserObject(Type t)
		{
			if (t == typeof(string)) return false;
			if (Utils.IsStruct(t)) return true;
			if (!t.IsClass && !t.IsInterface) return false;
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(Dictionary<,>) || g == typeof(Action<>) || g == typeof(Nullable<>))
					return false;
			}
			if (Utils.GetICollection(t) != null)
				return false;
			return true;
		}

		private Action<object> GetSurrogateWriter(Meta meta)
		{
			var sg = meta.Surrogate;
			if (sg.FuncTo == null)
				return null;
			var st = sg.SurrogateType;
			if (!IsUserObject(st))
				return GetWriteFunc(st);
			var sgMeta = Meta.Get(st, Options);
			// Unpack conditional compact surrogate for compact field to allow detection.
			if (!sgMeta.IsCompact || JsonOptions.IgnoreCompact || meta.IsCompact && sg.FuncIf != null)
				return obj => WriteObject(obj, meta);
			if (IsOneline(sgMeta))
				return obj => WriteObjectCompactOneline(obj, sgMeta);
			else
				return obj => WriteObjectCompact(obj, sgMeta);
		}

		private Action<object> MakeWriteFunc(Type t)
		{
			if (t.IsEnum) {
				if (JsonOptions.EnumAsString)
					return WriteUnescapedString;
				else
					return WriteEnumAsInt;
			}
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(Dictionary<,>))
					return MakeDelegate(Utils.GetPrivateCovariantGenericAll(GetType(), nameof(WriteDictionary), t));
				if (g == typeof(Action<>)) {
					return WriteAction;
				}
				if (g == typeof(Nullable<>)) {
					var w = GetWriteFunc(t.GetGenericArguments()[0]);
					return obj => WriteNullable(obj, w);
				}
			}
			if (t.IsArray)
				return MakeDelegate(Utils.GetPrivateCovariantGeneric(GetType(), nameof(WriteArray), t));
			var ienum = Utils.GetIEnumerable(t);
			if (ienum != null) {
				Meta.Get(t, Options); // Check for serializable fields.
				return MakeDelegate(Utils.GetPrivateCovariantGeneric(GetType(), nameof(WriteIEnumerable), ienum));
			}
			if (t.IsSubclassOf(typeof(YuzuUnknown)))
				return WriteUnknown;
			if (Utils.IsStruct(t) || t.IsClass || t.IsInterface) {
				var meta = Meta.Get(t, Options);
				var sg = meta.Surrogate;
				Action<object> surrogateWriter = GetSurrogateWriter(meta);
				if (sg.FuncTo != null && sg.FuncIf == null)
					return obj => surrogateWriter(sg.FuncTo(obj));

				Action<object> writeFunc;
				if (!meta.IsCompact || JsonOptions.IgnoreCompact)
					writeFunc = obj => WriteObject(obj, meta);
				else if (IsOneline(meta))
					writeFunc = obj => WriteObjectCompactOneline(obj, meta);
				else
					writeFunc = obj => WriteObjectCompact(obj, meta);

				if (sg.FuncTo != null && sg.FuncIf != null)
					return obj => {
						if (sg.FuncIf(obj))
							surrogateWriter(sg.FuncTo(obj));
						else
							writeFunc(obj);
					};

				return writeFunc;
			}
			throw new NotImplementedException(t.Name);
		}

		private void WriteSep(ref bool isFirst)
		{
			if (!isFirst) {
				writer.Write((byte)',');
				WriteFieldSeparator();
			}
			isFirst = false;
		}

		private void WriteName(string name, ref bool isFirst)
		{
			WriteSep(ref isFirst);
			WriteIndent();
			WriteUnescapedString(name);
			writer.Write((byte)':');
		}

		private void WriteUnknownStorageItem(YuzuUnknownStorage.Item item, ref bool isFirst)
		{
			WriteName(item.Name, ref isFirst);
			GetWriteFunc(item.Value.GetType())(item.Value);
		}

		private void WriteObject(object obj, Meta meta)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var expectedType = meta == null ? null : meta.Type;
			var actualType = obj.GetType();
			if (meta == null || meta.Type != actualType)
				meta = Meta.Get(actualType, Options);
			meta.BeforeSerialization.Run(obj);
			writer.Write((byte)'{');
			WriteFieldSeparator();
			objStack.Push(obj);
			try {
				depth += 1;
				var isFirst = true;
				if (
					expectedType != actualType || objStack.Count == 1 && JsonOptions.SaveRootClass ||
					meta.WriteAlias != null
				) {
					WriteName(JsonOptions.ClassTag, ref isFirst);
					WriteUnescapedString(meta.WriteAlias ?? TypeSerializer.Serialize(actualType));
				}
				var storage = meta.GetUnknownStorage == null ? null : meta.GetUnknownStorage(obj);
				// Duplicate code to optimize fast-path without unknown storage.
				if (storage == null || storage.Fields.Count == 0 || JsonOptions.Unordered) {
					foreach (var yi in meta.Items) {
						var value = yi.GetValue(obj);
						if (yi.SerializeIf != null && !yi.SerializeIf(obj, value))
							continue;
						WriteName(yi.Tag(Options), ref isFirst);
						GetWriteFunc(yi.Type)(value);
					}
					// If Unordered, dump all unknown fields after all known ones.
					if (storage != null)
						for (var storageIndex = 0; storageIndex < storage.Fields.Count; ++storageIndex)
							WriteUnknownStorageItem(storage.Fields[storageIndex], ref isFirst);
				}
				else {
					// Merge unknown and known fields.
					storage.Sort();
					var storageIndex = 0;
					foreach (var yi in meta.Items) {
						var value = yi.GetValue(obj);
						if (yi.SerializeIf != null && !yi.SerializeIf(obj, value))
							continue;
						var name = yi.Tag(Options);
						for (; storageIndex < storage.Fields.Count; ++storageIndex) {
							var si = storage.Fields[storageIndex];
							if (String.CompareOrdinal(si.Name, name) >= 0)
								break;
							WriteUnknownStorageItem(si, ref isFirst);
						}
						WriteName(name, ref isFirst);
						GetWriteFunc(yi.Type)(value);
					}
					for (; storageIndex < storage.Fields.Count; ++storageIndex)
						WriteUnknownStorageItem(storage.Fields[storageIndex], ref isFirst);
				}
				if (!isFirst)
					WriteFieldSeparator();
			}
			finally {
				depth -= 1;
				objStack.Pop();
			}
			WriteIndent();
			writer.Write((byte)'}');
		}

		private void WriteObjectCompact(object obj, Meta meta)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var actualType = obj.GetType();
			if (meta.Type != actualType)
				throw new YuzuException(String.Format(
					"Attempt to write compact type {0} instead of {1}", actualType.Name, meta.Type.Name));
			meta.BeforeSerialization.Run(obj);
			writer.Write((byte)'[');
			WriteFieldSeparator();
			var isFirst = true;
			objStack.Push(obj);
			try {
				depth += 1;
				foreach (var yi in meta.Items) {
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
				WriteFieldSeparator();
			WriteIndent();
			writer.Write((byte)']');
		}

		private void WriteObjectCompactOneline(object obj, Meta meta)
		{
			if (obj == null) {
				writer.Write(nullBytes);
				return;
			}
			var actualType = obj.GetType();
			if (meta.Type != actualType)
				throw new YuzuException(String.Format(
					"Attempt to write compact type {0} instead of {1}", actualType.Name, meta.Type.Name));
			meta.BeforeSerialization.Run(obj);
			writer.Write((byte)'[');
			var isFirst = true;
			objStack.Push(obj);
			try {
				foreach (var yi in meta.Items) {
					if (!isFirst)
						writer.Write((byte)',');
					isFirst = false;
					GetWriteFunc(yi.Type)(yi.GetValue(obj));
				}
			}
			finally {
				objStack.Pop();
			};
			writer.Write((byte)']');
		}

		protected override void ToWriter(object obj) => GetWriteFunc(obj.GetType())(obj);
	}

}

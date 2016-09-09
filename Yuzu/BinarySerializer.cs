using System;
using System.Collections.Generic;
using System.Globalization;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinarySerializeOptions
	{
		public byte[] Signature = new byte[] { (byte)'Y', (byte)'B', (byte)'0', (byte)'1' };
		public bool AutoSignature = false;
	}

	// These values are part of format.
	public enum RoughType: byte
	{
		None      =  0,
		SByte     =  1,
		Byte      =  2,
		Short     =  3,
		UShort    =  4,
		Int       =  5,
		UInt      =  6,
		Long      =  7,
		ULong     =  8,
		Bool      =  9,
		Char      = 10,
		Float     = 11,
		Double    = 12,
		Decimal   = 13,
		DateTime  = 14,
		TimeSpan  = 15,
		String    = 16,
		Any       = 17,
		Nullable  = 18,

		Record    = 32,
		Sequence  = 33,
		Mapping   = 34,

		FirstAtom = SByte,
		LastAtom  = Any,
	}

	internal static class RT
	{
		public static Type[] roughTypeToType = new Type[] {
			null,
			typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
			typeof(int), typeof(uint), typeof(long), typeof(ulong),
			typeof(bool), typeof(char), typeof(float), typeof(double), typeof(decimal),
			typeof(DateTime), typeof(TimeSpan), typeof(string),
			typeof(object),
		};

		public static bool IsRecord(this Type t)
		{
			return t.IsClass || t.IsInterface || Utils.IsStruct(t);
		}
	}

	public class BinarySerializer : AbstractWriterSerializer
	{
		public BinarySerializeOptions BinaryOptions = new BinarySerializeOptions();

		protected void WriteSByte(object obj) { writer.Write((sbyte)obj); }
		protected void WriteByte(object obj) { writer.Write((byte)obj); }
		protected void WriteShort(object obj) { writer.Write((short)obj); }
		protected void WriteUShort(object obj) { writer.Write((ushort)obj); }
		protected void WriteInt(object obj) { writer.Write((int)obj); }
		protected void WriteUInt(object obj) { writer.Write((uint)obj); }
		protected void WriteLong(object obj) { writer.Write((long)obj); }
		protected void WriteULong(object obj) { writer.Write((ulong)obj); }
		protected void WriteBool(object obj) { writer.Write((bool)obj); }
		protected void WriteChar(object obj) { writer.Write((char)obj); }
		protected void WriteFloat(object obj) { writer.Write((float)obj); }
		protected void WriteDouble(object obj) { writer.Write((double)obj); }
		protected void WriteDecimal(object obj) { writer.Write((decimal)obj); }

		protected void WriteDateTime(object obj) { writer.Write(((DateTime)obj).ToBinary()); }
		protected void WriteTimeSpan(object obj) { writer.Write(((TimeSpan)obj).Ticks); }

		protected void WriteString(object obj) {
			if (obj == null) {
				writer.Write("");
				writer.Write(true);
				return;
			}
			writer.Write((string)obj);
			if ((string)obj == "") {
				writer.Write(false);
			}
		}

		protected void WriteAny(object obj)
		{
			var t = obj.GetType();
			WriteRoughType(t);
			GetWriteFunc(t)(obj);
		}

		private Dictionary<Type, Action<object>> writerCache = new Dictionary<Type, Action<object>>();

		private Action<object> GetWriteFunc(Type t)
		{
			Action<object> result;
			if (writerCache.TryGetValue(t, out result))
				return result;
			result = MakeWriteFunc(t);
			writerCache[t] = result;
			return result;
		}

		public BinarySerializer()
		{
			InitWriters();
		}

		private void InitWriters()
		{
			writerCache[typeof(sbyte)] = WriteSByte;
			writerCache[typeof(byte)] = WriteByte;
			writerCache[typeof(short)] = WriteShort;
			writerCache[typeof(ushort)] = WriteUShort;
			writerCache[typeof(int)] = WriteInt;
			writerCache[typeof(uint)] = WriteUInt;
			writerCache[typeof(long)] = WriteLong;
			writerCache[typeof(ulong)] = WriteULong;
			writerCache[typeof(bool)] = WriteBool;
			writerCache[typeof(char)] = WriteChar;
			writerCache[typeof(float)] = WriteFloat;
			writerCache[typeof(double)] = WriteDouble;
			writerCache[typeof(decimal)] = WriteDecimal;
			writerCache[typeof(DateTime)] = WriteDateTime;
			writerCache[typeof(TimeSpan)] = WriteTimeSpan;
			writerCache[typeof(string)] = WriteString;
			writerCache[typeof(object)] = WriteAny;
		}

		private void WriteRoughType(Type t)
		{
			for (var result = RoughType.FirstAtom; result <= RoughType.LastAtom; ++result)
				if (t == RT.roughTypeToType[(int)result]) {
					writer.Write((byte)result);
					return;
				}
			if (t.IsEnum) {
				writer.Write((byte)RoughType.Int);
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				writer.Write((byte)RoughType.Mapping);
				var g = t.GetGenericArguments();
				WriteRoughType(g[0]);
				WriteRoughType(g[1]);
				return;
			}
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				writer.Write((byte)RoughType.Nullable);
				WriteRoughType(t.GetGenericArguments()[0]);
				return;
			}
			if (t.IsArray) {
				writer.Write((byte)RoughType.Sequence);
				WriteRoughType(t.GetElementType());
				return;
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				writer.Write((byte)RoughType.Sequence);
				WriteRoughType(icoll.GetGenericArguments()[0]);
				return;
			}
			if (t.IsRecord()) {
				writer.Write((byte)RoughType.Record);
				return;
			}
			throw new NotImplementedException();
		}

		private void WriteDictionary<K, V>(Dictionary<K, V> dict)
		{
			if (dict == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(dict.Count);
			var wk = GetWriteFunc(typeof(K));
			var wv = GetWriteFunc(typeof(V));
			foreach (var elem in dict) {
				wk(elem.Key);
				wv(elem.Value);
			}
		}

		private Stack<object> objStack = new Stack<object>();

		private void WriteAction(object obj)
		{
			if (obj == null) {
				writer.Write("");
				return;
			}
			var a = obj as MulticastDelegate;
			if (a.Target != objStack.Peek())
				throw new NotImplementedException();
			writer.Write(a.Method.Name);
		}

		private void WriteArray<T>(T[] arr)
		{
			if (arr == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(arr.Length);
			var wf = GetWriteFunc(typeof(T));
			foreach (var a in arr)
				wf(a);
		}

		private void WriteCollection<T>(ICollection<T> list)
		{
			if (list == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(list.Count);
			var wf = GetWriteFunc(typeof(T));
			foreach (var a in list)
				wf(a);
		}

		protected class ClassDef
		{
			public struct FieldDef
			{
				public Action<object> WriteFunc;
				public Action<object> WriteFuncCompact;
			}
			public short Id;
			internal Meta Meta;
			public List<FieldDef> Fields = new List<FieldDef>();
		}
		private Dictionary<Type, ClassDef> classIdCache = new Dictionary<Type, ClassDef>();

		public void ClearClassIds() { classIdCache.Clear(); }

		private ClassDef WriteClassId(Type t)
		{
			ClassDef result;
			if (classIdCache.TryGetValue(t, out result)) {
				writer.Write(result.Id);
				return result;
			}

			result = new ClassDef { Id = (short)(classIdCache.Count + 1) };
			result.Meta = Meta.Get(t, Options);
			classIdCache[t] = result;
			writer.Write(result.Id);
			writer.Write(TypeSerializer.Serialize(result.Meta.Type));
			writer.Write((short)result.Meta.Items.Count);
			for (short i = 0; i < result.Meta.Items.Count; ++i) {
				var yi = result.Meta.Items[i];
				writer.Write(yi.Tag(Options));
				WriteRoughType(yi.Type);
				short j = (short)(i + 1); // Capture.
				var wf = GetWriteFunc(yi.Type);
				var fd = new ClassDef.FieldDef();
				if (yi.SerializeIf != null)
					fd.WriteFunc = obj => {
						var value = yi.GetValue(obj);
						if (!yi.SerializeIf(obj, value))
							return;
						writer.Write(j);
						wf(value);
					};
				else
					fd.WriteFunc = obj => {
						writer.Write(j);
						wf(yi.GetValue(obj));
					};
				fd.WriteFuncCompact = obj => wf(yi.GetValue(obj));
				result.Fields.Add(fd);
			}
			return result;
		}

		private void WriteObject<T>(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var def = WriteClassId(obj.GetType());
			objStack.Push(obj);
			try {
				foreach (var d in def.Fields)
					d.WriteFunc(obj);
				writer.Write((short)0);
			}
			finally {
				objStack.Pop();
			}
		}

		private void WriteObjectCompact<T>(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var def = WriteClassId(obj.GetType());
			objStack.Push(obj);
			try {
				foreach (var d in def.Fields)
					d.WriteFuncCompact(obj);
			}
			finally {
				objStack.Pop();
			}
		}

		private Action<object> MakeWriteFunc(Type t)
		{
			if (t.IsEnum) 
				return WriteInt;
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
					return obj => {
						writer.Write(obj == null);
						if (obj != null)
							w(obj);
					};
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
				var name = Meta.Get(t, Options).IsCompact ? "WriteObjectCompact" : "WriteObject";
				var m = Utils.GetPrivateGeneric(GetType(), name, t);
				return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), this, m);
			}
			throw new NotImplementedException(t.Name);
		}

		protected override void ToWriter(object obj)
		{
			if (BinaryOptions.AutoSignature)
				WriteSignature();
			WriteAny(obj);
		}

		public void WriteSignature() { writer.Write(BinaryOptions.Signature); }
	}
}

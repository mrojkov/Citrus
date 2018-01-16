using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
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

		protected void WriteRecord(object obj) { GetWriteFunc(obj.GetType())(obj); }

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

			writerCache[typeof(Record)] = WriteRecord;
			writerCache[typeof(YuzuUnknown)] = WriteUnknown;
			writerCache[typeof(YuzuUnknownBinary)] = WriteUnknownBinary;
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
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(Dictionary<,>)) {
					writer.Write((byte)RoughType.Mapping);
					var a = t.GetGenericArguments();
					WriteRoughType(a[0]);
					WriteRoughType(a[1]);
					return;
				}
				if (g == typeof(Nullable<>)) {
					writer.Write((byte)RoughType.Nullable);
					WriteRoughType(t.GetGenericArguments()[0]);
					return;
				}
				if (g == typeof(IEnumerable<>)) {
					writer.Write((byte)RoughType.Sequence);
					WriteRoughType(t.GetGenericArguments()[0]);
					return;
				}
			}
			if (t.IsArray) {
				writer.Write((byte)RoughType.Sequence);
				WriteRoughType(t.GetElementType());
				return;
			}
			var ienum = Utils.GetIEnumerable(t);
			if (ienum != null) {
				writer.Write((byte)RoughType.Sequence);
				WriteRoughType(ienum.GetGenericArguments()[0]);
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

		private void WriteArray<T>(T[] arr, Action<object> wf)
		{
			if (arr == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(arr.Length);
			foreach (var a in arr)
				wf(a);
		}

		private void WriteIEnumerable<T>(IEnumerable<T> list, Action<object> wf)
		{
			if (list == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(list.Count());
			foreach (var a in list)
				wf(a);
		}

		// Duplicate WriteIEnumerable to optimize Count.
		private void WriteCollection<T>(ICollection<T> list, Action<object> wf)
		{
			if (list == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(list.Count);
			foreach (var a in list)
				wf(a);
		}

		private void WriteCollectionNG(object obj, Action<object> wf)
		{
			if (obj == null) {
				writer.Write(-1);
				return;
			}
			var list = (ICollection)obj;
			writer.Write(list.Count);
			foreach (var a in list)
				wf(a);
		}

		protected class ClassDef
		{
			public struct FieldDef
			{
				public string Name;
				public Type Type;
				public Action<object> WriteFunc;
				public Action<object> WriteFuncCompact;
				internal Action<object, YuzuUnknownStorage, BoxedInt> WriteFuncUnknown;
			}
			public short Id;
			internal Meta Meta;
			internal ReaderClassDef ReaderDef;
			public List<FieldDef> Fields = new List<FieldDef>();
		}
		private Dictionary<Type, ClassDef> classIdCache = new Dictionary<Type, ClassDef>();
		private Dictionary<string, ClassDef> unknownClassIdCache = new Dictionary<string, ClassDef>();

		public void ClearClassIds() { classIdCache.Clear(); }

		private void PrepareClassDefFields(ClassDef result)
		{
			for (short i = 0; i < result.Meta.Items.Count; ++i) {
				var yi = result.Meta.Items[i];
				short j = (short)(i + 1); // Capture.
				var wf = GetWriteFunc(yi.Type);
				var fd = new ClassDef.FieldDef { Name = yi.Tag(Options), Type = yi.Type };
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
		}

		private void PrepareClassDefFieldsUnknown(ClassDef result)
		{
			for (int ourIndex = 0, theirIndex = 1, i = 0; ; ++i) {
				var yi = ourIndex < result.Meta.Items.Count ? result.Meta.Items[ourIndex] : null;
				var their = theirIndex < result.ReaderDef.Fields.Count ? result.ReaderDef.Fields[theirIndex] : null;
				if (yi == null && their == null)
					break;

				short j = (short)(i + 1); // Capture.
				var ourName = yi == null ? null : yi.Tag(Options);
				var cmp = their == null ? -1 : yi == null ? 1 : String.CompareOrdinal(ourName, their.Name);
				if (cmp <= 0) {
					var wf = GetWriteFunc(yi.Type);
					var fd = new ClassDef.FieldDef { Name = ourName, Type = yi.Type };
					if (yi.SerializeIf != null)
						fd.WriteFuncUnknown = (obj, storage, storageIndex) => {
							var value = yi.GetValue(obj);
							if (!yi.SerializeIf(obj, value))
								return;
							writer.Write(j);
							wf(value);
						};
					else
						fd.WriteFuncUnknown = (obj, storage, storageIndex) => {
							writer.Write(j);
							wf(yi.GetValue(obj));
						};
					result.Fields.Add(fd);
					++ourIndex;
					if (cmp == 0)
						++theirIndex;
				}
				else {
					var theirType = result.ReaderDef.Fields[theirIndex].Type;
					var wf = GetWriteFunc(theirType);
					result.Fields.Add(new ClassDef.FieldDef {
						Name = their.Name, Type = theirType,
						WriteFuncUnknown = (obj, storage, storageIndex) => {
							var si = storageIndex.Value;
							if (si < storage.Fields.Count && storage.Fields[si].Name == their.Name) {
								writer.Write(j);
								wf(storage.Fields[si].Value);
								++storageIndex.Value;
							}
						}
					});
					++theirIndex;
				}
			}
		}

		private void WriteClassDefFields(ClassDef def, string className)
		{
			writer.Write(def.Id);
			writer.Write(className);
			writer.Write((short)def.Fields.Count);
			foreach (var fd in def.Fields) {
				writer.Write(fd.Name);
				WriteRoughType(fd.Type);
			}
		}

		private void WriteFields(ClassDef def, object obj)
		{
			def.Meta.BeforeSerialization.Run(obj);
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

		private ClassDef WriteClassId(object obj)
		{
			var t = obj.GetType();
			ClassDef result;
			if (classIdCache.TryGetValue(t, out result)) {
				writer.Write(result.Id);
				var g = result.Meta.GetUnknownStorage;
				if (g == null)
					return result;
				var i = g(obj).Internal;
				// If we have unknown fields, their definition must be present in the first serialized object,
				// but not necessariliy in subsequent ones.
				if (i != null && i != result.ReaderDef)
					throw new YuzuException("Conflictiing reader class definitions for unknown storage of " + t.Name);
				return result;
			}

			result = new ClassDef { Id = (short)(classIdCache.Count + unknownClassIdCache.Count + 1) };
			result.Meta = Meta.Get(t, Options);
			classIdCache[t] = result;
			if (result.Meta.GetUnknownStorage == null)
				PrepareClassDefFields(result);
			else {
				result.ReaderDef = result.Meta.GetUnknownStorage(obj).Internal as ReaderClassDef;
				if (result.ReaderDef == null)
					PrepareClassDefFields(result);
				else
					PrepareClassDefFieldsUnknown(result);
			}
			WriteClassDefFields(result, result.Meta.WriteAlias ?? TypeSerializer.Serialize(result.Meta.Type));
			return result;
		}

		// Unknown class lacking binary-specific field descriptions.
		protected void WriteUnknown(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var u = (YuzuUnknown)obj;
			ClassDef def;
			if (unknownClassIdCache.TryGetValue(u.ClassTag, out def)) {
				writer.Write(def.Id);
			}
			else {
				def = new ClassDef { Id = (short)(classIdCache.Count + unknownClassIdCache.Count + 1) };
				def.Meta = Meta.Unknown;
				unknownClassIdCache[u.ClassTag] = def;
				short i = 0;
				foreach (var f in u.Fields) {
					short j = (short)(i + 1); // Capture.
					var t = f.Value.GetType();
					var wf = GetWriteFunc(t);
					var name = f.Key; // Capture.
					def.Fields.Add(new ClassDef.FieldDef {
						Name = name, Type = t,
						WriteFunc = obj1 => {
							object value;
							if ((obj1 as YuzuUnknown).Fields.TryGetValue(name, out value)) {
								writer.Write(j);
								wf(value);
							}
						},
					});
					++i;
				}
				WriteClassDefFields(def, u.ClassTag);
			}
			WriteFields(def, obj);
		}

		protected void WriteUnknownBinary(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var u = (YuzuUnknownBinary)obj;
			ClassDef def;
			if (unknownClassIdCache.TryGetValue(u.ClassTag, out def)) {
				writer.Write(def.Id);
			}
			else {
				def = new ClassDef { Id = (short)(classIdCache.Count + unknownClassIdCache.Count + 1) };
				def.Meta = Meta.Unknown;
				unknownClassIdCache[u.ClassTag] = def;
				for (short i = 1; i < u.Def.Fields.Count; ++i) {
					var f = u.Def.Fields[i];
					short j = (short)i; // Capture.
					var wf = GetWriteFunc(f.Type);
					def.Fields.Add(new ClassDef.FieldDef {
						Name = f.Name, Type = f.Type,
						WriteFunc = obj1 => {
							object value;
							if ((obj1 as YuzuUnknown).Fields.TryGetValue(f.Name, out value)) {
								writer.Write(j);
								wf(value);
							}
						},
					});
				}
				WriteClassDefFields(def, u.ClassTag);
			}
			WriteFields(def, obj);
		}

		private void WriteObject(object obj)
		{
			if (obj == null)
				writer.Write((short)0);
			else
				WriteFields(WriteClassId(obj), obj);
		}

		private void WriteObjectUnknown(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var def = WriteClassId(obj);
			var storage = def.Meta.GetUnknownStorage(obj);
			var storageIndex = new BoxedInt();
			objStack.Push(obj);
			try {
				if (def.Fields.Count > 0) {
					if (def.Fields[0].WriteFuncUnknown != null)
						foreach (var d in def.Fields)
							d.WriteFuncUnknown(obj, storage, storageIndex);
					else
						foreach (var d in def.Fields)
							d.WriteFunc(obj);
				}
				writer.Write((short)0);
			}
			finally {
				objStack.Pop();
			}
		}

		private void WriteObjectCompact(object obj)
		{
			if (obj == null) {
				writer.Write((short)0);
				return;
			}
			var def = WriteClassId(obj);
			def.Meta.BeforeSerialization.Run(obj);
			objStack.Push(obj);
			try {
				foreach (var d in def.Fields)
					d.WriteFuncCompact(obj);
			}
			finally {
				objStack.Pop();
			}
		}

		private Action<object> MakeWriteIEnumerable(Type t)
		{
			var wf = GetWriteFunc(t.GetGenericArguments()[0]);
			var m = Utils.GetPrivateCovariantGeneric(GetType(), nameof(WriteIEnumerable), t);
			return obj => m.Invoke(this, new object[] { obj, wf });
		}

		private Action<object> MakeWriteFunc(Type t)
		{
			if (t.IsEnum) 
				return WriteInt;
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(Dictionary<,>)) {
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), nameof(WriteDictionary), t);
					return obj => m.Invoke(this, new object[] { obj });
				}
				if (g == typeof(Action<>))
					return WriteAction;
				if (g == typeof(Nullable<>)) {
					var w = GetWriteFunc(t.GetGenericArguments()[0]);
					return obj => {
						writer.Write(obj == null);
						if (obj != null)
							w(obj);
					};
				}
				if (g == typeof(IEnumerable<>))
					return MakeWriteIEnumerable(t);
			}
			if (t.IsArray) {
				var wf = GetWriteFunc(t.GetElementType());
				var m = Utils.GetPrivateCovariantGeneric(GetType(), nameof(WriteArray), t);
				return obj => m.Invoke(this, new object[] { obj, wf });
			}
			var meta = Meta.Get(t, Options);
			{
				var icoll = Utils.GetICollection(t);
				if (icoll != null) {
					var wf = GetWriteFunc(icoll.GetGenericArguments()[0]);
					if (Utils.GetICollectionNG(t) != null)
						return obj => WriteCollectionNG(obj, wf);
					var m = Utils.GetPrivateCovariantGeneric(GetType(), nameof(WriteCollection), icoll);
					return obj => m.Invoke(this, new object[] { obj, wf });
				}
			}
			{
				var ienum = Utils.GetIEnumerable(t);
				if (ienum != null)
					return MakeWriteIEnumerable(ienum);
			}
			if (Utils.IsStruct(t) || t.IsClass || t.IsInterface) {
				if (meta.IsCompact) return WriteObjectCompact;
				if (meta.GetUnknownStorage == null) return WriteObject;
				return WriteObjectUnknown;
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

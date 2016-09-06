using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Yuzu.Deserializer;
using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinaryDeserializer : AbstractReaderDeserializer
	{
		public static BinaryDeserializer Instance = new BinaryDeserializer();

		public BinaryDeserializer()
		{
			InitReaders();
		}

		public override void Initialize() {}

		private object ReadSByte() { return Reader.ReadSByte(); }
		private object ReadByte() { return Reader.ReadByte(); }
		private object ReadShort() { return Reader.ReadInt16(); }
		private object ReadUShort() { return Reader.ReadUInt16(); }
		private object ReadInt() { return Reader.ReadInt32(); }
		private object ReadUInt() { return Reader.ReadUInt32(); }
		private object ReadLong() { return Reader.ReadInt64(); }
		private object ReadULong() { return Reader.ReadUInt64(); }
		private object ReadBool() { return Reader.ReadBoolean(); }
		private object ReadChar() { return Reader.ReadChar(); }
		private object ReadFloat() { return Reader.ReadSingle(); }
		private object ReadDouble() { return Reader.ReadDouble(); }
		private object ReadDecimal() { return Reader.ReadDecimal(); }

		private DateTime ReadDateTime() { return DateTime.FromBinary(Reader.ReadInt64()); }
		private TimeSpan ReadTimeSpan() { return new TimeSpan(Reader.ReadInt64()); }

		private object ReadString()
		{
			var s = Reader.ReadString();
			return s != "" ? s : Reader.ReadBoolean() ? null : "";
		}

		private class Record { }

		private Type ReadType()
		{
			var rt = (RoughType)Reader.ReadByte();
			if (RoughType.FirstAtom <= rt && rt <= RoughType.LastAtom)
				return RT.roughTypeToType[(int)rt];
			if (rt == RoughType.Sequence)
				return typeof(List<>).MakeGenericType(ReadType());
			if (rt == RoughType.Mapping) {
				var k = ReadType();
				var v = ReadType();
				return typeof(Dictionary<,>).MakeGenericType(k, v);
			}
			if (rt == RoughType.Record)
				return typeof(Record);
			if (rt == RoughType.Nullable)
				return typeof(Nullable<>).MakeGenericType(ReadType());
			throw Error("Unknown rough type {0}", rt);
		}

		private bool ReadCompatibleType(Type expectedType)
		{
			var rt = (RoughType)Reader.ReadByte();
			if (expectedType.IsEnum)
				return rt == RoughType.Int;
			if (RoughType.FirstAtom <= rt && rt <= RoughType.LastAtom)
				return RT.roughTypeToType[(int)rt] == expectedType;
			if (expectedType.IsArray)
				return rt == RoughType.Sequence && ReadCompatibleType(expectedType.GetElementType());
			if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				if (rt != RoughType.Mapping)
					return false;
				var g = expectedType.GetGenericArguments();
				return ReadCompatibleType(g[0]) && ReadCompatibleType(g[1]);
			}
			if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Nullable<>))
				return rt == RoughType.Nullable && ReadCompatibleType(expectedType.GetGenericArguments()[0]);
			var icoll = Utils.GetICollection(expectedType);
			if (icoll != null)
				return rt == RoughType.Sequence && ReadCompatibleType(icoll.GetGenericArguments()[0]);
			if (rt == RoughType.Record)
				return expectedType.IsRecord();
			throw Error("Unknown rough type {0}", rt);
		}

		protected object ReadAny()
		{
			var t = ReadType();
			if (t == typeof(object))
				throw Error("Unable to read pure object");
			return ReadValueFunc(t)();
		}

		private void InitReaders()
		{
			readerCache[typeof(sbyte)] = ReadSByte;
			readerCache[typeof(byte)] = ReadByte;
			readerCache[typeof(short)] = ReadShort;
			readerCache[typeof(ushort)] = ReadUShort;
			readerCache[typeof(int)] = ReadInt;
			readerCache[typeof(uint)] = ReadUInt;
			readerCache[typeof(long)] = ReadLong;
			readerCache[typeof(ulong)] = ReadULong;
			readerCache[typeof(bool)] = ReadBool;
			readerCache[typeof(char)] = ReadChar;
			readerCache[typeof(float)] = ReadFloat;
			readerCache[typeof(double)] = ReadDouble;
			readerCache[typeof(decimal)] = ReadDecimal;
			readerCache[typeof(DateTime)] = ReadDateTimeObj;
			readerCache[typeof(TimeSpan)] = ReadTimeSpanObj;
			readerCache[typeof(string)] = ReadString;
			readerCache[typeof(object)] = ReadAny;
			readerCache[typeof(Record)] = ReadObject<object>;
		}

		private object ReadDateTimeObj() { return ReadDateTime(); }
		private object ReadTimeSpanObj() { return ReadTimeSpan(); }

		protected void ReadIntoCollection<T>(ICollection<T> list)
		{
			var rf = ReadValueFunc(typeof(T));
			var count = Reader.ReadInt32();
			for (int i = 0; i < count; ++i)
				list.Add((T)rf());
		}

		protected I ReadCollection<I, E>() where I : class, ICollection<E>, new()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var list = new I();
			var rf = ReadValueFunc(typeof(E));
			for (int i = 0; i < count; ++i)
				list.Add((E)rf());
			return list;
		}

		protected List<T> ReadList<T>()
		{
			var count = Reader.ReadInt32();
			if (count == -1) return null;
			var list = new List<T>();
			var rf = ReadValueFunc(typeof(T));
			for (int i = 0; i < count; ++i)
				list.Add((T)rf());
			return list;
		}

		protected List<object> ReadListRecord()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var list = new List<object>();
			for (int i = 0; i < count; ++i)
				list.Add(ReadObject<object>());
			return list;
		}

		protected void ReadIntoDictionary<K, V>(Dictionary<K, V> dict)
		{
			var rk = ReadValueFunc(typeof(K));
			var rv = ReadValueFunc(typeof(V));
			var count = Reader.ReadInt32();
			for (int i = 0; i < count; ++i)
				dict.Add((K)rk(), (V)rv());
		}

		protected Dictionary<K, V> ReadDictionary<K, V>()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var dict = new Dictionary<K, V>();
			var rk = ReadValueFunc(typeof(K));
			var rv = ReadValueFunc(typeof(V));
			for (int i = 0; i < count; ++i)
				dict.Add((K)rk(), (V)rv());
			return dict;
		}

		protected T[] ReadArray<T>()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var rf = ReadValueFunc(typeof(T));
			var array = new T[count];
			for (int i = 0; i < count; ++i)
				array[i] = (T)rf();
			return array;
		}

		protected Action<T> ReadAction<T>() { return GetAction<T>(Reader.ReadString()); }

		protected class ClassDef
		{
			public class FieldDef
			{
				public string Name;
				public int OurIndex; // 1-based
				public Action<object> ReadFunc;
			}

			internal Meta Meta;
			public const int EOF = short.MaxValue;
			public Action<BinaryDeserializer, ClassDef, object> ReadFields;
			public Func<BinaryDeserializer, ClassDef, object> Make;
			public List<FieldDef> Fields = new List<FieldDef> { new FieldDef { OurIndex = EOF } };
		}

		// Zeroth element corresponds to 'null'.
		private List<ClassDef> classDefs = new List<ClassDef> { new ClassDef() };

		protected virtual void PrepareReaders(ClassDef def)
		{
			def.ReadFields = ReadFields;
		}

		public void ClearClassIds() { classDefs = new List<ClassDef> { new ClassDef() }; }

		private ClassDef GetClassDef(short classId)
		{
			if (classId < classDefs.Count)
				return classDefs[classId];
			if (classId > classDefs.Count)
				throw Error("Bad classId: {0}", classId);
			var result = new ClassDef();
			var typeName = Reader.ReadString();
			var classType = FindType(typeName);
			result.Meta = Meta.Get(classType, Options);
			PrepareReaders(result);
			var ourCount = result.Meta.Items.Count;
			var theirCount = Reader.ReadInt16();
			int ourIndex = 0, theirIndex = 0;
			while (ourIndex < ourCount && theirIndex < theirCount) {
				var yi = result.Meta.Items[ourIndex];
				var ourName = yi.Tag(Options);
				var theirName = Reader.ReadString();
				var cmp = String.CompareOrdinal(ourName, theirName);
				if (cmp < 0) {
					if (!yi.IsOptional)
						throw Error("Missing required field {0} for class {1}", ourName, typeName);
					ourIndex += 1;
				}
				else if (cmp > 0) {
					if (!Options.IgnoreUnknownFields)
						throw Error("New field {0} for class {1}", theirName, typeName);
					var rf = ReadValueFunc(ReadType());
					result.Fields.Add(new ClassDef.FieldDef {
						Name = theirName, OurIndex = -1, ReadFunc = obj => rf() });
					theirIndex += 1;
				}
				else {
					if (!ReadCompatibleType(yi.Type))
						throw Error(
							"Incompatible type for field {0}, expected {1}", ourName, yi.Type.Name);
					var fieldDef = new ClassDef.FieldDef { Name = theirName, OurIndex = ourIndex + 1 };
					if (yi.SetValue != null) {
						var rf = ReadValueFunc(yi.Type);
						fieldDef.ReadFunc = obj => yi.SetValue(obj, rf());
					}
					else {
						var mf = MergeValueFunc(yi.Type);
						fieldDef.ReadFunc = obj => mf(yi.GetValue(obj));
					}
					result.Fields.Add(fieldDef);
					ourIndex += 1;
					theirIndex += 1;
				}
			}
			while (ourIndex < ourCount) {
				var yi = result.Meta.Items[ourIndex];
				var ourName = yi.Tag(Options);
				if (!yi.IsOptional)
					throw Error("Missing required field {0} for class {1}", ourName, typeName);
				ourIndex += 1;
			}
			while (theirIndex < theirCount) {
				var theirName = Reader.ReadString();
				if (!Options.IgnoreUnknownFields)
					throw Error("New field {0} for class {1}", theirName, typeName);
				var rf = ReadValueFunc(ReadType());
				result.Fields.Add(new ClassDef.FieldDef {
					Name = theirName, OurIndex = -1, ReadFunc = obj => rf() });
				theirIndex += 1;
			}
			classDefs.Add(result);
			return result;
		}

		private static void ReadFields(BinaryDeserializer d, ClassDef def, object obj)
		{
			d.objStack.Push(obj);
			try {
				if (def.Meta.IsCompact) {
					for (int i = 1; i < def.Fields.Count; ++i)
						def.Fields[i].ReadFunc(obj);
				}
				else {
					var actualIndex = d.Reader.ReadInt16();
					for (int i = 1; i < def.Fields.Count; ++i) {
						var fd = def.Fields[i];
						if (i < actualIndex || actualIndex == 0) {
							if (fd.OurIndex < 0 || def.Meta.Items[fd.OurIndex - 1].IsOptional)
								continue;
							throw d.Error("Expected field '{0}' ({1}), but found '{2}'",
								i, fd.Name, actualIndex);
						}
						fd.ReadFunc(obj);
						actualIndex = d.Reader.ReadInt16();
					}
					if (actualIndex != 0)
						throw d.Error("Unfinished object, expected zero, but got {0}", actualIndex);
				}
			}
			finally {
				d.objStack.Pop();
			}
			def.Meta.RunAfterDeserialization(obj);
		}

		protected void ReadIntoObject<T>(object obj)
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				throw Error("Unable to read null into object");
			var def = GetClassDef(classId);
			if (obj.GetType() != def.Meta.Type)
				throw Error("Unable to read type {0} into {1}", def.Meta.Type, obj.GetType());
			def.ReadFields(this, def, obj);
		}

		protected void ReadIntoObjectUnchecked<T>(object obj)
		{
			var classId = Reader.ReadInt16();
			var def = GetClassDef(classId);
			def.ReadFields(this, def, obj);
		}

		protected object ReadObject<T>() where T : class
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return null;
			var def = GetClassDef(classId);
			if (!typeof(T).IsAssignableFrom(def.Meta.Type))
				throw Error("Unable to assign type {0} to {1}", def.Meta.Type, typeof(T));
			if (def.Make != null)
				return def.Make(this, def);
			var result = Activator.CreateInstance(def.Meta.Type);
			def.ReadFields(this, def, result);
			return result;
		}

		protected object ReadObjectUnchecked<T>() where T : class
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return null;
			var def = GetClassDef(classId);
			if (def.Make != null)
				return def.Make(this, def);
			var result = Activator.CreateInstance(def.Meta.Type);
			def.ReadFields(this, def, result);
			return result;
		}

		protected object ReadStruct<T>() where T : struct
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return null;
			var def = GetClassDef(classId);
			if (!typeof(T).IsAssignableFrom(def.Meta.Type))
				throw Error("Unable to assign type {0} to {1}", def.Meta.Type, typeof(T));
			if (def.Make != null)
				return def.Make(this, def);
			var result = Activator.CreateInstance(def.Meta.Type);
			def.ReadFields(this, def, result);
			return result;
		}

		protected object ReadStructUnchecked<T>() where T : struct
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return null;
			var def = GetClassDef(classId);
			if (def.Make != null)
				return def.Make(this, def);
			var result = Activator.CreateInstance(def.Meta.Type);
			def.ReadFields(this, def, result);
			return result;
		}

		private Dictionary<Type, Func<object>> readerCache = new Dictionary<Type, Func<object>>();
		private Dictionary<Type, Action<object>> mergerCache = new Dictionary<Type, Action<object>>();

		private Func<object> ReadValueFunc(Type t)
		{
			if (t == null)
				return ReadObject<object>;
			Func<object> f;
			if (readerCache.TryGetValue(t, out f))
				return f;
			return readerCache[t] = MakeReaderFunc(t);
		}

		private Action<object> MergeValueFunc(Type t)
		{
			Action<object> f;
			if (mergerCache.TryGetValue(t, out f))
				return f;
			return mergerCache[t] = MakeMergerFunc(t);
		}

		private Func<object> MakeReaderFunc(Type t)
		{
			if (t.IsEnum)
				return () => Enum.ToObject(t, ReadInt());
			if (t.IsGenericType) {
				var g = t.GetGenericTypeDefinition();
				if (g == typeof(List<>)) {
					if (t.GetGenericArguments()[0] == typeof(Record))
						return ReadListRecord;
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadList", t);
					return () => m.Invoke(this, Utils.ZeroObjects);
				}
				if (g == typeof(Dictionary<,>)) {
					// FIXME: Check for Record, similar to List case above.
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), "ReadDictionary", t);
					return () => m.Invoke(this, Utils.ZeroObjects);
				}
				if (g == typeof(Action<>)) {
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadAction", t);
					return () => m.Invoke(this, Utils.ZeroObjects);
				}
				if (g == typeof(Nullable<>)) {
					var r = ReadValueFunc(t.GetGenericArguments()[0]);
					return () => Reader.ReadBoolean() ? null : r();
				}
			}
			if (t.IsArray) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadArray", t);
				return () => m.Invoke(this, Utils.ZeroObjects);
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				var elemType = icoll.GetGenericArguments()[0];
				var m = GetType().GetMethod("ReadCollection", BindingFlags.Instance | BindingFlags.NonPublic).
					MakeGenericMethod(t, elemType);
				return () => m.Invoke(this, Utils.ZeroObjects);
			}
			if (t.IsClass || t.IsInterface) {
				var m = Utils.GetPrivateGeneric(GetType(), "ReadObject", t);
				return (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), this, m);
			}
			if (Utils.IsStruct(t)) {
				var m = Utils.GetPrivateGeneric(GetType(), "ReadStruct", t);
				return (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), this, m);
			}
			throw new NotImplementedException(t.Name);
		}

		private Action<object> MakeMergerFunc(Type t)
		{
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				var m = Utils.GetPrivateCovariantGenericAll(GetType(), "ReadIntoDictionary", t);
				return obj => { m.Invoke(this, new object[] { obj }); };
			}
			var icoll = Utils.GetICollection(t);
			if (icoll != null) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadIntoCollection", icoll);
				return obj => { m.Invoke(this, new object[] { obj }); };
			}
			if ((t.IsClass || t.IsInterface || Utils.IsStruct(t)) && t != typeof(object)) {
				var m = Utils.GetPrivateGeneric(GetType(), "ReadIntoObject", t);
				return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), this, m);
			}
			throw Error("Unable to merge field of type {0}", t.Name);
		}

		public override object FromReaderInt() { return ReadAny(); }

		public override object FromReaderInt(object obj)
		{
			var expectedType = obj.GetType();
			if (expectedType == typeof(object))
				throw Error("Unable to read into untyped object");
			if (!ReadCompatibleType(expectedType))
				throw Error("Incompatible type to read into {0}", expectedType.Name);
			MergeValueFunc(expectedType)(obj);
			return obj;
		}

		public override T FromReaderInt<T>()
		{
			if (!ReadCompatibleType(typeof(T)))
				throw Error("Incompatible type to read into {0}", typeof(T).Name);
			return (T)ReadValueFunc(typeof(T))();
		}
	}
}

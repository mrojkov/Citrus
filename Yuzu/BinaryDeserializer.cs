using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinaryDeserializer : AbstractReaderDeserializer
	{
		public static BinaryDeserializer Instance = new BinaryDeserializer();

		public BinaryDeserializer()
		{
			Options.Assembly = Assembly.GetCallingAssembly();
			InitReaders();
		}

		public override void Initialize() {}

		protected YuzuException Error(string message, params object[] args)
		{
			return new YuzuException(
				String.Format(message, args),
				Options.ReportErrorPosition ? new YuzuPosition(Reader.BaseStream.Position) : null);
		}

		protected object ReadInt() { return Reader.ReadInt32(); }
		protected object ReadUInt() { return Reader.ReadUInt32(); }
		protected object ReadSByte() { return Reader.ReadSByte(); }
		protected object ReadByte() { return Reader.ReadByte(); }
		protected object ReadShort() { return Reader.ReadInt16(); }
		protected object ReadUShort() { return Reader.ReadUInt16(); }
		protected object ReadLong() { return Reader.ReadInt64(); }
		protected object ReadULong() { return Reader.ReadUInt64(); }
		protected object ReadBool() { return Reader.ReadBoolean(); }
		protected object ReadChar() { return Reader.ReadChar(); }
		protected object ReadFloat() { return Reader.ReadSingle(); }
		protected object ReadDouble() { return Reader.ReadDouble(); }

		protected DateTime ReadDateTime() { return DateTime.FromBinary(Reader.ReadInt64()); }
		protected TimeSpan ReadTimeSpan() { return new TimeSpan(Reader.ReadInt64()); }

		protected object ReadString() {
			var s = Reader.ReadString();
			return s != "" ? s : Reader.ReadBoolean() ? null : "";
		}

		private void InitReaders()
		{
			readerCache[typeof(int)] = ReadInt;
			readerCache[typeof(uint)] = ReadUInt;
			readerCache[typeof(sbyte)] = ReadSByte;
			readerCache[typeof(byte)] = ReadByte;
			readerCache[typeof(short)] = ReadShort;
			readerCache[typeof(ushort)] = ReadUShort;
			readerCache[typeof(long)] = ReadLong;
			readerCache[typeof(ulong)] = ReadULong;
			readerCache[typeof(bool)] = ReadBool;
			readerCache[typeof(char)] = ReadChar;
			readerCache[typeof(float)] = ReadFloat;
			readerCache[typeof(double)] = ReadDouble;
			readerCache[typeof(DateTime)] = ReadDateTimeObj;
			readerCache[typeof(TimeSpan)] = ReadTimeSpanObj;
			readerCache[typeof(string)] = ReadString;
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

		private Stack<object> objStack = new Stack<object>();

		protected Action<T> ReadAction<T>()
		{
			var name = Reader.ReadString();
			if (name == "")
				return null;
			var obj = objStack.Peek();
			var m = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
			if (m == null)
				throw Error("Unknown action '{0}'", name);
			return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), obj, m);
		}

		protected class ClassDef
		{
			internal Meta Meta;
			internal List<Action<object>> ReadFunc = new List<Action<object>>();
			internal List<Tuple<string, int>> Fields = new List<Tuple<string, int>> { Tuple.Create("", 0) };
		}
		// Zeroth element corresponds to 'null'.
		private List<ClassDef> classDefs = new List<ClassDef> { new ClassDef() };

		protected ClassDef GetClassDef(short classId)
		{
			if (classId < classDefs.Count)
				return classDefs[classId];
			if (classId > classDefs.Count)
				Error("Bad classId: {0}", classId);
			var result = new ClassDef();
			var typeName = Reader.ReadString();
			var t = Options.Assembly.GetType(typeName, throwOnError: true);
			var meta = result.Meta = Meta.Get(t, Options);
			var fieldCount = Reader.ReadInt16();
			if (fieldCount != meta.Items.Count)
				throw new NotImplementedException();
			for (int i = 0; i < fieldCount; ++i) {
				var yi = meta.Items[i];
				var n = Reader.ReadString();
				if (n != yi.Name)
					throw new NotImplementedException();
				if (yi.SetValue != null) {
					var rf = ReadValueFunc(yi.Type);
					result.ReadFunc.Add(obj => yi.SetValue(obj, rf()));
				}
				else {
					var mf = MergeValueFunc(yi.Type);
					result.ReadFunc.Add(obj => mf(yi.GetValue(obj)));
				}
			}
			classDefs.Add(result);
			return result;
		}

		private void ReadFields(ClassDef def, object obj)
		{
			objStack.Push(obj);
			try {
				if (Utils.IsCompact(def.Meta.Type, Options)) {
					for (int i = 0; i < def.Meta.Items.Count; ++i)
						def.ReadFunc[i](obj);
				}
				else {
					var fi = Reader.ReadInt16();
					for (int i = 0; i < def.Meta.Items.Count; ++i) {
						var yi = def.Meta.Items[i];
						if (fi != i + 1) {
							if (!yi.IsOptional)
								throw Error("Expected field '{0}({1})', but found '{2}'",
									i + 1, yi.NameTagged(Options), fi);
							continue;
						}
						def.ReadFunc[i](obj);
						fi = Reader.ReadInt16();
					}
					if (fi != 0)
						throw Error("Unfinished object, expected zero, but got {0}", fi);
				}
			}
			finally {
				objStack.Pop();
			}
			def.Meta.RunAfterDeserialization(obj);
		}

		private void ReadIntoObject<T>(object obj)
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				throw Error("Unable to read null into object");
			var def = GetClassDef(classId);
			if (obj.GetType() != def.Meta.Type)
				throw Error("Unable to read type {0} into {1}", def.Meta.Type, obj.GetType());
			ReadFields(def, obj);
		}

		private object ReadObject<T>() where T: class
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return null;
			var def = GetClassDef(classId);
			if (!typeof(T).IsAssignableFrom(def.Meta.Type))
				throw Error("Unable to assign type {0} to {1}", def.Meta.Type, typeof(T));
			var result = Activator.CreateInstance(def.Meta.Type);
			ReadFields(def, result);
			return result;
		}

		private Dictionary<Type, Func<object>> readerCache = new Dictionary<Type, Func<object>>();
		private Dictionary<Type, Action<object>> mergerCache = new Dictionary<Type, Action<object>>();

		private Func<object> ReadValueFunc(Type t)
		{
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
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadList", t);
					return () => m.Invoke(this, new object[] { });
				}
				if (g == typeof(Dictionary<,>)) {
					var m = Utils.GetPrivateCovariantGenericAll(GetType(), "ReadDictionary", t);
					return () => m.Invoke(this, new object[] { });
				}
				if (g == typeof(Action<>)) {
					var p = t.GetGenericArguments();
					var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadAction", t);
					return () => m.Invoke(this, new object[] { });
				}
			}
			if (t.IsArray) {
				var m = Utils.GetPrivateCovariantGeneric(GetType(), "ReadArray", t);
				return () => m.Invoke(this, new object[] { });
			}
			var icoll = t.GetInterface(typeof(ICollection<>).Name);
			if (icoll != null) {
				var elemType = icoll.GetGenericArguments()[0];
				var m = GetType().GetMethod("ReadCollection", BindingFlags.Instance | BindingFlags.NonPublic).
					MakeGenericMethod(t, elemType);
				return () => m.Invoke(this, new object[] { });
			}
			if (t == typeof(object))
				throw new NotImplementedException();
			if (t.IsClass || t.IsInterface) {
				var m = Utils.GetPrivateGeneric(GetType(), "ReadObject", t);
				return (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), this, m);
			}
			if (Utils.IsStruct(t))
				return () => FromReaderInt(Activator.CreateInstance(t));
			throw new NotImplementedException(t.Name);
		}

		private Action<object> MakeMergerFunc(Type t)
		{
			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
				var m = Utils.GetPrivateCovariantGenericAll(GetType(), "ReadIntoDictionary", t);
				return obj => { m.Invoke(this, new object[] { obj }); };
			}
			var icoll = t.GetInterface(typeof(ICollection<>).Name);
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

		public override object FromReaderInt() { return ReadObject<object>(); }

		public override object FromReaderInt(object obj)
		{
			MergeValueFunc(obj.GetType())(obj);
			return obj;
		}

		public T FromReader<T>(BinaryReader reader)
		{
			Reader = reader;
			Initialize();
			return (T)ReadValueFunc(typeof(T))();
		}

		public T FromBytes<T>(byte[] bytes)
		{
			return FromReader<T>(new BinaryReader(new MemoryStream(bytes, false)));
		}
	}
}

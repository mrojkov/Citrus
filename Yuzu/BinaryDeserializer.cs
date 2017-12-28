using System;
using System.Collections.Generic;
using System.Reflection;

using Yuzu.Deserializer;
using Yuzu.Metadata;
using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinaryDeserializer : AbstractReaderDeserializer
	{
		public static BinaryDeserializer Instance = new BinaryDeserializer();

		public BinarySerializeOptions BinaryOptions = new BinarySerializeOptions();

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
			for (int i = 0; i < count; ++i) {
				object t = rf();

				if (IsSkipForUnknownData<T>(t)) continue;

				list.Add((T) t);
			}
		}

		protected I ReadCollection<I, E>() where I : class, ICollection<E>, new()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var list = new I();
			var rf = ReadValueFunc(typeof(E));
			for (int i = 0; i < count; ++i) {
				object e = rf();
				
				if (IsSkipForUnknownData<E>(e)) continue;

				list.Add((E) e);
			}
			return list;
		}

		protected List<T> ReadList<T>()
		{
			var count = Reader.ReadInt32();
			if (count == -1) return null;
			var list = new List<T>();
			var rf = ReadValueFunc(typeof(T));
			for (int i = 0; i < count; ++i) {
				object t = rf();

				if (IsSkipForUnknownData<T>(t)) continue;
				
				list.Add((T) t);
			}
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
			for (int i = 0; i < count; ++i) {
				object k = rk();
				object v = rv();

				if (IsSkipForUnknownData<V>(v)) continue;

				dict.Add((K) k, (V) v);
			}
		}

		protected Dictionary<K, V> ReadDictionary<K, V>()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var dict = new Dictionary<K, V>();
			var rk = ReadValueFunc(typeof(K));
			var rv = ReadValueFunc(typeof(V));
			for (int i = 0; i < count; ++i) {
				object k = rk();
				object v = rv();

				if (IsSkipForUnknownData<V>(v)) continue;

				dict.Add((K) k, (V) v);
			}
			return dict;
		}

		protected T[] ReadArray<T>()
		{
			var count = Reader.ReadInt32();
			if (count == -1)
				return null;
			var rf = ReadValueFunc(typeof(T));
			var array = new T[count];
			for (int i = 0; i < count; ++i) {
				object t = rf();
				
				if (IsSkipForUnknownData<T>(t)) continue;

				array[i] = (T) t;
			}
			return array;
		}

		/// <summary>
		/// skip for unknown type, becouse it is skip reading
		// fixes bug to skip absent collections in new data structure
		/// </summary>
		private static bool IsSkipForUnknownData<T>(object t)
		{
			if (typeof(T) == typeof(Record) && !(t is Record)) return true;
			return false;
		}

		protected Action<T> ReadAction<T>() { return GetAction<T>(Reader.ReadString()); }

		// Zeroth element corresponds to 'null'.
		// Second Param is SavedPosition in stream since which ClassDef can be read again
		private List<KeyValuePair<ReaderClassDef, object>> classDefsWithPositions = new List<KeyValuePair<ReaderClassDef, object>> { new KeyValuePair<ReaderClassDef, object>(new ReaderClassDef(), null) };

		// classId -> surrogateType -> classDef
		private readonly Dictionary<int, Dictionary<Type, ReaderClassDef>> classDefForSurrogateType = new Dictionary<int, Dictionary<Type, ReaderClassDef>>();

		protected virtual void PrepareReaders(ReaderClassDef def)
		{
			def.ReadFields = ReadFields;
		}

		public void ClearClassIds() { classDefsWithPositions = new List<KeyValuePair<ReaderClassDef, object>> { new KeyValuePair<ReaderClassDef, object>(new ReaderClassDef(), null) }; }

		private ReaderClassDef GetClassDefUnknown(string typeName)
		{
			short classId = (short) classDefsWithPositions.Count;
			object savedPosition = SavePosition();

			var result = new ReaderClassDef {
				Meta = Meta.Unknown,
				Make = (bd, def) => {
					var obj = new YuzuUnknownBinary { ClassTag = typeName, Def = def };
					obj = (YuzuUnknownBinary) ReadObjectFieldsWithMigrationCheck(classId, def, obj, true, true);
					return obj;
				},
			};
			var theirCount = Reader.ReadInt16();
			for (int theirIndex = 0; theirIndex < theirCount; ++theirIndex) {
				var theirName = Reader.ReadString();
				var t = ReadType();
				var rf = ReadValueFunc(t);
				result.Fields.Add(new ReaderClassDef.FieldDef {
					Name = theirName, Type = t, OurIndex = -1,
					ReadFunc = obj => ((YuzuUnknown)obj).Fields[theirName] = rf()
				});
			}
			classDefsWithPositions.Add(new KeyValuePair<ReaderClassDef, object>(result, savedPosition));
			return result;
		}

		private void AddUnknownFieldDef(ReaderClassDef def, string fieldName, string typeName)
		{
			if (!Options.AllowUnknownFields)
				throw Error("New field {0} for class {1}", fieldName, typeName);
			var fd = new ReaderClassDef.FieldDef { Name = fieldName, OurIndex = -1, Type = ReadType() };
			var rf = ReadValueFunc(fd.Type);
			if (def.Meta.GetUnknownStorage == null)
				fd.ReadFunc = obj => rf();
			else
				fd.ReadFunc = obj => def.Meta.GetUnknownStorage(obj).Add(fieldName, rf());
			def.Fields.Add(fd);
		}

		private ReaderClassDef GetClassDef(short classId, Type forSurrogateType = null)
		{
			if (forSurrogateType != null) {
				Dictionary<Type, ReaderClassDef> defsByType;
				ReaderClassDef def;
				if (classDefForSurrogateType.TryGetValue(classId, out defsByType) &&
					defsByType.TryGetValue(forSurrogateType, out def)) return def;

				GetClassDef(classId);

				object storedPosition = classDefsWithPositions[classId].Value;
				if (storedPosition == null) {
					throw Error("Cannot use class surrogate for not seekable stream {0}", forSurrogateType);
				}

				object wasPosition = SavePosition();
				RestorePosition(storedPosition);

				ReaderClassDef classDef = ReadClassDef(forSurrogateType, TypeSerializer.Serialize(forSurrogateType));
				if (defsByType == null) {
					defsByType = new Dictionary<Type, ReaderClassDef>();
					classDefForSurrogateType[classId] = defsByType;
				}
				defsByType[forSurrogateType] = classDef;

				RestorePosition(wasPosition);

				return classDef;
			}

			if (classId < classDefsWithPositions.Count) {
				return classDefsWithPositions[classId].Key;
			}
			if (classId > classDefsWithPositions.Count)
				throw Error("Bad classId: {0}", classId);
			var typeName = Reader.ReadString();
			var classType = TypeSerializer.Deserialize(typeName);
			if (classType == null)
				return GetClassDefUnknown(typeName);

			object savedPosition = SavePosition();

			ReaderClassDef result = ReadClassDef(classType, typeName);

			classDefsWithPositions.Add(new KeyValuePair<ReaderClassDef, object>(result, savedPosition));

			return result;
		}

		private ReaderClassDef ReadClassDef(Type classType, string typeName)
		{
			ReaderClassDef result = new ReaderClassDef {Meta = Meta.Get(classType, Options)};
			PrepareReaders(result);
			var ourCount = result.Meta.Items.Count;
			var theirCount = Reader.ReadInt16();
			int ourIndex = 0, theirIndex = 0;
			var theirName = "";
			while (ourIndex < ourCount && theirIndex < theirCount) {
				var yi = result.Meta.Items[ourIndex];
				var ourName = yi.Tag(Options);
				if (theirName == "")
					theirName = Reader.ReadString();
				var cmp = String.CompareOrdinal(ourName, theirName);
				if (cmp < 0) {
					if (!yi.IsOptional)
						throw Error("Missing required field {0} for class {1}", ourName, typeName);
					ourIndex += 1;
				} else if (cmp > 0) {
					AddUnknownFieldDef(result, theirName, typeName);
					theirIndex += 1;
					theirName = "";
				} else {
					if (!ReadCompatibleType(yi.Type))
						throw Error(
							"Incompatible type for field {0}, expected {1}", ourName, yi.Type.Name);
					var fieldDef = new ReaderClassDef.FieldDef {
						Name = theirName,
						OurIndex = ourIndex + 1,
						Type = yi.Type
					};
					if (yi.SetValue != null) {
						var rf = ReadValueFunc(yi.Type);
						fieldDef.ReadFunc = obj => yi.SetValue(obj, rf());
					} else {
						var mf = MergeValueFunc(yi.Type);
						fieldDef.ReadFunc = obj => mf(yi.GetValue(obj));
					}
					result.Fields.Add(fieldDef);
					ourIndex += 1;
					theirIndex += 1;
					theirName = "";
				}
			}
			for (; ourIndex < ourCount; ++ourIndex) {
				var yi = result.Meta.Items[ourIndex];
				var ourName = yi.Tag(Options);
				if (!yi.IsOptional)
					throw Error("Missing required field {0} for class {1}", ourName, typeName);
			}
			for (; theirIndex < theirCount; ++theirIndex) {
				if (theirName == "")
					theirName = Reader.ReadString();
				AddUnknownFieldDef(result, theirName, typeName);
				theirName = "";
			}
			return result;
		}

		private static void ReadFields(BinaryDeserializer d, ReaderClassDef def, object obj)
		{
			d.objStack.Push(obj);
			try {
				if (def.Meta.IsCompact) {
					for (int i = 1; i < def.Fields.Count; ++i)
						def.Fields[i].ReadFunc(obj);
				}
				else {
					if (def.Meta.GetUnknownStorage != null) {
						var storage = def.Meta.GetUnknownStorage(obj);
						storage.Clear();
						storage.Internal = def;
					}
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
			def.Meta.AfterDeserialization.Run(obj);
		}

		protected void ReadIntoObject<T>(object obj)
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				throw Error("Unable to read null into object");
			var def = GetClassDef(classId);
			var expectedType = obj.GetType();
			if (
				expectedType != def.Meta.Type &&
				(!Meta.Get(expectedType, Options).AllowReadingFromAncestor || expectedType.BaseType != def.Meta.Type)
			)
				throw Error("Unable to read type {0} into {1}", def.Meta.Type, expectedType);
			ReadObjectFieldsWithMigrationCheck(classId, def, obj, false);
		}

		protected void ReadIntoObjectUnchecked<T>(object obj)
		{
			var classId = Reader.ReadInt16();
			var def = GetClassDef(classId);
			ReadObjectFieldsWithMigrationCheck(classId, def, obj, false);
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

			return ReadObjectFieldsWithMigrationCheck(classId, def, result, true);
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
			return ReadObjectFieldsWithMigrationCheck(classId, def, result, true);
		}

		protected void EnsureClassDef(Type t)
		{
			var def = GetClassDef(Reader.ReadInt16());
			if (def.Meta.Type != t)
				throw Error("Expected type {0}, but found {1}", def.Meta.Type, t.Name);
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
			return ReadObjectFieldsWithMigrationCheck(classId, def, result, true);
		}

		protected void ReadIntoStruct<T>(ref T s) where T : struct
		{
			var classId = Reader.ReadInt16();
			if (classId == 0)
				return;
			var def = GetClassDef(classId);
			if (!typeof(T).IsAssignableFrom(def.Meta.Type))
				throw Error("Unable to assign type {0} to {1}", def.Meta.Type, typeof(T));
			if (def.Make != null) {
				s = (T)def.Make(this, def);
				return;
			}
			var result = Activator.CreateInstance(def.Meta.Type);
			result = ReadObjectFieldsWithMigrationCheck(classId, def, result, true);
			s = (T)result;
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
			result = ReadObjectFieldsWithMigrationCheck(classId, def, result, true);
			return result;
		}

		private object ReadObjectFieldsWithMigrationCheck(short classId, ReaderClassDef def, object result, bool allowRecreate, bool readFieldsDirect = false)
		{
			object savedPosition = SavePosition();
			try {
				if (readFieldsDirect) {
					ReadFields(this, def, result);
				} else {
					def.ReadFields(this, def, result);
				}
			} catch (Exception) {
				if (def.Meta.MigrateOnDeserializationException == null || savedPosition == null) throw;

				result = ApplyYuzuMigration(def.Meta.MigrateOnDeserializationException, classId, def.Meta.Type, result, savedPosition, allowRecreate);
			}

			if (def.Meta.MigrateOnDeserializationValidation != null && savedPosition != null) {
				foreach (Meta.ValidationItem validationItem in def.Meta.MigrateOnDeserializationValidation) {
					if (!validationItem.Invoke(result)) {
						result = ApplyYuzuMigration(validationItem.Attr, classId, def.Meta.Type, result, savedPosition, allowRecreate);
					}
				}
			}

			return result;
		}

		private object ApplyYuzuMigration(YuzuMigration yuzuMigration, short classId, Type resultType, object result, object savedPosition, bool allowRecreate)
		{
			if (savedPosition == null) return result;

			RestorePosition(savedPosition);

			object surrogate = Activator.CreateInstance(yuzuMigration.PreviousVersionTypeSurrogate);
			if (allowRecreate && yuzuMigration.RecreateIfAllowed) {
				result = Activator.CreateInstance(resultType);
			}

			ReaderClassDef defSurrogate = GetClassDef(classId, yuzuMigration.PreviousVersionTypeSurrogate);
			surrogate = ReadObjectFieldsWithMigrationCheck(classId, defSurrogate, surrogate, true);

			yuzuMigration.MethodInfoApplyToNewer.Invoke(surrogate, new[] {result});
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

		public override object FromReaderInt()
		{
			if (BinaryOptions.AutoSignature)
				CheckSignature();
			return ReadAny();
		}

		public override object FromReaderInt(object obj)
		{
			var expectedType = obj.GetType();
			if (expectedType == typeof(object))
				throw Error("Unable to read into untyped object");
			if (BinaryOptions.AutoSignature)
				CheckSignature();
			if (!ReadCompatibleType(expectedType))
				throw Error("Incompatible type to read into {0}", expectedType.Name);
			MergeValueFunc(expectedType)(obj);
			return obj;
		}

		public override T FromReaderInt<T>()
		{
			if (BinaryOptions.AutoSignature)
				CheckSignature();
			if (typeof(T) == typeof(object))
				return (T)ReadAny();
			if (!ReadCompatibleType(typeof(T)))
				throw Error("Incompatible type to read into {0}", typeof(T).Name);
			return (T)ReadValueFunc(typeof(T))();
		}

		// If possible, preserves stream position if signature is absent.
		public bool IsValidSignature()
		{
			var s = BinaryOptions.Signature;
			if (s.Length == 0)
				return true;
			if (!Reader.BaseStream.CanSeek)
				return s.Equals(Reader.ReadBytes(s.Length));
			var pos = Reader.BaseStream.Position;
			if (Reader.BaseStream.Length - pos < s.Length)
				return false;
			foreach (var b in s)
				if (b != Reader.ReadByte()) {
					Reader.BaseStream.Position = pos;
					return false;
				}
			return true;
		}

		public void CheckSignature()
		{
			if (!IsValidSignature())
				throw Error("Signature not found");
		}

		protected override object SavePosition()
		{
			object savePosition = base.SavePosition();
			if (savePosition == null) return null;
			return new[] {savePosition, new List<KeyValuePair<ReaderClassDef, object>>(classDefsWithPositions)};
		}

		protected override void RestorePosition(object position)
		{
			if (position == null) return;
			base.RestorePosition(((object[])position)[0]);
			classDefsWithPositions.Clear();
			classDefsWithPositions.AddRange((List<KeyValuePair<ReaderClassDef, object>>)((object[])position)[1]);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Yuzu.Util;

namespace Yuzu.Metadata
{
	public class Meta
	{
		private static Dictionary<Tuple<Type, CommonOptions>, Meta> cache =
			new Dictionary<Tuple<Type, CommonOptions>, Meta>();
		private static Dictionary<CommonOptions, Dictionary<string, Type>> readAliasCache =
			new Dictionary<CommonOptions, Dictionary<string, Type>>();

		public class Item : IComparable<Item>
		{
			private string id;

			public string Name;
			public string Alias;
			public string Id
			{
				get
				{
					if (id == null)
						id = IdGenerator.GetNextId();
					return id;
				}
			}
			public bool IsOptional;
			public bool IsCompact;
			public Func<object, object, bool> SerializeIf;
			public Type Type;
			public Func<object, object> GetValue;
			public Action<object, object> SetValue;
			public FieldInfo FieldInfo;
			public PropertyInfo PropInfo;

			public int CompareTo(Item yi) { return Alias.CompareTo(yi.Alias); }

			public string Tag(CommonOptions options)
			{
				switch (options.TagMode) {
					case TagMode.Names:
						return Name;
					case TagMode.Aliases:
						return Alias;
					case TagMode.Ids:
						return Id;
					default:
						throw new YuzuAssert();
				}
			}
			public string NameTagged(CommonOptions options)
			{
				var tag = Tag(options);
				return Name + (tag == Name ? "" : " (" + tag + ")");
			}
		}

		public readonly Type Type;
		private MetaOptions Options;
		public readonly List<Item> Items = new List<Item>();
		public readonly bool IsCompact;
		public object Default { get; private set; }
		public YuzuItemKind Must = YuzuItemKind.None;
		public YuzuItemKind AllKind = YuzuItemKind.None;
		public YuzuItemOptionality AllOptionality = YuzuItemOptionality.None;
		public bool AllowReadingFromAncestor;
		public Surrogate Surrogate;
		public string WriteAlias;
		public int RequiredCount { get; private set; }
		public Func<object, int, object, bool> SerializeItemIf;

		public Dictionary<string, Item> TagToItem = new Dictionary<string, Item>();
		public Func<object, YuzuUnknownStorage> GetUnknownStorage;

		public ActionList BeforeSerialization = new ActionList();
		public ActionList AfterDeserialization = new ActionList();

#if !iOS // Apple forbids code generation.
		private static Action<object, object> SetterGenericHelper<TTarget, TParam>(MethodInfo m)
		{
			var action =
				(Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
			return (object target, object param) => action((TTarget)target, (TParam)param);
		}

		private static Func<object, object> GetterGenericHelper<TTarget, TReturn>(MethodInfo m)
		{
			var func =
				(Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), m);
			return (object target) => (object)func((TTarget)target);
		}

		private static Action<object, object> BuildSetter(MethodInfo m)
		{
			var helper = typeof(Meta).
				GetMethod(nameof(SetterGenericHelper), BindingFlags.Static | BindingFlags.NonPublic).
				MakeGenericMethod(m.DeclaringType, m.GetParameters()[0].ParameterType);
			return (Action<object, object>)helper.Invoke(null, new object[] { m });
		}

		private static Func<object, object> BuildGetter(MethodInfo m)
		{
			var helper = typeof(Meta).
				GetMethod(nameof(GetterGenericHelper), BindingFlags.Static | BindingFlags.NonPublic).
				MakeGenericMethod(m.DeclaringType, m.ReturnType);
			return (Func<object, object>)helper.Invoke(null, new object[] { m });
		}
#endif

		private struct ItemAttrs
		{
			private Attribute[] attrs;
			public Attribute Optional => attrs[0];
			public Attribute Required => attrs[1];
			public Attribute Member => attrs[2];
			public int Count;
			public Attribute Any() => Optional ?? Required ?? Member;

			public ItemAttrs(MemberInfo m, MetaOptions options, YuzuItemOptionality opt)
			{
				var attrTypes = new Type[] {
					options.OptionalAttribute,
					options.RequiredAttribute,
					options.MemberAttribute,
				};
				attrs = attrTypes.Select(t => m.GetCustomAttribute_Compat(t, false)).ToArray();
				Count = attrs.Count(a => a != null);
				if (Count == 0 && opt > 0 && attrTypes[(int)opt - 1] != null) {
					attrs[(int)opt - 1] = Activator.CreateInstance(attrTypes[(int)opt - 1]) as Attribute;
					Count = 1;
				}
			}
		}

		private bool IsNonEmptyCollection<T>(object obj, object value) =>
			value == null || ((ICollection<T>)value).Count > 0;

		private void AddItem(MemberInfo m, bool must, bool all)
		{
			var ia = new ItemAttrs(m, Options, all ? AllOptionality : YuzuItemOptionality.None);
			if (ia.Count == 0) {
				if (must)
					throw Error("Item {0} must be serialized", m.Name);
				return;
			}
			if (ia.Count != 1)
				throw Error("More than one of optional, required and member attributes for field '{0}'", m.Name);
			var serializeIf = m.GetCustomAttribute_Compat(Options.SerializeIfAttribute, true);
			var item = new Item {
				Alias = Options.GetAlias(ia.Any()) ?? m.Name,
				IsOptional = ia.Required == null,
				IsCompact = m.IsDefined(Options.CompactAttribute, false),
				SerializeIf = serializeIf != null ? Options.GetSerializeCondition(serializeIf) : null,
				Name = m.Name,
			};
			if (!item.IsOptional)
				RequiredCount += 1;
			var merge = m.IsDefined(Options.MergeAttribute, false);

			switch (m.MemberType) {
				case MemberTypes.Field:
					var f = m as FieldInfo;
					if (!f.IsPublic)
						throw Error("Non-public item '{0}'", f.Name);
					item.Type = f.FieldType;
					item.GetValue = f.GetValue;
					if (!merge)
						item.SetValue = f.SetValue;
					item.FieldInfo = f;
					break;
				case MemberTypes.Property:
					var p = m as PropertyInfo;
					var getter = p.GetGetMethod();
					if (getter == null)
						throw Error("No getter for item '{0}'", p.Name);
					item.Type = p.PropertyType;
					var setter = p.GetSetMethod();
#if iOS // Apple forbids code generation.
					item.GetValue = obj => p.GetValue(obj, Utils.ZeroObjects);
					if (!merge && setter != null)
						item.SetValue = (obj, value) => p.SetValue(obj, value, Utils.ZeroObjects);
#else
					if (Utils.IsStruct(Type)) {
						item.GetValue = obj => p.GetValue(obj, Utils.ZeroObjects);
						if (!merge && setter != null)
							item.SetValue = (obj, value) => p.SetValue(obj, value, Utils.ZeroObjects);
					} else {
						item.GetValue = BuildGetter(getter);
						if (!merge && setter != null)
							item.SetValue = BuildSetter(setter);
					}
#endif
					item.PropInfo = p;
					break;
				default:
					throw Error("Member type {0} not supported", m.MemberType);
			}
			if (item.SetValue == null) {
				if (!item.Type.IsClass && !item.Type.IsInterface || item.Type == typeof(object))
					throw Error("Unable to either set or merge item {0}", item.Name);
			}
			if (item.Type.IsDefined(Options.CompactAttribute, false))
				item.IsCompact = true;
			if (ia.Member != null && item.SerializeIf == null && !Type.IsAbstract && !Type.IsInterface) {
				if (Default == null)
					Default = Activator.CreateInstance(Type);
				var d = item.GetValue(Default);
				var icoll = Utils.GetICollection(item.Type);
				if (d != null && icoll != null) {
					var mi = Utils.GetPrivateGeneric(
						GetType(), nameof(IsNonEmptyCollection), icoll.GetGenericArguments()[0]);
					item.SerializeIf =
						(Func<object, object, bool>)
						Delegate.CreateDelegate(typeof(Func<object, object, bool>), this, mi);
				}
				else
					item.SerializeIf = (object obj, object value) => !Object.Equals(item.GetValue(obj), d);
			}
			Items.Add(item);
		}

		private void AddMethod(MethodInfo m)
		{
			if (m.IsDefined(Options.SerializeItemIfAttribute, false)) {
				if (SerializeItemIf != null)
					throw Error("Duplicate SerializeItemIf");
				if (Utils.GetIEnumerable(Type) == null)
					throw Error("SerializeItemIf may only be used inside of IEnumerable");
				SerializeItemIf = YuzuSerializeItemIf.MakeChecker(m);
			}
			BeforeSerialization.MaybeAdd(m, Options.BeforeSerializationAttribute);
			AfterDeserialization.MaybeAdd(m, Options.AfterDeserializationAttribute);
			Surrogate.ProcessMethod(m);
		}

		private void ExploreType(Type t)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			foreach (var m in t.GetMembers(bindingFlags)) {
				if (Options.ExcludeAttribute != null && m.IsDefined(Options.ExcludeAttribute, false))
					continue;
				switch (m.MemberType) {
					case MemberTypes.Field:
						var f = m as FieldInfo;
						if (f.FieldType == typeof(YuzuUnknownStorage)) {
							if (GetUnknownStorage != null)
								throw Error("Duplicated unknown storage in field {0}", m.Name);
							GetUnknownStorage = obj => (YuzuUnknownStorage)f.GetValue(obj);
						}
						else
							AddItem(m,
								Must.HasFlag(YuzuItemKind.Field) && f.IsPublic,
								AllKind.HasFlag(YuzuItemKind.Field) && f.IsPublic);
						break;
					case MemberTypes.Property:
						var p = m as PropertyInfo;
						var g = p.GetGetMethod();
						if (p.PropertyType == typeof(YuzuUnknownStorage)) {
							if (GetUnknownStorage != null)
								throw Error("Duplicated unknown storage in field {0}", m.Name);
#if iOS // Apple forbids code generation.
							GetUnknownStorage = obj => (YuzuUnknownStorage)p.GetValue(obj, Utils.ZeroObjects);
#else
							var getter = BuildGetter(g);
							GetUnknownStorage = obj => (YuzuUnknownStorage)getter(obj);
#endif
						}
						else
							AddItem(m,
								Must.HasFlag(YuzuItemKind.Property) && g != null,
								AllKind.HasFlag(YuzuItemKind.Property) && g != null);
						break;
					case MemberTypes.Method:
						AddMethod(m as MethodInfo);
						break;
				}
			}
		}

		private Meta(Type t)
		{
			Type = t;
			Options = MetaOptions.Default;
		}

		private Meta(Type t, CommonOptions options)
		{
			Type = t;
			Options = options.Meta ?? MetaOptions.Default;
			IsCompact = t.IsDefined(Options.CompactAttribute, false);
			var must = t.GetCustomAttribute_Compat(Options.MustAttribute, false);
			if (must != null)
				Must = Options.GetItemKind(must);
			var all = t.GetCustomAttribute_Compat(Options.AllAttribute, false);
			if (all != null) {
				var ok = Options.GetItemOptionalityAndKind(all);
				AllOptionality = ok.Item1;
				AllKind = ok.Item2;
			}

			Surrogate = new Surrogate(Type, Options);
			foreach (var i in t.GetInterfaces())
				ExploreType(i);
			ExploreType(t);
			Surrogate.Complete();

			if (Utils.GetICollection(t) != null) {
				if (Items.Count > 0)
					throw Error("Serializable fields in collection are not supported");
			}
			else if (
				!options.AllowEmptyTypes && Items.Count == 0 && !(t.IsInterface || t.IsAbstract) &&
				Surrogate.SurrogateType == null
			)
				throw Error("No serializable fields");

			Items.Sort();
			Item prev = null;
			foreach (var i in Items) {
				if (prev != null && prev.CompareTo(i) == 0)
					throw Error("Duplicate item {0} / {1}", i.Name, i.Alias);
				prev = i;
			}
			var prevTag = "";
			foreach (var i in Items) {
				var tag = i.Tag(options);
				if (tag == "")
					throw Error("Empty tag for field '{0}'", i.Name);
				foreach (var ch in tag)
					if (ch <= ' ' || ch >= 127)
						throw Error("Bad character '{0}' in tag for field '{1}'", ch, i.Name);
				if (tag == prevTag)
					throw Error("Duplicate tag '{0}' for field '{1}'", tag, i.Name);
				prevTag = tag;
				TagToItem.Add(tag, i);
			}

			AllowReadingFromAncestor = t.IsDefined(Options.AllowReadingFromAncestorAttribute, false);
			if (AllowReadingFromAncestor) {
				var ancestorMeta = Get(t.BaseType, options);
				if (ancestorMeta.Items.Count != Items.Count)
					throw Error(
						"Allows reading from ancestor {0}, but has {1} items instead of {2}",
						t.BaseType.Name, Items.Count, ancestorMeta.Items.Count);
			}

			var alias = t.GetCustomAttribute_Compat(Options.AliasAttribute, false);
			if (alias != null) {
				var aliases = Options.GetReadAliases(alias);
				if (aliases != null) {
					Dictionary<string, Type> readAliases;
					if (!readAliasCache.TryGetValue(options, out readAliases)) {
						readAliases = new Dictionary<string, Type>();
						readAliasCache.Add(options, readAliases);
					}
					foreach (var a in aliases)
						try {
							if (String.IsNullOrWhiteSpace(a))
								throw Error("Empty read alias");
							readAliases.Add(a, t);
						} catch (ArgumentException) {
							throw Error("Duplicate read alias '{0}'", a);
						}
				}
				WriteAlias = Options.GetWriteAlias(alias);
				if (WriteAlias != null && WriteAlias == "")
					throw Error("Empty write alias");
			}
		}

		public static Meta Get(Type t, CommonOptions options)
		{
			Meta meta;
			if (cache.TryGetValue(Tuple.Create(t, options), out meta))
				return meta;
			meta = new Meta(t, options);
			cache.Add(Tuple.Create(t, options), meta);
			return meta;
		}

		public static Type GetTypeByReadAlias(string alias, CommonOptions options)
		{
			Dictionary<string, Type> readAliases;
			if (!readAliasCache.TryGetValue(options, out readAliases))
				return null;
			Type result;
			return readAliases.TryGetValue(alias, out result) ? result : null;
		}

		internal static Meta Unknown = new Meta(typeof(YuzuUnknown));

		private YuzuException Error(string format, params object[] args) =>
			new YuzuException("In type '" + Type.FullName + "': " + String.Format(format, args));

		private static bool HasItems(Type t, MetaOptions options)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var all = t.GetCustomAttribute_Compat(options.AllAttribute, false);
			var k = all != null ? options.GetItemOptionalityAndKind(all).Item2 : YuzuItemKind.None;
			foreach (var m in t.GetMembers(bindingFlags)) {
				if (
					m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property ||
					options.ExcludeAttribute != null && m.IsDefined(options.ExcludeAttribute, false)
				)
					continue;
				if (
					k.HasFlag(YuzuItemKind.Field) && m.MemberType == MemberTypes.Field ||
					k.HasFlag(YuzuItemKind.Property) && m.MemberType == MemberTypes.Property ||
					new ItemAttrs(m, options, YuzuItemOptionality.None).Any() != null
				)
					return true;
			}
			return false;
		}

		public static List<Type> Collect(Assembly assembly, MetaOptions options = null)
		{
			var result = new List<Type>();
			var q = new Queue<Type>(assembly.GetTypes());
			while (q.Count > 0) {
				var t = q.Dequeue();
				if (HasItems(t, options ?? MetaOptions.Default) && !t.IsGenericTypeDefinition)
					result.Add(t);
				foreach (var nt in t.GetNestedTypes())
					q.Enqueue(nt);
			}
			return result;
		}

		public const int FoundNonPrimitive = -1;
		public int CountPrimitiveChildren(CommonOptions options)
		{
			int result = 0;
			foreach (var yi in Items) {
				if (yi.Type.IsPrimitive || yi.Type.IsEnum || yi.Type == typeof(string)) {
					result += 1;
				} else if (yi.IsCompact) {
					var c = Get(yi.Type, options).CountPrimitiveChildren(options);
					if (c == FoundNonPrimitive) return FoundNonPrimitive;
					result += c;
				} else
					return FoundNonPrimitive;
			}
			return result;
		}
	}

}

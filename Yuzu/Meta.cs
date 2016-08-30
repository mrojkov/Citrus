using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Yuzu.Util;

namespace Yuzu.Metadata
{
	public class Meta
	{
		private static Dictionary<Tuple<Type, CommonOptions>, Meta> cache =
			new Dictionary<Tuple<Type, CommonOptions>, Meta>();

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
		public Dictionary<string, Item> TagToItem = new Dictionary<string, Item>();

		public struct MethodAction
		{
			public MethodInfo Info;
			public Action<object> Run;
		}

		public List<MethodAction> AfterDeserialization = new List<MethodAction>();

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
			var helper = typeof(Meta).GetMethod(
				"SetterGenericHelper", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(
				m.DeclaringType, m.GetParameters()[0].ParameterType);
			return (Action<object, object>)helper.Invoke(null, new object[] { m });
		}

		private static Func<object, object> BuildGetter(MethodInfo m)
		{
			var helper = typeof(Meta).GetMethod(
				"GetterGenericHelper", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(
				m.DeclaringType, m.ReturnType);
			return (Func<object, object>)helper.Invoke(null, new object[] { m });
		}
#endif

		private struct ItemAttrs
		{
			public Attribute Optional;
			public Attribute Required;
			public Attribute Member;
			public int Count;
			public Attribute Any() { return Optional ?? Required ?? Member; }
			public ItemAttrs(MemberInfo m, MetaOptions options)
			{
				Optional = m.GetCustomAttribute_Compat(options.OptionalAttribute, false);
				Required = m.GetCustomAttribute_Compat(options.RequiredAttribute, false);
				Member = m.GetCustomAttribute_Compat(options.MemberAttribute, false);
				Count = (Optional != null ? 1 : 0) + (Required != null ? 1 : 0) + (Member != null ? 1 : 0);
			}
		}

		private void AddItem(MemberInfo m)
		{
			var ia = new ItemAttrs(m, Options);
			if (ia.Count == 0)
				return;
			if (ia.Count != 1)
				throw Error("More than one of optional, required and member attributes for field '{0}'", m.Name);
			var serializeIf = m.GetCustomAttribute_Compat(Options.SerializeIfAttribute, true);
			var item = new Item {
				Alias = Options.GetAlias(ia.Any()) ?? m.Name,
				IsOptional = ia.Required == null,
				IsCompact =
					m.IsDefined(Options.CompactAttribute, false) ||
					m.GetType().IsDefined(Options.CompactAttribute, false),
				SerializeIf = serializeIf != null ? Options.GetSerializeCondition(serializeIf) : null,
				Name = m.Name,
			};
			var merge = m.IsDefined(Options.MergeAttribute, false);

			switch (m.MemberType) {
				case MemberTypes.Field:
					var f = m as FieldInfo;
					item.Type = f.FieldType;
					item.GetValue = f.GetValue;
					if (!merge)
						item.SetValue = f.SetValue;
					item.FieldInfo = f;
					break;
				case MemberTypes.Property:
					var p = m as PropertyInfo;
					item.Type = p.PropertyType;
#if iOS // Apple forbids code generation.
					item.GetValue = obj => p.GetValue(obj, Utils.ZeroObjects);
					var setter = p.GetSetMethod();
					if (!merge && setter != null)
						item.SetValue = (obj, value) => p.SetValue(obj, value, Utils.ZeroObjects);
#else
					item.GetValue = BuildGetter(p.GetGetMethod());
					var setter = p.GetSetMethod();
					if (!merge && setter != null)
						item.SetValue = BuildSetter(setter);
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
			if (ia.Member != null && item.SerializeIf == null && !Type.IsAbstract && !Type.IsInterface) {
				if (Default == null)
					Default = Activator.CreateInstance(Type);
				var d = item.GetValue(Default);
				item.SerializeIf = (object obj, object value) => !Object.Equals(item.GetValue(obj), d);
			}
			Items.Add(item);
		}

		private void AddMethod(MethodInfo m)
		{
			if (m.IsDefined(Options.AfterDeserializationAttribute, false))
				AfterDeserialization.Add(new MethodAction { Info = m, Run = obj => m.Invoke(obj, null) });
		}

		public void RunAfterDeserialization(object obj)
		{
			foreach (var a in AfterDeserialization)
				a.Run(obj);
		}

		private void ExploreType(Type t)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			foreach (var m in t.GetMembers(bindingFlags)) {
				if (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
					AddItem(m);
				else if (m.MemberType == MemberTypes.Method)
					AddMethod(m as MethodInfo);
			}
		}

		private Meta(Type t, CommonOptions options)
		{
			Type = t;
			Options = options.Meta ?? MetaOptions.Default;
			IsCompact = t.IsDefined(Options.CompactAttribute, false);

			foreach (var i in t.GetInterfaces())
				ExploreType(i);
			ExploreType(t);
			if (t.GetInterface(typeof(ICollection<>).Name) != null) {
				if (Items.Count > 0)
					throw Error("Serializable fields in collection are not supported");
			}
			else if (!options.AllowEmptyTypes && Items.Count == 0 && !(t.IsInterface || t.IsAbstract))
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

		private YuzuException Error(string format, params object[] args)
		{
			return new YuzuException("In type '" + Type.FullName + "': " + String.Format(format, args));
		}

		private static bool HasItems(Type t, MetaOptions options)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			foreach (var m in t.GetMembers(bindingFlags)) {
				if (m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property)
					continue;
				if (new ItemAttrs(m, options).Any() != null)
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
	}

}

using System;
using System.Collections.Generic;
using System.Reflection;

using Yuzu.Util;

namespace Yuzu.Metadata
{
	internal class Meta
	{
		private static Dictionary<Tuple<Type, CommonOptions>, Meta> cache =
			new Dictionary<Tuple<Type, CommonOptions>, Meta>();

		internal class Item : IComparable<Item>
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
		public readonly CommonOptions Options;
		public readonly List<Item> Items = new List<Item>();

		public struct MethodAction
		{
			public MethodInfo Info;
			public Action<object> Run;
		}

		public List<MethodAction> AfterDeserialization = new List<MethodAction>();

		private void AddItem(MemberInfo m)
		{
			var optional = m.GetCustomAttribute(Options.OptionalAttribute, false);
			var required = m.GetCustomAttribute(Options.RequiredAttribute, false);
			if (optional == null && required == null)
				return;
			if (optional != null && required != null)
				throw Error("Both optional and required attributes for field '{0}'", m.Name);
			var serializeIf = m.GetCustomAttribute(Options.SerializeIfAttribute, true);
			var item = new Item {
				Alias = Options.GetAlias(optional ?? required) ?? m.Name,
				IsOptional = optional != null,
				IsCompact =
					m.IsDefined(Options.CompactAttribute) ||
					m.GetType().IsDefined(Options.CompactAttribute),
				SerializeIf = serializeIf != null ? Options.GetSerializeCondition(serializeIf) : null,
				Name = m.Name,
			};

			switch (m.MemberType) {
				case MemberTypes.Field:
					var f = m as FieldInfo;
					item.Type = f.FieldType;
					item.GetValue = f.GetValue;
					item.SetValue = f.SetValue;
					item.FieldInfo = f;
					break;
				case MemberTypes.Property:
					var p = m as PropertyInfo;
					item.Type = p.PropertyType;
					item.GetValue = p.GetValue;
					item.SetValue = p.SetValue;
					item.PropInfo = p;
					break;
				default:
					throw Error("Member type {0} not supported", m.MemberType);
			}

			Items.Add(item);
		}

		private void AddMethod(MethodInfo m)
		{
			if (m.IsDefined(Options.AfterDeserializationAttribute))
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
			Options = options;
			foreach (var i in t.GetInterfaces())
				ExploreType(i);
			ExploreType(t);
			if (Utils.IsICollection(t)) {
				if (Items.Count > 0)
					throw Error("Serializable fields in collection are not supported");
			}
			else if (!options.AllowEmptyTypes && Items.Count == 0 && !t.IsInterface)
				throw Error("No serializable fields");
			Items.Sort();
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
	}

}

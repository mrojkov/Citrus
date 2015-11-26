using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuzu
{
#if NET40
	internal static class Net4
	{
		public static bool IsDefined(this MemberInfo m, Type t)
		{
			return m.IsDefined(t, false);
		}

		public static Attribute GetCustomAttribute(this MemberInfo m, Type t, bool inherit)
		{
			var attrs = m.GetCustomAttributes(t, inherit);
			if (attrs.Count() > 1)
				throw new AmbiguousMatchException();
			return (Attribute)attrs.FirstOrDefault();
		}

		public static object GetValue(this PropertyInfo m, object obj)
		{
			return m.GetValue(obj, new object[] { });
		}
		public static void SetValue(this PropertyInfo m, object obj, object value)
		{
			m.SetValue(obj, value, new object[] { });
		}
	}
#endif

	internal class YuzuItem: IComparable<YuzuItem>
	{
		private string id;

		public string Name;
		public string Alias;
		public string Id { get {
			if (id == null)
				id = IdGenerator.GetNextId();
			return id;
		} }
		public bool IsOptional;
		public bool IsCompact;
		public Func<object, object, bool> SerializeIf;
		public Type Type;
		public Func<object, object> GetValue;
		public Action<object, object> SetValue;
		public FieldInfo FieldInfo;
		public PropertyInfo PropInfo;

		public int CompareTo(YuzuItem yi) { return Alias.CompareTo(yi.Alias); }

		public string Tag(CommonOptions options)
		{
			switch (options.TagMode) {
				case TagMode.Names: return Name;
				case TagMode.Aliases: return Alias;
				case TagMode.Ids: return Id;
				default: throw new YuzuAssert();
			}
		}
		public string NameTagged(CommonOptions options)
		{
			var tag = Tag(options);
			return Name + (tag == Name ? "" : " (" + tag + ")");
		}
	}

	internal class Utils
	{
		private static Dictionary<Tuple<Type, CommonOptions>, List<YuzuItem>> yuzuItemsCache =
			new Dictionary<Tuple<Type, CommonOptions>, List<YuzuItem>>();

		public static string QuoteCSharpStringLiteral(string s)
		{
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t");
		}

		public static string CodeValueFormat(object value)
		{
			var t = value.GetType();
			if (t == typeof(int) || t == typeof(uint) || t == typeof(float) || t == typeof(double))
				return value.ToString();
			if (t == typeof(bool))
				return value.ToString().ToLower();
			if (t == typeof(string))
				return '"' + QuoteCSharpStringLiteral(value.ToString()) + '"';
			return "";
			//throw new NotImplementedException();
		}

		public static bool IsStruct(Type t)
		{
			return t.IsValueType && !t.IsPrimitive && !t.IsEnum && !t.IsPointer;
		}

		public static MethodInfo GetPrivateCovariantGeneric(Type callerType, string name, Type container)
		{
			var t = container.HasElementType ? container.GetElementType() : container.GetGenericArguments()[0];
			return callerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(t);
		}

		public static MethodInfo GetPrivateCovariantGenericAll(Type callerType, string name, Type container)
		{
			return
				callerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).
					MakeGenericMethod(container.GetGenericArguments());
		}

		public static bool IsCompact(Type t, CommonOptions options)
		{
			return t.IsDefined(options.CompactAttribute);
		}

		private static YuzuException Error(Type t, string format, params object[] args)
		{
			return new YuzuException("In type '" + t.FullName + "': " + String.Format(format, args));

		}

		public static List<YuzuItem> GetYuzuItems(Type t, CommonOptions options)
		{
			List<YuzuItem> items;
			if (yuzuItemsCache.TryGetValue(Tuple.Create(t, options), out items))
				return items;
			items = new List<YuzuItem>();
			yuzuItemsCache.Add(Tuple.Create(t, options), items);
			foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)) {
				if (m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property)
					continue;

				var optional = m.GetCustomAttribute(options.OptionalAttribute, false);
				var required = m.GetCustomAttribute(options.RequiredAttribute, false);
				var serializeIf = m.GetCustomAttribute(options.SerializeIfAttribute, true);
				if (optional == null && required == null)
					continue;
				if (optional != null && required != null)
					throw Error(t, "Both optional and required attributes for field '{0}'", m.Name);
				var item = new YuzuItem {
						Alias = options.GetAlias(optional ?? required) ?? m.Name,
						IsOptional = optional != null,
						IsCompact =
							m.IsDefined(options.CompactAttribute) ||
							m.GetType().IsDefined(options.CompactAttribute),
						SerializeIf = serializeIf != null ? options.GetSerializeCondition(serializeIf) : null,
						Name = m.Name,
				};

				if (m.MemberType == MemberTypes.Field) {
					var f = m as FieldInfo;
					item.Type = f.FieldType;
					item.GetValue = f.GetValue;
					item.SetValue = f.SetValue;
					item.FieldInfo = f;
				}
				else{
					var p = m as PropertyInfo;
					item.Type = p.PropertyType;
					item.GetValue = p.GetValue;
					item.SetValue = p.SetValue;
					item.PropInfo = p;
				}

				items.Add(item);
			}
			if (!options.AllowEmptyTypes && items.Count == 0)
				throw Error(t, "No serializable fields");
			items.Sort();
			var prevTag = "";
			foreach (var i in items) {
				var tag = i.Tag(options);
				if (tag == "")
					throw Error(t, "Empty tag for field '{0}'", i.Name);
				foreach (var ch in tag)
					if (ch <= ' ' || ch >= 127)
						throw Error(t, "Bad character '{0}' in tag for field '{1}'", ch, i.Name);
				if (tag == prevTag)
					throw Error(t, "Duplicate tag '{0}' for field '{1}'", tag, i.Name);
				prevTag = tag;
			}
			return items;
		}
	}
}

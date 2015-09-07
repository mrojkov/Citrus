using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuzu
{
	internal class YuzuItem: IComparable<YuzuItem> {
		public int Order;
		public bool IsOptional;
		public string Name;
		public Type Type;
		public Func<object, object> GetValue;
		public Action<object, object> SetValue;
		public FieldInfo FieldInfo;
		public PropertyInfo PropInfo;

		public int CompareTo(YuzuItem yi) { return Order.CompareTo(yi.Order); }
	}

	internal class Utils
	{
		private static Dictionary<Tuple<Type, CommonOptions>, List<YuzuItem>> yuzuItemsCache =
			new Dictionary<Tuple<Type, CommonOptions>, List<YuzuItem>>();

		public static List<YuzuItem> GetYuzuItems(Type t, CommonOptions options)
		{
			List<YuzuItem> items;
			if (!yuzuItemsCache.TryGetValue(Tuple.Create(t, options), out items))
				items = new List<YuzuItem>();
			foreach (var m in t.GetMembers()) {
				if (m.MemberType != MemberTypes.Field && m.MemberType != MemberTypes.Property)
					continue;

				var optional = m.GetCustomAttribute(options.OptionalAttribute, false);
				var required = m.GetCustomAttribute(options.RequiredAttribute, false);
				if (optional == null && required == null)
					continue;
				if (optional != null && required != null)
					throw new YuzuException();
				var item = new YuzuItem {
						Order = options.GetOrder(optional ?? required),
						IsOptional = optional != null,
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
			items.Sort();
			return items;
		}
	}
}

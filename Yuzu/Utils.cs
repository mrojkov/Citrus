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
		public static IEnumerable<YuzuItem> GetYuzuItems(Type t, CommonOptions options)
		{
			var items = new List<YuzuItem>();
			foreach (var m in t.GetMembers()) {
				var optional = m.GetCustomAttribute(options.OptionalAttribute, false);
				var required = m.GetCustomAttribute(options.RequiredAttribute, false);

				if (optional == null && required == null)
					continue;
				if (optional != null && required != null)
					throw new YuzuException();
				var order = options.GetOrder(optional ?? required);

				if (m.MemberType == MemberTypes.Field) {
					var f = m as FieldInfo;
					items.Add(new YuzuItem {
						Order = order,
						IsOptional = optional != null,
						Name = m.Name,
						Type = f.FieldType,
						GetValue = f.GetValue,
						SetValue = f.SetValue,
						FieldInfo = f,
					});
				}
				else if (m.MemberType == MemberTypes.Property) {
					var p = m as PropertyInfo;
					items.Add(new YuzuItem {
						Order = order,
						IsOptional = optional != null,
						Name = m.Name,
						Type = p.PropertyType,
						GetValue = p.GetValue,
						SetValue = p.SetValue,
						PropInfo = p,
					});
				}
				else
					continue;
			}
			items.Sort();
			return items;
		}
	}
}

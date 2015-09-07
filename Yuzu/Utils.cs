using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuzu
{
	internal class YuzuItem {
		public bool IsOptional;
		public string Name;
		public Type Type;
		public Func<object, object> GetValue;
		public Action<object, object> SetValue;
		public FieldInfo FieldInfo;
		public PropertyInfo PropInfo;
	}

	internal class Utils
	{
		public static IEnumerable<YuzuItem> GetYuzuItems(Type t, CommonOptions options)
		{
			foreach (var m in t.GetMembers()) {
				var isOptional = m.IsDefined(options.OptionalAttribute, false);
				var isRequired = m.IsDefined(options.RequiredAttribute, false);
				if (!isOptional && !isRequired)
					continue;
				if (m.MemberType == MemberTypes.Field) {
					var f = m as FieldInfo;
					yield return new YuzuItem {
						IsOptional = isOptional,
						Name = m.Name,
						Type = f.FieldType,
						GetValue = f.GetValue,
						SetValue = f.SetValue,
					};
				}
				else if (m.MemberType == MemberTypes.Property) {
					var p = m as PropertyInfo;
					yield return new YuzuItem {
						IsOptional = isOptional,
						Name = m.Name,
						Type = p.PropertyType,
						GetValue = p.GetValue,
						SetValue = p.SetValue,
					};
				}
				else
					continue;
			}
		}
	}
}

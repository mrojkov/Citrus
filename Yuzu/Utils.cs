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
			foreach (var f in t.GetFields()) {
				if (f.IsDefined(options.OptionalAttribute, false))
					yield return new YuzuItem {
						IsOptional = true,
						Name = f.Name,
						Type = f.FieldType,
						GetValue = f.GetValue,
						SetValue = f.SetValue,
						FieldInfo = f,
					};
				else if (f.IsDefined(options.RequiredAttribute, false))
					yield return new YuzuItem {
						IsOptional = false,
						Name = f.Name,
						Type = f.FieldType,
						GetValue = f.GetValue,
						SetValue = f.SetValue,
						FieldInfo = f,
					};
			}
			foreach (var p in t.GetProperties()) {
				if (p.IsDefined(options.OptionalAttribute, false))
					yield return new YuzuItem {
						IsOptional = true,
						Name = p.Name,
						Type = p.PropertyType,
						GetValue = p.GetValue,
						SetValue = p.SetValue,
						PropInfo = p,
					};
				else if (p.IsDefined(options.RequiredAttribute, false))
					yield return new YuzuItem {
						IsOptional = false,
						Name = p.Name,
						Type = p.PropertyType,
						GetValue = p.GetValue,
						SetValue = p.SetValue,
						PropInfo = p,
					};
			}
		}
	}
}

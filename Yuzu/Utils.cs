using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuzu
{
	internal class YuzuItem {
		public bool IsOptional;
		public string Name;
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
						FieldInfo = f,
					};
				else if (f.IsDefined(options.RequiredAttribute, false))
					yield return new YuzuItem {
						IsOptional = false,
						Name = f.Name,
						FieldInfo = f,
					};
			}
			foreach (var p in t.GetProperties()) {
				if (p.IsDefined(options.OptionalAttribute, false))
					yield return new YuzuItem {
						IsOptional = true,
						Name = p.Name,
						PropInfo = p,
					};
				else if (p.IsDefined(options.RequiredAttribute, false))
					yield return new YuzuItem {
						IsOptional = false,
						Name = p.Name,
						PropInfo = p,
					};
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System;
using Lime;

namespace Tangerine.UI
{
	public static class PropertyRegistry
	{
		static Dictionary<Type, Dictionary<string, TangerineAttribute>> map;
		
		static PropertyRegistry()
		{
			map = new Dictionary<Type, Dictionary<string, TangerineAttribute>>();
		}
		
		public static TangerineAttribute GetTangerineAttribute(Type type, string property)
		{
			Dictionary<string, TangerineAttribute> propMap;
			if (!map.TryGetValue(type, out propMap)) {
				map[type] = propMap = new Dictionary<string, TangerineAttribute>();
			}
			TangerineAttribute ta;
			if (!propMap.TryGetValue(property, out ta)) {
				var prop = type.GetProperty(property);
				var attr = prop.GetCustomAttributes(false);
				propMap[property] = ta = attr.FirstOrDefault(i => i is TangerineAttribute) as TangerineAttribute;
			}
			return ta;
		}
	}
}
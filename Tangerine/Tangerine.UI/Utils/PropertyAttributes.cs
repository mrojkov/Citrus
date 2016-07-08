using System.Collections.Generic;
using System.Linq;
using System;
using Lime;

namespace Tangerine.UI
{
	public static class PropertyAttributes<T> where T: Attribute
	{
		static Dictionary<Type, Dictionary<string, T>> map;
		
		static PropertyAttributes()
		{
			map = new Dictionary<Type, Dictionary<string, T>>();
		}
		
		public static T Get(Type type, string property)
		{
			Dictionary<string, T> propMap;
			if (!map.TryGetValue(type, out propMap)) {
				map[type] = propMap = new Dictionary<string, T>();
			}
			T ta;
			if (!propMap.TryGetValue(property, out ta)) {
				var prop = type.GetProperty(property);
				var attr = prop.GetCustomAttributes(false);
				propMap[property] = ta = attr.FirstOrDefault(i => i is T) as T;
			}
			return ta;
		}
	}
}
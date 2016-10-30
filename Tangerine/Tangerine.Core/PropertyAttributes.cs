using System.Collections.Generic;
using System.Linq;
using System;

namespace Tangerine.Core
{
	public static class PropertyAttributes<T> where T: Attribute
	{
		static readonly Dictionary<Type, Dictionary<string, T>> map = new Dictionary<Type, Dictionary<string, T>>();

		public static T Get(Type type, string property)
		{
			Dictionary<string, T> propMap;
			if (!map.TryGetValue(type, out propMap)) {
				map[type] = propMap = new Dictionary<string, T>();
			}
			T attr;
			if (!propMap.TryGetValue(property, out attr)) {
				var prop = type.GetProperty(property);
				propMap[property] = attr = prop.GetCustomAttributes(false).FirstOrDefault(i => i is T) as T;
			}
			return attr;
		}
	}
}
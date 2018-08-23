using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

namespace Tangerine.Core
{
	public static class PropertyAttributes<T> where T: Attribute
	{
		static readonly Dictionary<Type, Dictionary<string, T>> map = new Dictionary<Type, Dictionary<string, T>>();

		public static T Get(System.Reflection.PropertyInfo property)
		{
			return Get(property.DeclaringType, property.Name);
		}

		public static T Get(Type type, string property)
		{
			Dictionary<string, T> propMap;
			if (!map.TryGetValue(type, out propMap)) {
				map[type] = propMap = new Dictionary<string, T>();
			}
			T attr;
			if (!propMap.TryGetValue(property, out attr)) {
				// use last part of property path in case it's Animator.PropertyPath
				int index = property.LastIndexOf('.');
				var actualProperty = index == -1
					? property
					: property.Substring(index + 1);
				var prop = type.GetProperties().First(p => p.Name == actualProperty);
				// workaround for hidden properties ambiguity (e.g. Layout.Owner vs NodeComponent.Owner)
				propMap[property] = attr = prop.GetCustomAttributes(false).FirstOrDefault(i => i is T) as T;
			}
			return attr;
		}
	}
}

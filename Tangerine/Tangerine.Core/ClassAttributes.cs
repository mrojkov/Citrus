using System.Collections.Generic;
using System.Linq;
using System;

namespace Tangerine.Core
{
	public static class ClassAttributes<T> where T: Attribute
	{
		static readonly Dictionary<Type, T> map = new Dictionary<Type, T>();

		public static T Get(Type type)
		{
			T attr;
			if (!map.TryGetValue(type, out attr)) {
				attr = type.GetCustomAttributes(false).FirstOrDefault(i => i is T) as T;
				map[type] = attr;
			}
			return attr;
		}
	}
}
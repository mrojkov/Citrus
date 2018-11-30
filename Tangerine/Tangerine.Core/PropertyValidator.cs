using System;
using System.Reflection;
using Lime;

namespace Tangerine.Core
{
	public static class PropertyValidator
	{
		public static bool TryValidateValue(object value, Type type, string property)
		{
			if (value == null) {
				return true;
			}
			var attr = PropertyAttributes<TangerineValidationAttribute>.Get(type, property);
			return attr == null || attr.IsValid(value);
		}

		public static bool TryValidateValue(object value, PropertyInfo propertyInfo)
		{
			return TryValidateValue(value, propertyInfo.DeclaringType, propertyInfo.Name);
		}
	}
}

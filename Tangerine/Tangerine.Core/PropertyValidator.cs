using System;
using System.Reflection;
using Lime;

namespace Tangerine.Core
{
	public static class PropertyValidator
	{
		public static bool ValidateValue(object value, Type type, string property)
		{
			var attr = PropertyAttributes<TangerineValidationAttribute>.Get(type, property);
			return attr == null || attr.IsValid(value);
		}

		public static bool ValidateValue(object value, PropertyInfo propertyInfo)
		{
			return ValidateValue(value, propertyInfo.DeclaringType, propertyInfo.Name);
		}
	}
}

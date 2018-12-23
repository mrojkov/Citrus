using System;
using System.Reflection;
using Lime;

namespace Tangerine.Core
{
	public static class PropertyValidator
	{
		public static ValidationResult ValidateValue(object value, Type type, string property, out string message)
		{
			var attr = PropertyAttributes<TangerineValidationAttribute>.Get(type, property);
			message = null;
			return attr == null ? ValidationResult.Ok : attr.IsValid(value, out message);
		}

		public static ValidationResult ValidateValue(object value, PropertyInfo propertyInfo, out string message)
		{
			return ValidateValue(value, propertyInfo.DeclaringType, propertyInfo.Name, out message);
		}
	}
}

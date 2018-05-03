using System;
using System.Linq;
using System.Reflection;
using Lime;

namespace Tangerine.Core
{
	public static class NodeCompositionValidator
	{
		public static bool Validate(Type parentType, Type childType)
		{
			return ValidateChildType(parentType, childType) && ValidateParentType(parentType, childType);
		}

		static bool ValidateChildType(Type parentType, Type childType)
		{
			for (var p = parentType; p != null; p = p.BaseType) {
				var a = ClassAttributes<AllowedChildrenTypes>.Get(p);
				if (a != null) {
					return a.Types.Any(t => childType == t || childType.IsSubclassOf(t));
				}
			}
			return false;
		}

		static bool ValidateParentType(Type parentType, Type childType)
		{
			for (var p = childType; p != null; p = p.BaseType) {
				var a = ClassAttributes<AllowedParentTypes>.Get(p);
				if (a != null) {
					return a.Types.Any(t => parentType == t || parentType.IsSubclassOf(t));
				}
			}
			return true;
		}

		public static bool CanHaveChildren(Type type)
		{
			for (var p = type; p != null; p = p.BaseType) {
				var a = ClassAttributes<AllowedChildrenTypes>.Get(p);
				if (a != null) {
					return a.Types.Length > 0;
				}
			}
			return false;
		}

		public static bool IsCopyPasteAllowed(Type type)
		{
			return type.GetCustomAttribute<TangerineForbidCopyPasteAttribute>(false) == null;
		}
	}
}

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

		private static bool ValidateChildType(Type parentType, Type childType)
		{
			for (var p = parentType; p != null; p = p.BaseType) {
				var a = ClassAttributes<TangerineAllowedChildrenTypes>.Get(p);
				if (a != null) {
					return a.Types.Any(t => childType == t || childType.IsSubclassOf(t));
				}
			}
			return false;
		}

		private static bool ValidateParentType(Type parentType, Type childType)
		{
			for (var p = childType; p != null; p = p.BaseType) {
				var a = ClassAttributes<TangerineAllowedParentTypes>.Get(p);
				if (a != null) {
					return a.Types.Any(t => parentType == t || parentType.IsSubclassOf(t));
				}
			}
			return true;
		}

		public static bool CanHaveChildren(Type type)
		{
			for (var p = type; p != null; p = p.BaseType) {
				var a = ClassAttributes<TangerineAllowedChildrenTypes>.Get(p);
				if (a != null) {
					return a.Types.Length > 0;
				}
			}
			return false;
		}

		public static bool ValidateComponentType(Type nodeType, Type componentType)
		{
			for (var t = componentType; t != null && t != typeof(NodeComponent); t = t.BaseType) {
				var a = ClassAttributes<AllowedComponentOwnerTypes>.Get(t);
				if (a != null) {
					return a.Types.Any(ownerType => ownerType == nodeType || nodeType.IsSubclassOf(ownerType));
				}
			}
			return true;
		}

		public static bool IsCopyPasteAllowed(Type type)
		{
			return type.GetCustomAttribute<TangerineForbidCopyPasteAttribute>(false) == null;
		}
	}
}

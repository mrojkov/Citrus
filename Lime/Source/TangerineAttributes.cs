using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Yuzu;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class TangerineRegisterComponentAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class TangerineRegisterNodeAttribute : Attribute
	{
		public bool CanBeRoot;
		public int Order = int.MaxValue;
	}

	/// <summary>
	/// Denotes a property which can not be animated within Tangerine.
	/// </summary>
	public sealed class TangerineStaticPropertyAttribute : Attribute
	{ }

	public sealed class TangerineKeyframeColorAttribute : Attribute
	{
		public int ColorIndex;

		public TangerineKeyframeColorAttribute(int colorIndex)
		{
			ColorIndex = colorIndex;
		}
	}

	public sealed class TangerineNodeBuilderAttribute : Attribute
	{
		public string MethodName { get; private set; }

		public TangerineNodeBuilderAttribute(string methodName)
		{
			MethodName = methodName;
		}
	}

	public sealed class TangerineAllowedParentTypes : Attribute
	{
		public Type[] Types;

		public TangerineAllowedParentTypes(params Type[] types)
		{
			Types = types;
		}
	}

	public sealed class TangerineAllowedChildrenTypes : Attribute
	{
		public Type[] Types;

		public TangerineAllowedChildrenTypes(params Type[] types)
		{
			Types = types;
		}
	}

	public sealed class TangerineForbidCopyPasteAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineIgnoreIfAttribute : Attribute
	{
		public readonly string Method;

		private Func<object, bool> checker;

		public TangerineIgnoreIfAttribute(string method)
		{
			Method = method;
		}

		public bool Check(object obj)
		{
			if (checker == null) {
				var fn = obj.GetType().GetMethod(Method, BindingFlags.NonPublic | BindingFlags.Instance);
				if (fn == null) {
					throw new System.Exception("Couldn't find method " + Method);
				}

				var p = Expression.Parameter(typeof(object));
				var e = Expression.Call(Expression.Convert(p, obj.GetType()), fn);
				checker = Expression.Lambda<Func<object, bool>>(e, p).Compile();
			}

			return checker(obj);
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TangerineIgnoreAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineInspectAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineGroupAttribute : Attribute
	{
		public readonly string Name;

		public TangerineGroupAttribute(string name)
		{
			Name = name ?? String.Empty;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TangerineVisualHintGroupAttribute : Attribute
	{
		public readonly string Group;
		public readonly string AliasTypeName;

		public TangerineVisualHintGroupAttribute(string group, string aliasTypeName = null)
		{
			Group = group ?? "/";
			AliasTypeName = aliasTypeName;
		}
	}

	public sealed class TangerineDropDownListPropertyEditorAttribute : Attribute
	{
		private readonly string methodName;
		private readonly Type type;
		private Func<IEnumerable<(string, object)>> lister = null;

		public TangerineDropDownListPropertyEditorAttribute(string methodName, Type type)
		{
			this.type = type;
			this.methodName = methodName;
		}

		public IEnumerable<(string, object)> EnumerateItems()
		{
			var mi = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
			if (lister == null) {
				var fn = type.GetMethod(methodName);
				var e = Expression.Call(fn);
				lister = Expression.Lambda<Func<IEnumerable<(string, object)>>>(e).Compile();
			}
			return lister();
		}
	}
}

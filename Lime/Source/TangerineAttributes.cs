using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

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

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineReadOnlyAttribute : Attribute
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
				var fn = obj.GetType().GetMethod(Method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (fn == null) {
					throw new System.Exception("Couldn't find method " + Method);
				}

				var p = Expression.Parameter(typeof(object));
				var e = Expression.Call(Expression.Convert(p, fn.DeclaringType), fn);
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

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class TangerineOnPropertySetAttribute : Attribute
	{
		private readonly string methodName;

		public TangerineOnPropertySetAttribute(string methodName)
		{
			this.methodName = methodName;
		}

		public void Invoke(object o)
		{
			var type = o.GetType();
			var fn = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			fn.Invoke(o, new object[] { });
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

	public sealed class TangerineFilePropertyAttribute : Attribute
	{
		public readonly string[] AllowedFileTypes;
		private readonly string valueToStringMethodName;
		private readonly string stringToValueMethodName;
		public TangerineFilePropertyAttribute(string[] allowedFileTypes, string ValueToStringMethodName = null, string StringToValueMethodName = null)
		{
			AllowedFileTypes = allowedFileTypes;
			stringToValueMethodName = StringToValueMethodName;
			valueToStringMethodName = ValueToStringMethodName;
		}

		public T StringToValueConverter<T>(Type type, string s) => string.IsNullOrEmpty(stringToValueMethodName)
				? (T)(object)(s ?? "")
				: (T)type.GetMethod(stringToValueMethodName).Invoke(null, new object[] { s });

		public string ValueToStringConverter<T>(Type type, T v) => string.IsNullOrEmpty(valueToStringMethodName)
			? (string)(object)(v == null ? (T)(object)"" : v)
			: (string)type.GetMethod(valueToStringMethodName).Invoke(null, new object[] { v });
	}

	public sealed class TangerineDropDownListPropertyEditorAttribute : Attribute
	{
		private readonly string methodName;

		public TangerineDropDownListPropertyEditorAttribute(string methodName)
		{
			this.methodName = methodName;
		}

		public IEnumerable<(string, object)> EnumerateItems(object o)
		{
			var type = o.GetType();
			var fn = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
			return (IEnumerable<(string, object)>)fn.Invoke(o, new object[] { });
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class TangerineNumericEditBoxStepAttribute : Attribute
	{
		public readonly float Step;

		public TangerineNumericEditBoxStepAttribute(float step)
		{
			Step = step;
		}

		public void SetProperty(object editBox) => ((NumericEditBox)editBox).Step = Step;
	}

	public enum ValidationResult
	{
		Ok, Warning, Error,
	}

	/// <summary>
	/// Everything that leads to exception is Error, except if exception throw is
	/// influenced by something from outside (e.g. another property).
	/// Otherwise it's Warning.
	/// </summary>
	public abstract class TangerineValidationAttribute : Attribute
	{
		public abstract ValidationResult IsValid(object value, out string message);
	}

	public class TangerineValidRangeAttribute : TangerineValidationAttribute
	{
		public ValidationResult WarningLevel = ValidationResult.Warning;

		public object Minimum { get; private set; }
		public object Maximum { get; private set; }

		public TangerineValidRangeAttribute(int minimum, int maximum)
		{
			Maximum = maximum;
			Minimum = minimum;
		}

		public TangerineValidRangeAttribute(float minimum, float maximum)
		{
			Maximum = maximum;
			Minimum = minimum;
		}

		public override ValidationResult IsValid(object value, out string message)
		{
			var min = (IComparable)Minimum;
			var max = (IComparable)Maximum;
			message = min.CompareTo(value) <= 0 && max.CompareTo(value) >= 0 ? null : $"Value should be in range [{Minimum}, {Maximum}].";
			return message == null ? ValidationResult.Ok : WarningLevel;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class TangerineDefaultCharsetAttribute : TangerineValidationAttribute
	{
		private static readonly Regex regex = new Regex(@"\p{IsCyrillic}", RegexOptions.Compiled);

		public override ValidationResult IsValid(object value, out string message)
		{
			return IsValidPath(value as string, out message);
		}

		public static ValidationResult IsValidPath(string path, out string message)
		{
			message = path == null || path is string s && !regex.IsMatch(s) ? null : "Wrong charset";
			return message == null ? ValidationResult.Ok : ValidationResult.Warning;
		}
	}

	public class TangerineTileImageTextureAttribute : TangerineValidationAttribute
	{
		public override ValidationResult IsValid(object value, out string message)
		{
			var res = value is ITexture texture && (texture.IsStubTexture ||
			                                        !(texture.TextureParams.WrapModeU == TextureWrapMode.Clamp ||
			                                          texture.TextureParams.WrapModeV == TextureWrapMode.Clamp));
			message = res ? null : $"Texture of TiledImage should have WrapMode set to either Repeat or MirroredRepeat.";
			return res ? ValidationResult.Ok : ValidationResult.Warning;
		}
	}
}

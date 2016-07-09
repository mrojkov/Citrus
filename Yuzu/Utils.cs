using System;
using System.Linq;
using System.Reflection;

namespace Yuzu.Util
{
#if NET40
	internal static class Net4
	{
		public static bool IsDefined(this MemberInfo m, Type t)
		{
			return m.IsDefined(t, false);
		}

		public static Attribute GetCustomAttribute(this MemberInfo m, Type t, bool inherit)
		{
			var attrs = m.GetCustomAttributes(t, inherit);
			if (attrs.Count() > 1)
				throw new AmbiguousMatchException();
			return (Attribute)attrs.FirstOrDefault();
		}

		public static object GetValue(this PropertyInfo m, object obj)
		{
			return m.GetValue(obj, new object[] { });
		}
		public static void SetValue(this PropertyInfo m, object obj, object value)
		{
			m.SetValue(obj, value, new object[] { });
		}
	}
#endif

	internal class Utils
	{
		public static string QuoteCSharpStringLiteral(string s)
		{
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t");
		}

		public static string CodeValueFormat(object value)
		{
			var t = value.GetType();
			if (t == typeof(int) || t == typeof(uint) || t == typeof(float) || t == typeof(double))
				return value.ToString();
			if (t == typeof(bool))
				return value.ToString().ToLower();
			if (t == typeof(string))
				return '"' + QuoteCSharpStringLiteral(value.ToString()) + '"';
			return "";
			//throw new NotImplementedException();
		}

		public static bool IsStruct(Type t)
		{
			return t.IsValueType && !t.IsPrimitive && !t.IsEnum && !t.IsPointer;
		}

		public static MethodInfo GetPrivateGeneric(Type callerType, string name, Type parameter)
		{
			return callerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).
				MakeGenericMethod(parameter);
		}

		public static MethodInfo GetPrivateCovariantGeneric(Type callerType, string name, Type container)
		{
			var t = container.HasElementType ? container.GetElementType() : container.GetGenericArguments()[0];
			return callerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(t);
		}

		public static MethodInfo GetPrivateCovariantGenericAll(Type callerType, string name, Type container)
		{
			return
				callerType.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).
					MakeGenericMethod(container.GetGenericArguments());
		}

		public static bool IsCompact(Type t, CommonOptions options)
		{
			return t.IsDefined(options.CompactAttribute);
		}
	}
}

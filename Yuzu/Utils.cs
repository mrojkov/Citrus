using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yuzu.Util
{
	// Compatilility with .NET 4
	internal static class Net4
	{
		public static Attribute GetCustomAttribute_Compat(this MemberInfo m, Type t, bool inherit)
		{
			var attrs = m.GetCustomAttributes(t, inherit);
			if (attrs.Count() > 1)
				throw new AmbiguousMatchException();
			return (Attribute)attrs.FirstOrDefault();
		}
	}

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

		private static string DeclaringTypes(Type t, string separator)
		{
			return t.DeclaringType == null ? "" :
				DeclaringTypes(t.DeclaringType, separator) + t.DeclaringType.Name + separator;
		}

		public static string GetTypeSpec(Type t)
		{
			var p = "global::" + t.Namespace + ".";
			var n = DeclaringTypes(t, ".") + t.Name;
			if (!t.IsGenericType)
				return p + n;
			var args = String.Join(",", t.GetGenericArguments().Select(a => GetTypeSpec(a)));
			return p + String.Format("{0}<{1}>", n.Remove(n.IndexOf('`')), args);
		}

		public static string GetMangledTypeName(Type t)
		{
			var n = DeclaringTypes(t, "__") + t.Name;
			if (!t.IsGenericType)
				return n;
			var args = String.Join("__", t.GetGenericArguments().Select(a => GetMangledTypeName(a)));
			return n.Remove(n.IndexOf('`')) + "_" + args;
		}

	}
}

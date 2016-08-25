using System;
using System.Collections.Generic;
using System.IO;
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

	internal class CodeWriter
	{
		public StreamWriter Output;
		private int indentLevel = 0;
		public string IndentString = "\t";

		public void PutPart(string format, params object[] p)
		{
			var s = p.Length > 0 ? String.Format(format, p) : format;
			Output.Write(s.Replace("\n", "\r\n"));
		}

		public void Put(string format, params object[] p)
		{
			var s = p.Length > 0 ? String.Format(format, p) : format;
			if (s.StartsWith("}")) // "}\n" or "} while"
				indentLevel -= 1;
			if (s != "\n")
				for (int i = 0; i < indentLevel; ++i)
					PutPart(IndentString);
			PutPart(s);
			if (s.EndsWith("{\n"))
				indentLevel += 1;
		}

		// Check for explicit vs implicit interface implementation.
		public void PutAddToColllection(Type t, Type icoll, string collName, string elementName)
		{
			var imap = t.GetInterfaceMap(icoll);
			var addIndex = Array.FindIndex(imap.InterfaceMethods, m => m.Name == "Add");
			if (imap.TargetMethods[addIndex].Name == "Add")
				Put("{0}.Add({1});\n", collName, elementName);
			else
				Put("(({2}){0}).Add({1});\n", collName, elementName, Utils.GetTypeSpec(icoll));
		}
	}
}

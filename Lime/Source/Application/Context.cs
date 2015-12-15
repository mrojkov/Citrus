using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public struct ContextScope : IDisposable
	{
		public Context OldContext;

		public void Dispose()
		{
			OldContext.MakeCurrent();
		}
	}

	public class Context
	{
		private static Dictionary<Type, PropertyInfo> properties = new Dictionary<Type, PropertyInfo>();

		private Dictionary<PropertyInfo, object> snapshot = new Dictionary<PropertyInfo, object>();

		public static Context Current { get; private set; }

		static Context()
		{
			Current = new Context();
		}

		public ContextScope MakeCurrent()
		{
			Current = this;
			var scope = new ContextScope { OldContext = Current };
			foreach (var pair in snapshot) {
				pair.Key.SetValue(null, pair.Value);
			}
			return scope;
		}	

		public static void RegisterSingleton(Type type, string property)
		{
			properties[type] = type.GetProperty(property);
		}

		public void SetSingleton(Type type, object value)
		{
			snapshot[properties[type]] = value;
		}
	}
}


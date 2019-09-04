using System;
using System.Collections.Generic;

namespace Lime
{
	public class ServiceRegistry : IServiceProvider
	{
		private Dictionary<Type, object> services = new Dictionary<Type, object>();

		public void Add<T>(T service) where T : class
		{
			Add(typeof(T), service);
		}

		public void Add(Type type, object service)
		{
			if (service == null) {
				throw new ArgumentNullException(nameof(service));
			}
			services.Add(type, service);
		}

		public bool Remove<T>() where T : class
		{
			return Remove(typeof(T));
		}

		public bool Remove(Type type)
		{
			return services.Remove(type);
		}

		public object GetService(Type type)
		{
			return services.TryGetValue(type, out var service) ? service : null;
		}
	}
}

using System;

namespace Lime
{
	public static class ServiceProviderExtensions
	{
		public static T GetService<T>(this IServiceProvider provider) where T : class
		{
			return (T)provider.GetService(typeof(T));
		}

		public static T RequireService<T>(this IServiceProvider provider) where T : class
		{
			return provider.GetService<T>() ?? throw new InvalidOperationException();
		}
	}
}

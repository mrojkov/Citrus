using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemoteScripting
{
	public class PortableAssembly
	{
		private readonly List<PortableEntryPoint> entryPoints = new List<PortableEntryPoint>();

		public IReadOnlyList<PortableEntryPoint> EntryPoints => entryPoints;

		public PortableAssembly(byte[] assemblyRawBytes, byte[] pdbRawBytes, string entryPointsClassName)
		{
			var assembly = Assembly.Load(assemblyRawBytes, pdbRawBytes);
			var type = assembly.GetType(entryPointsClassName);
			foreach (var methodInfo in type.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public)) {
				var entryPointAttribute = methodInfo.GetCustomAttributes(true)
					.OfType<PortableEntryPointAttribute>()
					.FirstOrDefault();
				if (entryPointAttribute != null && methodInfo.GetParameters().Length == 0) {
					var entryPoint = new PortableEntryPoint(type.FullName, methodInfo.Name, methodInfo, entryPointAttribute.Summary) {
						Order = entryPointAttribute.Order
					};
					entryPoints.Add(entryPoint);
				}
			}
			entryPoints.Sort((lhs, rhs) => lhs.Order.CompareTo(rhs.Order));
		}
	}

	public class PortableEntryPoint
	{
		public string ClassName { get; }
		public string MethodName { get; }
		public MethodInfo Info { get; }
		public string Summary { get; }

		internal int Order { get; set; }

		public PortableEntryPoint(string className, string methodName, MethodInfo methodInfo, string summary)
		{
			ClassName = className;
			MethodName = methodName;
			Info = methodInfo;
			Summary = summary;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PortableEntryPointAttribute : Attribute
	{
		public readonly string Summary;
		public readonly int Order;

		public PortableEntryPointAttribute(string summary, int order = 0)
		{
			Summary = summary;
			Order = order;
		}
	}
}

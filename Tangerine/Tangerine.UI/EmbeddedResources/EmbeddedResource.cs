using System;
using System.Linq;
using System.Reflection;

namespace Tangerine.UI
{
	public class EmbeddedResource
	{
		public readonly string ResourceId;
		public readonly string AssemblyName;

		public EmbeddedResource(string resourceId, string assemblyName)
		{
			ResourceId = resourceId;
			AssemblyName = assemblyName;
		}

		protected Assembly GetAssembly()
		{
			var resourcesAssembly = AppDomain.CurrentDomain.GetAssemblies().
				SingleOrDefault(a => a.GetName().Name == AssemblyName);
			if (resourcesAssembly == null) {
				throw new Lime.Exception("Assembly '{0}' doesn't exist", AssemblyName);
			}
			return resourcesAssembly;
		}

		public virtual System.IO.Stream GetResourceStream()
		{
			return GetAssembly().GetManifestResourceStream(ResourceId);
		}

		public byte[] GetResourceBytes()
		{
			using (var ms = new System.IO.MemoryStream()) {
				GetResourceStream().CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orange
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

		public System.IO.Stream GetResourceStream()
		{
			var resourcesAssembly = AppDomain.CurrentDomain.GetAssemblies().
				SingleOrDefault(a => a.GetName().Name == AssemblyName);
			if (resourcesAssembly == null) {
				throw new Lime.Exception("Assembly '{0}' doesn't exist", AssemblyName);
			}
			return resourcesAssembly.GetManifestResourceStream(ResourceId);
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

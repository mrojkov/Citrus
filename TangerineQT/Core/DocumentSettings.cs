using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class DocumentSettings
	{
		public readonly Dictionary<string, ObjectSettings> Objects = new Dictionary<string, ObjectSettings>();

		public T GetObjectSettings<T>(string id) where T : ObjectSettings, new()
		{
			ObjectSettings o;
			if (!Objects.TryGetValue(id, out o)) {
				o = new T();
				Objects[id] = o;
			}
			return o as T;
		}
	}

	public class ObjectSettings
	{
	}

	public class NodeSettings : ObjectSettings
	{
		public bool IsFolded = true;
	}
}

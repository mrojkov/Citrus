using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Lime;

namespace Kumquat
{
	public static class Extensions
	{
		public static IEnumerable<T> Descendants<T>(this Node root) where T : Node
		{
			foreach (Node node in root.Nodes) {
				T result = node as T;
				if (result != null)
					yield return result;

				if (node.Nodes.Count > 0)
					foreach (var n in Descendants<T>(node))
						yield return n;
			}
		}

		public static string Tag(this Enum value) 
		{
			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField(value.ToString());
			Tag[] attribs = fieldInfo.GetCustomAttributes(typeof(Tag), false) as Tag[];
			return attribs.Length > 0 ? attribs[0].Value : null; 
		}

	}
}

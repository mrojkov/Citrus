using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	}
}

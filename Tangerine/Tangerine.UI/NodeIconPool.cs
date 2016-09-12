using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public static class NodeIconPool
	{
		static Dictionary<Type, ITexture> map = new Dictionary<Type, ITexture>();

		public static ITexture GetTexture(Node node)
		{
			ITexture texture;
			if (!map.TryGetValue(node.GetType(), out texture)) {
				texture = IconPool.GetTexture("Nodes." + node.GetType(), "Nodes.Unknown");
			}
			return texture;
		}
	}
}

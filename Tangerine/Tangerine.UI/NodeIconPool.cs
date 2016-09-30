using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public static class NodeIconPool
	{
		static Dictionary<Type, ITexture> map = new Dictionary<Type, ITexture>();

		public static ITexture GetTexture(Type nodeType)
		{
			ITexture texture;
			if (!map.TryGetValue(nodeType, out texture)) {
				texture = IconPool.GetTexture("Nodes." + nodeType, "Nodes.Unknown");
			}
			return texture;
		}
	}
}

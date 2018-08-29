using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public static class NodeIconPool
	{
		private static readonly Dictionary<Type, Icon> map = new Dictionary<Type, Icon>();

		public static Icon GetIcon(Type nodeType)
		{
			if (!map.TryGetValue(nodeType, out var icon)) {
				map[nodeType] = icon = IconPool.GetIcon("Nodes." + nodeType, "Nodes.Unknown");
			}
			return icon;
		}

		public static ITexture GetTexture(Type nodeType) => GetIcon(nodeType).AsTexture;
	}
}

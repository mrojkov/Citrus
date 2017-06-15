using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	class DragKeyframesRequest : Component
	{
		public bool RemoveOriginals { get; private set; }
		public IntVector2 Offset { get; private set; }

		public DragKeyframesRequest(IntVector2 offset, bool removeOriginals)
		{
			Offset = offset;
			RemoveOriginals = removeOriginals;
		}
	}
}
using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	class DragKeyframesRequest : IComponent
	{
		public IntVector2 Offset { get; private set; }

		public DragKeyframesRequest(IntVector2 offset)
		{
			this.Offset = offset;
		}
	}
}
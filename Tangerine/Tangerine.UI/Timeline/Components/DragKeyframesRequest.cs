using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	class DragKeyframesRequest : IComponent
	{
		public IntVector2 Offset { get; private set; }
		public GridSelection Selection { get; private set; }

		public DragKeyframesRequest(IntVector2 offset, GridSelection selection)
		{
			this.Offset = offset;
			this.Selection = selection;
		}
	}
}
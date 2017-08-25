using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	class HasKeyframeRequest : Component
	{
		public readonly IntVector2 Cell;
		public bool Result;

		public HasKeyframeRequest(IntVector2 cell)
		{
			this.Cell = cell;
		}
	}	
}

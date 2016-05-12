using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	class HasKeyframeRequest : IComponent
	{
		public readonly IntVector2 cell;
		public bool Result;

		public HasKeyframeRequest(IntVector2 cell)
		{
			this.cell = cell;
		}
	}	
}

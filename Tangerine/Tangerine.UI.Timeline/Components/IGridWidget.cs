using System;
using Tangerine.Core;
using Lime;

namespace Tangerine.UI.Timeline.Components
{
	public interface IGridWidget : IComponent
	{
		Widget Widget { get; }

		float Top { get; }
		float Bottom { get; }
		float Height { get; }
	}	
}

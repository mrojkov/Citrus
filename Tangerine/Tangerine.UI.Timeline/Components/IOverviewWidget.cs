using System;
using Tangerine.Core;
using Lime;

namespace Tangerine.UI.Timeline.Components
{
	public interface IOverviewWidget : IComponent
	{
		Widget Widget { get; }
	}
}
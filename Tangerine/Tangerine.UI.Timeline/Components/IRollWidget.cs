using System;
using Tangerine.Core;
using Lime;

namespace Tangerine.UI.Timeline.Components
{
	public interface IRollWidget : IComponent
	{
		Widget Widget { get; }
		float Indentation { set; }
	}
}

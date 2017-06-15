using System;
using Lime;

namespace Tangerine.UI.Timeline.Components
{
	public sealed class RowView : Component
	{
		public IRollRowView RollRow;
		public IGridRowView GridRow;
	}

	public interface IRollRowView
	{
		Widget Widget { get; }
		float Indentation { set; }
	}

	public interface IGridRowView
	{
		Widget GridWidget { get; }
		Widget OverviewWidget { get; }
	}
}

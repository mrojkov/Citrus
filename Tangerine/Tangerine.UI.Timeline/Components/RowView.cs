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
		AwakeBehavior AwakeBehavior { get; }
		float Indentation { set; }
		void Rename();
	}

	public interface IGridRowView
	{
		Widget GridWidget { get; }
		Widget OverviewWidget { get; }
		AwakeBehavior GridWidgetAwakeBehavior { get; }
		AwakeBehavior OverviewWidgetAwakeBehavior { get; }
	}
}

using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class CenterTimelineOnCurrentColumn
	{
		public static void Perform()
		{
			var timeline = Timeline.Instance;
			Document.Current.History.DoTransaction(() => {
				timeline.OffsetX = Mathf.Max(0, (timeline.CurrentColumn + 1) * TimelineMetrics.ColWidth - timeline.Grid.RootWidget.Width / 2);
				SetCurrentColumn.Perform(timeline.CurrentColumn);
			});
		}
	}
}

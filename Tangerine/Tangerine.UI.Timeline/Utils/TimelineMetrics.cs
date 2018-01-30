using Tangerine.UI.Timeline;

namespace Tangerine.UI
{
	public static class TimelineMetrics
	{
		public const float RollIndentation = 25;
		public static float ColWidth
		{
			get { return TimelineUserPreferences.Instance.ColWidth; }
			set { TimelineUserPreferences.Instance.ColWidth = value; }
		}
		public const float DefaultRowHeight = 25;
		public const float RowSpacing = 1;
		public const float ToolbarMinWidth = 50;
	}
}

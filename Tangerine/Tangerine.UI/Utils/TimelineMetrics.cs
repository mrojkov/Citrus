using System;

namespace Tangerine.UI
{
	public static class Metrics
	{
		public const float ToolbarHeight = 26;
	}

	public static class TimelineMetrics
	{
		public const float RollIndentation = 25;
		public static float ColWidth
		{
			get { return UserPreferences.Instance.TimelineColWidth; }
			set { UserPreferences.Instance.TimelineColWidth = value; }
		} 
		public const float DefaultRowHeight = 25;
		public const float RowSpacing = 1;
		public const float ToolbarMinWidth = 50;
	}
}

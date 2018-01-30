using System;
using System.Collections.Generic;
using Lime;
using Yuzu;

namespace Tangerine.UI.Timeline
{
	public class TimelineUserPreferences : Component
	{
		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		[YuzuRequired]
		public bool AnimationMode { get; set; }

		[YuzuRequired]
		public float ColWidth { get; set; }

		[YuzuRequired]
		public bool EditCurves { get; set; }

		[YuzuRequired]
		public List<float> TimelineVSplitterStretches;

		[YuzuRequired]
		public List<float> TimelineHSplitterStretches;

		public TimelineUserPreferences()
		{
			ResetToDefaults();
		}

		public void ResetToDefaults()
		{
			ColWidth = 15;
			EditCurves = true;
			AutoKeyframes = false;
			AnimationMode = false;
			TimelineVSplitterStretches = new List<float>();
			TimelineHSplitterStretches = new List<float>();
		}

		public static TimelineUserPreferences Instance => Core.UserPreferences.Instance.Get<TimelineUserPreferences>();
	}
}
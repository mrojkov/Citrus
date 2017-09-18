using System;
using System.Collections.Generic;
using Lime;
using Yuzu;

namespace Tangerine.UI.Timeline
{
	public class UserPreferences : Component
	{
		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		[YuzuRequired]
		public bool AnimationMode { get; set; }

		[YuzuRequired]
		public float ColWidth { get; set; } = 15;

		[YuzuRequired]
		public bool EditCurves { get; set; } = true;

		[YuzuRequired]
		public List<float> TimelineVSplitterStretches = new List<float>();

		[YuzuRequired]
		public List<float> TimelineHSplitterStretches = new List<float>();
	}
}
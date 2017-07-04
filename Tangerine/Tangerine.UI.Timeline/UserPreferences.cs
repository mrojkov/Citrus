using System;
using System.Collections.Generic;
using Yuzu;

namespace Tangerine.UI.Timeline
{
	public class UserPreferences
	{
		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		[YuzuRequired]
		public bool AnimationMode { get; set; }

		[YuzuRequired]
		public float ColWidth { get; set; } = 15;

		[YuzuRequired]
		public List<float> TimelineVSplitterStretches = new List<float>();

		[YuzuRequired]
		public List<float> TimelineHSplitterStretches = new List<float>();

		public static UserPreferences Instance;

		public UserPreferences()
		{

		}

		public UserPreferences(bool makeInstance)
		{
			if (makeInstance) {
				if (Instance != null) {
					throw new InvalidOperationException();
				}
				Instance = this;
			}
		}
	}
}
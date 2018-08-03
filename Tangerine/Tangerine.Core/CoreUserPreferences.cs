using Lime;
using Yuzu;

namespace Tangerine.Core
{
	public class CoreUserPreferences : Component
	{
		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		[YuzuRequired]
		public bool AnimationMode { get; set; }

		[YuzuRequired]
		public KeyFunction DefaultKeyFunction { get; set; }

		[YuzuRequired]
		public bool ReloadModifiedFiles { get; set; }

		public CoreUserPreferences()
		{
			ResetToDefaults();
		}

		public void ResetToDefaults()
		{
			AutoKeyframes = false;
			AnimationMode = false;
			DefaultKeyFunction = KeyFunction.Linear;
		}

		public static CoreUserPreferences Instance => UserPreferences.Instance.Get<CoreUserPreferences>();
	}
}

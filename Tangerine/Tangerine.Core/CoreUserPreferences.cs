using Lime;
using Yuzu;

namespace Tangerine.Core
{
	public class CoreUserPreferences : Component
	{
		[YuzuRequired]
		public bool AutoKeyframes { get; set; }

		public CoreUserPreferences()
		{
			ResetToDefaults();
		}

		public void ResetToDefaults()
		{
			AutoKeyframes = false;
		}

		public static CoreUserPreferences Instance => UserPreferences.Instance.Get<CoreUserPreferences>();
	}
}

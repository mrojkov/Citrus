using System;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class UserPreferences
	{
		public static UserPreferences Instance;

		[YuzuRequired]
		public bool ShowOverlays { get; set; }

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
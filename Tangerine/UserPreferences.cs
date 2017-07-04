using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine
{
	public class UserPreferences : Component
	{
		[YuzuRequired]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		[YuzuRequired]
		public readonly List<string> RecentProjects = new List<string>();

		[YuzuRequired]
		public ColorThemeEnum Theme { get; set; }

		[YuzuRequired]
		public Vector2 DefaultSceneDimensions { get; set; } = new Vector2(1024, 768);
	}
}
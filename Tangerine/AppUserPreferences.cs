using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine
{
	public class AppUserPreferences : Component
	{
		[YuzuRequired]
		public UI.DockManager.State DockState;

		[YuzuRequired]
		public readonly List<string> RecentProjects;

		[YuzuRequired]
		public ColorThemeEnum Theme { get; set; }

		[YuzuRequired]
		public Vector2 DefaultSceneDimensions { get; set; }

		/// <summary>
		/// Autosave delay in seconds
		/// </summary>
		[YuzuRequired]
		public int AutosaveDelay { get; set; }

		public AppUserPreferences()
		{
			DockState = new UI.DockManager.State();
			RecentProjects = new List<string>();
			ResetToDefaults();
		}

		internal void ResetToDefaults()
		{
			Theme = ColorThemeEnum.Light;
			DefaultSceneDimensions = new Vector2(1024, 768);
			AutosaveDelay = 600;
		}

		public static AppUserPreferences Instance => Core.UserPreferences.Instance.Get<AppUserPreferences>();
	}
}
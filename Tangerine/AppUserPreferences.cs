using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Yuzu;

namespace Tangerine
{
	public class AppUserPreferences : Component
	{
		[YuzuRequired]
		public UI.Docking.DockManager.State DockState;

		[YuzuRequired]
		public readonly List<string> RecentProjects;

		public static int RecentProjectsCount { get; private set; } = 5;

		[YuzuRequired]
		public UI.ColorTheme ColorTheme { get; set; }

		[YuzuRequired]
		public Theme.ColorTheme LimeColorTheme { get; set; }

		[YuzuRequired]
		public Vector2 DefaultSceneDimensions { get; set; }

		[YuzuRequired]
		public string CurrentHotkeyProfile { get; set; }

		/// <summary>
		/// Autosave delay in seconds
		/// </summary>
		[YuzuRequired]
		public int AutosaveDelay { get; set; }

		[YuzuRequired]
		public ToolbarLayout ToolbarLayout { get; set; }

		public AppUserPreferences()
		{
			DockState = new UI.Docking.DockManager.State();
			RecentProjects = new List<string>();
			ResetToDefaults();
		}

		internal void ResetToDefaults()
		{
			ColorTheme = UI.ColorTheme.CreateLightTheme();
			LimeColorTheme = Theme.ColorTheme.CreateLightTheme();
			DefaultSceneDimensions = new Vector2(1024, 768);
			AutosaveDelay = 600;
			ToolbarLayout = ToolbarLayout.DefaultToolbarLayout();
		}

		public static AppUserPreferences Instance => Core.UserPreferences.Instance.Get<AppUserPreferences>();
	}
}

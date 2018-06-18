using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Yuzu;

namespace Tangerine
{
	public class AppUserPreferences : Component
	{
		[YuzuRequired]
		public UI.Docking.DockManager.State DockState;

		[YuzuRequired]
		public readonly List<string> RecentProjects;

		[YuzuRequired]
		public readonly List<string> RecentDocuments;

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
			DockState = new UI.Docking.DockManager.State();
			RecentProjects = new List<string>();
			RecentDocuments = new List<string>();
			ResetToDefaults();
		}

		internal void ResetToDefaults()
		{
			Theme = ColorThemeEnum.Light;
			DefaultSceneDimensions = new Vector2(1024, 768);
			AutosaveDelay = 600;
		}

		public void AddRecentDocument(string path)
		{
			var prefs = Instance;
			Instance.RecentDocuments.Remove(path);
			prefs.RecentDocuments.Insert(0, path);
			UserPreferences.Instance.Save();
			TangerineMenu.RebuildRecentDocumentsMenu();
		}

		public void AddRecentProject(string path)
		{
			var prefs = Instance;
			Instance.RecentProjects.Remove(path);
			prefs.RecentProjects.Insert(0, path);
			UserPreferences.Instance.Save();
			TangerineMenu.RebuildRecentProjectsMenu();
		}

		public static AppUserPreferences Instance => Core.UserPreferences.Instance.Get<AppUserPreferences>();
	}
}

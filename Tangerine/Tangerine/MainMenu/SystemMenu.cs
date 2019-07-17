using Lime;
using Tangerine.UI;
using System.IO;
using Tangerine.Core;
using Tangerine.Panels;
using Console = System.Console;
using Environment = Lime.Environment;

namespace Tangerine.MainMenu
{
	public class PurgeBackUps : CommandHandler
	{
		public override void Execute()
		{
			if (new AlertDialog("Are you sure you want to purge all backups?", "Yes", "Cancel").Show() == 0) {
				var path = Path.Combine(Environment.GetDataDirectory("Tangerine"), "Backups",
					Path.GetFileNameWithoutExtension(Project.Current?.CitprojPath ?? ""));
				if (Directory.Exists(path)) {
					Directory.Delete(path, true);
					BackupHistoryPanel.Instance.RefreshHistory();
				}
			}
		}
	}

	public class ClearCache : CommandHandler
	{
		public override void Execute()
		{
			if (
				new AlertDialog("Are you sure you want to clear project cache?", "Yes", "Cancel").Show() == 0 &&
				File.Exists(Orange.The.Workspace.TangerineCacheBundle)
			) {
				File.Delete(Orange.The.Workspace.TangerineCacheBundle);
			}
		}
	}

	public class ResetGlobalSettings : CommandHandler
	{
		public override void Execute()
		{
			if (new AlertDialog("Are you sure you want to reset to defaults?", "Yes", "Cancel").Show() == 0) {
				AppUserPreferences.Instance.ResetToDefaults();
				UI.SceneView.SceneUserPreferences.Instance.ResetToDefaults();
				UI.Timeline.TimelineUserPreferences.Instance.ResetToDefaults();
				Core.CoreUserPreferences.Instance.ResetToDefaults();
				HotkeyRegistry.ResetToDefaults();
				AppUserPreferences.Instance.ToolbarModel = AppUserPreferences.DefaultToolbarModel();
				new AlertDialog("Some actions will take place after Tangerine restart", "Ok").Show();
			}
		}
	}
}

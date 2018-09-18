using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine
{
	public class SaveRuler : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			return SceneViewCommands.ToggleDisplayRuler.Checked &&
			       ProjectUserPreferences.Instance.ActiveRuler.Lines.Count > 0;
		}

		public override void ExecuteTransaction()
		{
			var ruler = ProjectUserPreferences.Instance.ActiveRuler;
			if (!new SaveRulerDialog(ruler).Show()) {
				return;
			}
			if (ProjectUserPreferences.Instance.Rulers.Any(o => o.Name == ruler.Name)) {
				new AlertDialog("Ruler with exact name already exist").Show();
				ruler.Name = string.Empty;
				ruler.AnchorToRoot = false;
			} else {
				if (ruler.AnchorToRoot) {
					var size = Document.Current.Container.AsWidget.Size / 2;
					foreach (var l in ruler.Lines) {
						l.Value -= (l.RulerOrientation == RulerOrientation.Horizontal ? size.Y : size.X);
					}
				}
				ProjectUserPreferences.Instance.SaveActiveRuler();
			}
		}
	}

	public class DisplayRuler : DocumentCommandHandler
	{
		public override bool GetChecked() => ProjectUserPreferences.Instance.RulerVisible;

		public override void ExecuteTransaction()
		{
			ProjectUserPreferences.Instance.RulerVisible = !ProjectUserPreferences.Instance.RulerVisible;
		}
	}

	public class SnapWidgetPivotCommandHandler : DocumentCommandHandler
	{
		public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapWidgetPivotToRuler;
		public override void ExecuteTransaction()
		{
			var prefs = UI.SceneView.SceneUserPreferences.Instance;
			prefs.SnapWidgetPivotToRuler = !prefs.SnapWidgetPivotToRuler;
		}
	}

	public class SnapWidgetBorderCommandHandler : DocumentCommandHandler
	{
		public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapWidgetBorderToRuler;
		public override void ExecuteTransaction()
		{
			var prefs = UI.SceneView.SceneUserPreferences.Instance;
			prefs.SnapWidgetBorderToRuler = !prefs.SnapWidgetBorderToRuler;
		}
	}

	public class SnapRulerLinesToWidgetCommandHandler : DocumentCommandHandler
	{
		public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapRulerLinesToWidgets;
		public override void ExecuteTransaction()
		{
			var prefs = UI.SceneView.SceneUserPreferences.Instance;
			prefs.SnapRulerLinesToWidgets = !prefs.SnapRulerLinesToWidgets;
		}
	}

	public class PanelCommandHandler : CommandHandler
	{
		private readonly string panelId;
		private PanelPlacement placement => DockManager.Instance.Model.FindPanelPlacement(panelId);

		public override void RefreshCommand(ICommand command)
		{
			command.Checked = !placement.Hidden;
		}

		public PanelCommandHandler(string panelId)
		{
			this.panelId = panelId;
		}

		public override void Execute()
		{
			placement.Hidden = !placement.Hidden;
			DockManager.Instance.Refresh();
		}
	}
}

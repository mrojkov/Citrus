using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class OverviewWidgetsUpdater : SymmetricOperationProcessor
	{
		private Timeline timeline => Timeline.Instance;

		public override void Process(IOperation op)
		{
			if (!AreWidgetsValid()) {
				ResetWidgets();
			}
			AdjustWidths();
		}

		private static void AdjustWidths()
		{
			foreach (var row in Document.Current.Rows) {
				var gw = row.Components.Get<Components.RowView>().GridRow;
				gw.OverviewWidget.MinWidth = Timeline.Instance.ColumnCount * TimelineMetrics.ColWidth;
			}
		}

		private void ResetWidgets()
		{
			var content = timeline.Overview.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var gridRow = row.Components.Get<Components.RowView>().GridRow;
				var widget = gridRow.OverviewWidget;
				if (!gridRow.OverviewWidgetAwakeBehavior.IsAwoken) {
					gridRow.OverviewWidgetAwakeBehavior.Update(0);
				}
				content.AddNode(widget);
			}
		}

		private bool AreWidgetsValid()
		{
			var content = timeline.Overview.ContentWidget;
			if (Document.Current.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<Components.RowView>().GridRow.OverviewWidget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}

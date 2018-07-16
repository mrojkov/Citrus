using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridWidgetsUpdater : SymmetricOperationProcessor
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
				row.GridWidget().MinWidth = Timeline.Instance.ColumnCount * TimelineMetrics.ColWidth;
			}
		}

		private void ResetWidgets()
		{
			var content = timeline.Grid.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var gridRow = row.Components.Get<Components.RowView>().GridRow;
				var widget = gridRow.GridWidget;
				if (!gridRow.GridWidgetAwakeBehavior.IsAwoken) {
					widget.Update(0);
				}
				content.AddNode(widget);
			}
			// Layout widgets in order to have valid row positions and sizes, which are used for rows visibility determination.
			Lime.WidgetContext.Current.Root.LayoutManager.Layout();
		}

		private bool AreWidgetsValid()
		{
			var content = timeline.Grid.ContentWidget;
			if (Document.Current.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in Document.Current.Rows) {
				if (row.GridWidget() != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}


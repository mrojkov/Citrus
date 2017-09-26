using System;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridWidgetsUpdater : SymmetricOperationProcessor
	{
		Timeline timeline => Timeline.Instance;

		public override void Process(IOperation op)
		{
			if (!AreWidgetsValid()) {
				ResetWidgets();
			}
			AdjustWidths();
		}

		void AdjustWidths()
		{
			foreach (var row in Document.Current.Rows) {
				row.GridWidget().MinWidth = Timeline.Instance.ColumnCount * TimelineMetrics.ColWidth;
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Grid.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var widget = row.GridWidget();
				if (!widget.IsAwoken) {
					widget.Update(0);
				}
				content.AddNode(widget);
			}
			// Layout widgets in order to have valid row positions and sizes, which are used for rows visibility determination.
			Lime.WidgetContext.Current.Root.LayoutManager.Layout();
		}

		bool AreWidgetsValid()
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


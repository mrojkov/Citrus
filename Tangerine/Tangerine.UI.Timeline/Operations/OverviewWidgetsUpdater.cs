using System;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class OverviewWidgetsUpdater : SymmetricOperationProcessor
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
				var gw = row.Components.Get<Components.RowView>().GridRow;
				gw.OverviewWidget.MinWidth = Timeline.Instance.ColumnCount * TimelineMetrics.ColWidth;
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Overview.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var widget = row.Components.Get<Components.RowView>().GridRow.OverviewWidget;
				if (!widget.IsAwake) {
					widget.Update(0);
				}
				content.AddNode(widget);
			}
		}

		bool AreWidgetsValid()
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

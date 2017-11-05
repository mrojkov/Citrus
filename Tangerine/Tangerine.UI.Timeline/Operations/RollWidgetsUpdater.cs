using System;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RollWidgetsUpdater : SymmetricOperationProcessor
	{
		Timeline timeline => Timeline.Instance;

		public override void Process(IOperation op)
		{
			if (!AreWidgetsValid()) {
				ResetWidgets();
			}
			UpdateIndentation();
		}

		void UpdateIndentation()
		{
			foreach (var row in Document.Current.Rows) {
				row.Components.Get<Components.RowView>().RollRow.Indentation = CalcIndentation(row) * TimelineMetrics.RollIndentation;
			}
		}

		int CalcIndentation(Row row)
		{
			int i = 0;
			for (var r = row.Parent; r != null; r = r.Parent) {
				i++;
			}
			return i - 1;
		}

		void ResetWidgets()
		{
			var content = timeline.Roll.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				var widget = row.Components.Get<Components.RowView>().RollRow.Widget;
				if (!widget.IsAwake) {
					widget.Update(0);
				}
				content.AddNode(widget);
			}
		}

		bool AreWidgetsValid()
		{
			var content = timeline.Roll.ContentWidget;
			if (Document.Current.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<Components.RowView>().RollRow.Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}

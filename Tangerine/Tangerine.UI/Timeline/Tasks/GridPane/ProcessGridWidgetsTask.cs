using System;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class ProcessGridWidgetsTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			while (true) {
				if (!AreWidgetsValid()) {
					ResetWidgets();
				}
				AdjustWidths();
				yield return null;
			}
		}

		void AdjustWidths()
		{
			foreach (var row in timeline.Rows) {
				var gw = row.Components.Get<Components.IGridWidget>();
				gw.Widget.MinWidth = Timeline.Instance.ColumnCount * Metrics.ColWidth;
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Grid.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in timeline.Rows) {
				content.AddNode(row.Components.Get<Components.IGridWidget>().Widget);
			}
		}

		bool AreWidgetsValid()
		{
			var content = timeline.Grid.ContentWidget;
			if (timeline.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in timeline.Rows) {
				if (row.Components.Get<Components.IGridWidget>().Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}


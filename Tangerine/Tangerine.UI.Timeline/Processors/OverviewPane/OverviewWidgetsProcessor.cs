using System;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class OverviewWidgetsProcessor : Core.IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
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
				var gw = row.Components.Get<Components.IOverviewWidget>();
				gw.Widget.MinWidth = Timeline.Instance.ColumnCount * Metrics.TimelineColWidth;
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Overview.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in timeline.Rows) {
				content.AddNode(row.Components.Get<Components.IOverviewWidget>().Widget);
			}
		}

		bool AreWidgetsValid()
		{
			var content = timeline.Overview.ContentWidget;
			if (timeline.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in timeline.Rows) {
				if (row.Components.Get<Components.IOverviewWidget>().Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}

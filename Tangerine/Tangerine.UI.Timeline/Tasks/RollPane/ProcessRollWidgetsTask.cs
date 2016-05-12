using System;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class ProcessRollWidgetsTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			while (true) {
				if (!AreWidgetsValid()) {
					ResetWidgets();
				}
				yield return null;
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Roll.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in timeline.Rows) {
				content.AddNode(row.Components.Get<Components.IRollWidget>().Widget);
			}
		}

		bool AreWidgetsValid()
		{
			var content = timeline.Roll.ContentWidget;
			if (timeline.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in timeline.Rows) {
				if (row.Components.Get<Components.IRollWidget>().Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}

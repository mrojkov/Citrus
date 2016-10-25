using System;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RollWidgetsUpdater : IDocumentUpdater
	{
		Timeline timeline => Timeline.Instance;

		public void Update()
		{
			if (!AreWidgetsValid()) {
				ResetWidgets();
			}
		}

		void ResetWidgets()
		{
			var content = timeline.Roll.ContentWidget;
			content.Nodes.Clear();
			foreach (var row in Document.Current.Rows) {
				content.AddNode(row.Components.Get<Components.IRollWidget>().Widget);
			}
		}

		bool AreWidgetsValid()
		{
			var content = timeline.Roll.ContentWidget;
			if (Document.Current.Rows.Count != content.Nodes.Count) {
				return false;
			}
			foreach (var row in Document.Current.Rows) {
				if (row.Components.Get<Components.IRollWidget>().Widget != content.Nodes[row.Index]) {
					return false;
				}
			}
			return true;
		}
	}
}

using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class UnlinkedNodesDeselector : IDocumentUpdater
	{
		Timeline timeline => Timeline.Instance;

		public void Update()
		{
			List<Row> rowsToUnlink = null;
			foreach (var row in Document.Current.SelectedRows) {
				var node = row.Components.Get<Core.Components.NodeRow>()?.Node;
				if (node != null && node.Parent != timeline.Container) {
					rowsToUnlink = rowsToUnlink ?? new List<Row>();
					rowsToUnlink.Add(row);
				}
			}
			if (rowsToUnlink != null) {
				foreach (var row in rowsToUnlink) {
					Core.Operations.SelectRow.Perform(row, false);
				}
			}
		}
	}
}
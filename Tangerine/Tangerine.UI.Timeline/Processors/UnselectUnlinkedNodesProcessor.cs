using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class UnselectUnlinkedNodesProcessor : IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				List<Row> rowsToUnlink = null;
				foreach (var row in timeline.SelectedRows) {
					var node = row.Components.Get<Components.NodeRow>()?.Node;
					if (node != null && node.Parent != timeline.Container) {
						rowsToUnlink = rowsToUnlink ?? new List<Row>();
						rowsToUnlink.Add(row);
					}
				}
				if (rowsToUnlink != null) {
					foreach (var row in rowsToUnlink) {
						Operations.SelectRow.Perform(row, false);
					}
				}
				yield return null;
			}
		}
	}
}
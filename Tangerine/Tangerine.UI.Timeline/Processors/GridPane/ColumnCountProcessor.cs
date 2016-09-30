using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class ColumnCountProcessor : IProcessor
	{
		public IEnumerator<object> Loop()
		{
			var timeline = Timeline.Instance;
			while (true) {
				var rows = Document.Current.Rows;
				int maxColumn = 0;
				foreach (var row in rows) {
					var nodeData = row.Components.Get<Core.Components.NodeRow>();
					if (nodeData != null) {
						maxColumn = Math.Max(maxColumn, nodeData.Node.Animators.GetOverallDuration());
					}
				}
				var maxVisibleColumn = ((timeline.ScrollOrigin.X + timeline.Grid.Size.X) / Metrics.ColWidth).Ceiling();
				timeline.ColumnCount = Math.Max(maxColumn + 1, maxVisibleColumn);
				yield return null;
			}
		}
	}
}
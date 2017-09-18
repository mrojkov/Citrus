using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class ColumnCountUpdater : SymmetricOperationProcessor
	{
		const int ExtraFramesCount = 100;

		public override void Process(IOperation op)
		{
			var timeline = Timeline.Instance;
			var rows = Document.Current.Rows;
			var container = Document.Current.Container;
			int maxColumn = 0;
			foreach (var row in rows) {
				var nodeData = row.Components.Get<Core.Components.NodeRow>();
				if (nodeData != null) {
					int maxMarkerColumn = 0;
					var maxAnimatorColumn = Math.Max(maxColumn, nodeData.Node.Animators.GetOverallDuration() + ExtraFramesCount);
					if (container.Markers.Count > 0) {
						maxMarkerColumn = Math.Max(maxColumn, container.Markers[container.Markers.Count - 1].Frame + ExtraFramesCount);
					}
					maxColumn = Math.Max(maxMarkerColumn, maxAnimatorColumn);
				}
			}
			var maxVisibleColumn = ((timeline.OffsetX + timeline.Ruler.RootWidget.Width) / TimelineMetrics.ColWidth).Ceiling();
			timeline.ColumnCount = Math.Max(maxColumn + 1, maxVisibleColumn);
		}
	}
}
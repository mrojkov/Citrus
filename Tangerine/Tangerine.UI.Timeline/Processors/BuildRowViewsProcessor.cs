using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class BuildRowViewsProcessor : IProcessor
	{
		public IEnumerator<object> Loop()
		{
			while (true) {
				foreach (var row in Document.Current.Rows) {
					TryCreateGridNodeView(row);
					TryCreateGridPropertyView(row);
					TryCreateGridCurveView(row);
					TryCreateRollNodeView(row);
					TryCreateRollPropertyView(row);
					TryCreateRollCurveView(row);
				}
				yield return null;
			}
		}

		static void TryCreateRollNodeView(Row row)
		{
			var nodeRow = row.Components.Get<Core.Components.NodeRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && nodeRow != null) {
				view = new RollNodeView(row, 0);
				row.Components.Add<IRollWidget>(view);
			}
		}

		static void TryCreateRollPropertyView(Row row)
		{
			var propRow = row.Components.Get<Core.Components.PropertyRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && propRow != null) {
				view = new RollPropertyView(row, 1);
				row.Components.Add<IRollWidget>(view);
			}
		}

		static void TryCreateRollCurveView(Row row)
		{
			var curveRow = row.Components.Get<Core.Components.CurveRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && curveRow != null) {
				view = new RollCurveView(row, 2);
				row.Components.Add<IRollWidget>(view);
			}
		}

		static void TryCreateGridNodeView(Row row)
		{
			var nodeRow = row.Components.Get<Core.Components.NodeRow>();
			if (nodeRow != null && !row.Components.Has<IGridWidget>()) {
				var c = new GridNodeView(nodeRow.Node);
				row.Components.Add<IGridWidget>(c);
				row.Components.Add<IOverviewWidget>(c);
			}
		}

		static void TryCreateGridPropertyView(Row row)
		{
			var propRow = row.Components.Get<Core.Components.PropertyRow>();
			if (propRow != null && !row.Components.Has<IGridWidget>()) {
				var c = new GridPropertyView(propRow.Node, propRow.Animator);
				row.Components.Add<IGridWidget>(c);
				row.Components.Add<IOverviewWidget>(c);
			}
		}

		static void TryCreateGridCurveView(Row row)
		{
			var curveRow = row.Components.Get<Core.Components.CurveRow>();
			if (curveRow != null && !row.Components.Has<IGridWidget>()) {
				var c = new GridCurveView(curveRow.Node, curveRow.Animator, curveRow.State);
				row.Components.Add<IGridWidget>(c);
				row.Components.Add<IOverviewWidget>(c);
			}
		}
	}
}
using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class RowViewsUpdater : SymmetricOperationProcessor
	{
		public override void Process(IOperation op)
		{
			foreach (var row in Document.Current.Rows) {
				TryCreateGridNodeView(row);
				TryCreateGridFolderView(row);
				TryCreateGridPropertyView(row);
				TryCreateGridCurveView(row);
				TryCreateRollNodeView(row);
				TryCreateRollFolderView(row);
				TryCreateRollPropertyView(row);
				TryCreateRollCurveView(row);
			}
		}

		static void TryCreateRollNodeView(Row row)
		{
			var nodeRow = row.Components.Get<Core.Components.NodeRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && nodeRow != null) {
				view = new RollNodeView(row);
				row.Components.Add(view);
			}
		}

		static void TryCreateRollFolderView(Row row)
		{
			var folderRow = row.Components.Get<Core.Components.FolderRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && folderRow != null) {
				view = new RollFolderView(row);
				row.Components.Add(view);
			}
		}

		static void TryCreateRollPropertyView(Row row)
		{
			var propRow = row.Components.Get<Core.Components.PropertyRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && propRow != null) {
				view = new RollPropertyView(row);
				row.Components.Add(view);
			}
		}

		static void TryCreateRollCurveView(Row row)
		{
			var curveRow = row.Components.Get<Core.Components.CurveRow>();
			var view = row.Components.Get<IRollWidget>();
			if (view == null && curveRow != null) {
				view = new RollCurveView(row);
				row.Components.Add(view);
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

		static void TryCreateGridFolderView(Row row)
		{
			var folderRow = row.Components.Get<Core.Components.FolderRow>();
			if (folderRow != null && !row.Components.Has<IGridWidget>()) {
				var c = new GridNodeView(folderRow.Node);
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
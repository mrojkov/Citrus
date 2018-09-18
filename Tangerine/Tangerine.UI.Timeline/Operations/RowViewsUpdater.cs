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
				var view = row.Components.GetOrAdd<RowView>();
				TryCreateRollView(row);
				TryCreateGridView(row);
			}
		}

		static void TryCreateRollView(Row row)
		{
			var view = row.Components.GetOrAdd<RowView>();
			if (view.RollRow != null) {
				return;
			}
			if (row.Components.Contains<Core.Components.BoneRow>()) {
				view.RollRow = new RollBoneView(row);
			} else if (row.Components.Contains<Core.Components.NodeRow>()) {
				view.RollRow = new RollNodeView(row);
			} else if (row.Components.Contains<Core.Components.FolderRow>()) {
				view.RollRow = new RollFolderView(row);
			} else if (row.Components.Contains<Core.Components.PropertyRow>()) {
				view.RollRow = new RollPropertyView(row);
			}
		}

		static void TryCreateGridView(Row row)
		{
			var view = row.Components.GetOrAdd<RowView>();
			if (view.GridRow != null) {
				return;
			}
			if (row.Components.Contains<Core.Components.BoneRow>()) {
				var nodeRow = row.Components.Get<Core.Components.NodeRow>();
				view.GridRow = new GridNodeView(nodeRow.Node);
			}
			if (row.Components.Contains<Core.Components.NodeRow>()) {
				var nodeRow = row.Components.Get<Core.Components.NodeRow>();
				if (nodeRow.Node is Audio) {
					view.GridRow = new GridAudioView((Audio)nodeRow.Node);
				} else {
					view.GridRow = new GridNodeView(nodeRow.Node);
				}
			} else if (row.Components.Contains<Core.Components.FolderRow>()) {
				view.GridRow = new GridFolderView();
			} else if (row.Components.Contains<Core.Components.PropertyRow>()) {
				var propRow = row.Components.Get<Core.Components.PropertyRow>();
				view.GridRow = new GridPropertyView(propRow.Node, propRow.Animator);
			}
		}
	}
}
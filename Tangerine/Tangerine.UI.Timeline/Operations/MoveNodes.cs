using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline
{
	public static class MoveNodesUp
	{
		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				var rows = Document.Current.TopLevelSelectedRows().ToList();
				int prevIndex = -1;
				for (int i = 0; i < rows.Count; ++i) {
					var row = rows[i];
					IFolderItem item = null;
					var nr = row.Components.Get<NodeRow>();
					if (nr != null) {
						item = nr.Node;
					}
					var fr = row.Components.Get<FolderRow>();
					if (fr != null) {
						item = fr.Folder;
					}
					var oldLoc = Row.GetFolderItemLocation(row);
					var newLoc = new FolderItemLocation(oldLoc.Folder, oldLoc.Index - 1);
					if (newLoc.Index < 0 || newLoc.Index == prevIndex) {
						prevIndex = oldLoc.Index;
						continue;
					}
					Core.Operations.MoveNodes.Perform(item, newLoc);
					prevIndex = newLoc.Index;
				}
			});
		}
	}

	public static class MoveNodesDown
	{
		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				var rows = Document.Current.TopLevelSelectedRows().ToList();
				int prevIndex = -1;
				for (int i = rows.Count - 1; i >= 0; --i) {
					var row = rows[i];
					IFolderItem item = null;
					var nr = row.Components.Get<NodeRow>();
					if (nr != null) {
						item = nr.Node;
					}
					var fr = row.Components.Get<FolderRow>();
					if (fr != null) {
						item = fr.Folder;
					}
					var oldLoc = Row.GetFolderItemLocation(row);
					var newLoc = new FolderItemLocation(oldLoc.Folder, oldLoc.Index + 2);
					if (newLoc.Index > newLoc.Folder.Items.Count || prevIndex == newLoc.Index - 1) {
						prevIndex = oldLoc.Index;
						continue;
					}
					prevIndex = newLoc.Index;
					Core.Operations.MoveNodes.Perform(item, newLoc);
				}
			});
		}
	}
}

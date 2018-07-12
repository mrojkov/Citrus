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
					if (newLoc.Index < 0) {
						continue;
					}
					Core.Operations.MoveNodes.Perform(item, newLoc);
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
					if (newLoc.Index > Document.Current.Rows.Count) {
						continue;
					}
					Core.Operations.MoveNodes.Perform(item, newLoc);
				}
			});
		}
	}
}

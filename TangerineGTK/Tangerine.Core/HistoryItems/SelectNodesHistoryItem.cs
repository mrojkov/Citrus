using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pinta.Core
{
	public class SelectNodesHistoryItem : BaseHistoryItem
	{
		List<Lime.Node> initialSelection;
		List<Lime.Node> updatedSelection;

		public override bool CausesDirty { get { return false; } }

		public SelectNodesHistoryItem(IEnumerable<Lime.Node> nodes, bool append)
		{
			initialSelection = PintaCore.Workspace.ActiveDocument.Selection.ToList();
			updatedSelection = new List<Lime.Node>(nodes);
			if (append) {
				foreach (var i in initialSelection) {
					if (!updatedSelection.Contains(i)) {
						updatedSelection.Add(i);
					}
				}
			}
			Redo();
			PintaCore.Workspace.OnDocumentModified(new DocumentEventArgs(PintaCore.Workspace.ActiveDocument));
		}

		public override void Redo()
		{
			PintaCore.Workspace.ActiveDocument.Selection.Clear();
			PintaCore.Workspace.ActiveDocument.Selection.AddRange(updatedSelection);
		}

		public override void Undo()
		{
			PintaCore.Workspace.ActiveDocument.Selection.Clear();
			PintaCore.Workspace.ActiveDocument.Selection.AddRange(initialSelection);
		}
	}
}

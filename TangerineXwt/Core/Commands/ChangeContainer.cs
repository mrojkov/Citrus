using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Commands
{
	public class ChangeContainer : Command
	{
		Lime.Node savedContainer;
		Lime.Node container;
		Command selectRows;

		public ChangeContainer(Lime.Node container)
		{
			this.container = container;
		}

		public override void Do()
		{
			savedContainer = The.Document.Container;
			The.Document.Container = container;
			The.Document.RebuildRows();
			selectRows = new SelectRows(0);
			if (container.Nodes.Contains(savedContainer)) {
				// Если выходим наверх, то выставим в качестве текущего нода, тот откуда мы вышли
				foreach (var row in The.Document.Rows) {
					var nodeRow = row as Timeline.NodeRow;
					if (nodeRow != null && nodeRow.Node == savedContainer) {
						selectRows = new SelectRows(row.Index);
					}
				}
			}
			selectRows.Do();
		}

		public override void Undo()
		{
			selectRows.Undo();
			The.Document.Container = savedContainer;
		}
	}
}

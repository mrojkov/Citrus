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
		Command selectLines;

		public ChangeContainer(Lime.Node container)
		{
			this.container = container;
		}

		public override void Do()
		{
			savedContainer = The.Document.Container;
			The.Document.Container = container;
			if (container.Nodes.Contains(savedContainer)) {
				var linesCache = The.Timeline.LinesBuilder;
				var lines = linesCache.BuildLines(container);
				int i = lines.IndexOf(linesCache.GetNodeLine(savedContainer));
				selectLines = new SelectLines(i);
			} else {
				selectLines = new SelectLines(0);
			}
			selectLines.Do();
		}

		public override void Undo()
		{
			selectLines.Undo();
			The.Document.Container = savedContainer;
		}
	}
}

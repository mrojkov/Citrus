using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Commands
{
	public class SelectLines : Command
	{
		int[] lines;
		int[] savedLines;
		int savedTopLine;

		public SelectLines(params int[] lines)
		{
			Array.Sort(lines);
			this.lines = lines;
		}

		public override void Do()
		{
			savedTopLine = The.Timeline.TopLine;
			savedLines = The.Document.SelectedLines.ToArray();
			The.Document.SelectedLines.Clear();
			The.Document.SelectedLines.AddRange(lines);
			The.Timeline.EnsureLineVisible(lines[0]);
		}

		public override void Undo()
		{
			The.Timeline.TopLine = savedTopLine;
			The.Document.SelectedLines.Clear();
			The.Document.SelectedLines.AddRange(savedLines);
		}
	}
}

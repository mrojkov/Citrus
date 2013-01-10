using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Commands
{
	public class SelectRows : Command
	{
		int[] lines;
		int[] savedLines;
		int savedTopLine;

		public SelectRows(params int[] lines)
		{
			Array.Sort(lines);
			this.lines = lines;
		}

		public override void Do()
		{
			savedTopLine = The.Document.TopRow;
			savedLines = The.Document.SelectedRows.ToArray();
			The.Document.SelectedRows.Clear();
			The.Document.SelectedRows.AddRange(lines);
			The.Timeline.EnsureRowVisible(lines[0]);
		}

		public override void Undo()
		{
			The.Document.TopRow = savedTopLine;
			The.Document.SelectedRows.Clear();
			The.Document.SelectedRows.AddRange(savedLines);
		}
	}
}

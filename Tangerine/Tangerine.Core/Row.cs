using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public struct RowLocation
	{
		public Row ParentRow;
		public int Index;

		public RowLocation(Row parentRow, int index)
		{
			ParentRow = parentRow;
			Index = index;
		}
	}

	public class Row
	{
		public int Index { get; set; }
		public bool Selected => SelectCounter != 0;
		public int SelectCounter { get; set; }
		public bool CanHaveChildren { get; set; }
		public Row Parent { get; set; }
		public readonly List<Row> Rows = new List<Row>();
		public readonly ComponentCollection<Component> Components = new ComponentCollection<Component>();
	}
}
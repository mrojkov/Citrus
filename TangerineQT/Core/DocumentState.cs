using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public partial class Document
	{
		private TimelineRowsCache rowsBuilder = new TimelineRowsCache();

		public List<Timeline.Row> Rows = new List<Timeline.Row>();
		public List<int> SelectedRows = new List<int>();
		public int TopRow;
		public int LeftColumn;
		
		public int RightColumn 
		{
			get { return Timeline.Toolbox.CalcNumberOfVisibleColumns() + LeftColumn - 1; } 
		}
		
		public int BottomRow
		{
			get { return Timeline.Toolbox.CalcNumberOfVisibleRows() + TopRow - 1; }
		}

		public int CurrentColumn;
		public Lime.Node RootNode { get; private set; }
		public Lime.Node Container { get; set; }
		public GridSelection SelectedCells = new GridSelection();
		public int RowHeight { get { return The.Preferences.TimelineRowHeight; } }
		public int ColumnWidth { get { return The.Preferences.TimelineColWidth; } }

		public void RebuildRows()
		{
			Rows = rowsBuilder.GetRowsForContainer(Container);
		}
	}
}

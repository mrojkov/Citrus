using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public partial class Document
	{
		private TimelineRowsCache rowsBuilder = new TimelineRowsCache();
		
		public List<TimelineRow> Rows = new List<TimelineRow>();
		public List<int> SelectedRows = new List<int>();
		public int TopRow;
		public int LeftColumn;
		public int CurrentColumn;
		public Lime.Node RootNode { get; private set; }
		public Lime.Node Container { get; set; }
		public int RowHeight { get { return The.Preferences.TimelineRowHeight; } }
		public int ColumnWidth { get { return The.Preferences.TimelineColWidth; } }

		public void RebuildRows()
		{
			Rows = rowsBuilder.GetRowsForContainer(Container);
		}
	}
}

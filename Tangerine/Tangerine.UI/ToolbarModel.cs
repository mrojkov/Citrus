using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Tangerine.UI
{
	public class ToolbarModel
	{
		public class ToolbarPanel
		{
			[YuzuRequired]
			public List<string> CommandIds { get; } = new List<string>();

			[YuzuOptional]
			public string Title { get; set; } = "Panel";

			public ToolbarRow Parent { get; set; }
			public int Index { get; set; }
			public bool Draggable { get; set; } = true;

			public bool ContainsId(string id) => CommandIds.Contains(id);
		}

		public class ToolbarRow
		{
			[YuzuRequired]
			public List<ToolbarPanel> Panels { get; } = new List<ToolbarPanel>();

			public ToolbarModel Parent { get; set; }
			public int Index { get; set; }

			public bool ContainsId(string id)
			{
				foreach (var panel in Panels) {
					if (panel.ContainsId(id)) {
						return true;
					}
				}
				return false;
			}
		}

		[YuzuRequired]
		public List<ToolbarRow> Rows { get; } = new List<ToolbarRow>();

		public void RefreshAfterLoad()
		{
			for (int i = 0; i < Rows.Count; ++i) {
				var row = Rows[i];
				row.Parent = this;
				row.Index = i;
				for (int j = 0; j < row.Panels.Count; ++j) {
					var panel = row.Panels[j];
					panel.Parent = row;
					panel.Index = j;
				}
			}
		}

		public void InsertPanel(ToolbarRow row, ToolbarPanel panel, int index)
		{
			panel.Parent = row;
			panel.Index = index;
			for (int i = index; i < row.Panels.Count; ++i) {
				row.Panels[i].Index += 1;
			}
			row.Panels.Insert(index, panel);
		}

		public void RemovePanel(ToolbarPanel panel)
		{
			var row = panel.Parent;
			row.Panels.RemoveAt(panel.Index);
			for (int i = panel.Index; i < row.Panels.Count; ++i) {
				row.Panels[i].Index -= 1;
			}
		}

		public void SwapPanels(ToolbarPanel panel1, ToolbarPanel panel2)
		{
			panel1.Parent.Panels[panel1.Index] = panel2;
			panel2.Parent.Panels[panel2.Index] = panel1;
			var index = panel1.Index;
			panel1.Index = panel2.Index;
			panel2.Index = index;
			var row = panel1.Parent;
			panel1.Parent = panel2.Parent;
			panel2.Parent = row;
		}

		public void InsertRow(ToolbarRow row, int index)
		{
			row.Parent = this;
			row.Index = index;
			for (int i = index; i < Rows.Count; ++i) {
				Rows[i].Index += 1;
			}
			Rows.Insert(index, row);
		}

		public void RemoveRow(ToolbarRow row)
		{
			if (row.Index > 0) {
				var newRow = Rows[row.Index - 1];
				foreach (var panel in row.Panels) {
					InsertPanel(newRow, panel, newRow.Panels.Count);
				}
			}
			Rows.RemoveAt(row.Index);
			for (int i = row.Index; i < Rows.Count; ++i) {
				Rows[i].Index -= 1;
			}
		}

		public bool ContainsId(string id)
		{
			foreach (var row in Rows) {
				if (row.ContainsId(id)) {
					return true;
				}
			}
			return false;
		}
	}
}

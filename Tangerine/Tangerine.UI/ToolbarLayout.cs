using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzu;
using Lime;

namespace Tangerine.UI
{
	public static class ToolbarCommandRegister
	{
		public static List<ICommand> RegisteredCommands { get; } = new List<ICommand>();

		public static void RegisterCommand(ICommand command)
		{
			RegisteredCommands.Add(command);
		}

		public static void RegisterCommands(params ICommand[] commands)
		{
			RegisteredCommands.AddRange(commands);
		}
	}

	public class ToolbarLayout
	{
		public class ToolbarRow
		{
			[YuzuRequired]
			public List<int> CommandIndexes { get; set; } = new List<int>();

			[YuzuOptional]
			public string Title { get; set; } = "Row";

			[YuzuRequired]
			public int Index { get; set; }

			public readonly bool Editable = true;

			private readonly Widget toolbarContainer;
			public readonly Toolbar Toolbar;

			public ToolbarRow()
			{
				toolbarContainer = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new HBoxLayout { Spacing = 4 },
					LayoutCell = new LayoutCell { StretchY = 0 },
				};
				Toolbar = new Toolbar(toolbarContainer);
			}

			public ToolbarRow(bool editable) : this()
			{
				Editable = editable;
			}

			public void Rebuild(Widget widget)
			{
				widget.Nodes.Add(toolbarContainer);
				if (CommandIndexes.Count == 0) {
					return;
				}
				Toolbar.Clear();
				int count = ToolbarCommandRegister.RegisteredCommands.Count;
				foreach (var index in CommandIndexes) {
					if (index >= count) {
						continue;
					}
					Toolbar.Add(ToolbarCommandRegister.RegisteredCommands[index]);
				}
				Toolbar.Rebuild();
			}

			public static ToolbarRow FromCommands(params ICommand[] commands)
			{
				var toolbarPanel = new ToolbarRow();
				foreach (var command in commands) {
					toolbarPanel.CommandIndexes.Add(ToolbarCommandRegister.RegisteredCommands.IndexOf(command));
				}
				return toolbarPanel;
			}
		}

		[YuzuRequired]
		public List<ToolbarRow> Rows { get; set; } = new List<ToolbarRow>();

		public readonly ToolbarRow CreateToolbarRow;

		[YuzuRequired]
		public int CreateRowIndex {
			get => CreateToolbarRow.Index;
			set => CreateToolbarRow.Index = value;
		}

		public Toolbar CreateToolbar { get => CreateToolbarRow.Toolbar; }

		public ToolbarLayout()
		{
			CreateToolbarRow = new ToolbarRow(false) {
				Index = 0,
				Title = "Create tools row",
			};
		}

		public void Rebuild(Widget widget)
		{
			widget.Nodes.Clear();
			foreach (var row in GetAllRows()) {
				row.Rebuild(widget);
			}
		}

		public IEnumerable<ToolbarRow> GetAllRows()
		{
			for (int i = 0; i < CreateRowIndex; ++i) {
				yield return Rows[i];
			}
			yield return CreateToolbarRow;
			for (int i = CreateRowIndex; i < Rows.Count; ++i) {
				yield return Rows[i];
			}
		}

		public static ToolbarLayout DefaultToolbarLayout()
		{
			return new ToolbarLayout {
				Rows = {
					new ToolbarRow {
						Index = 1,
						Title = "Row1",
						CommandIndexes = new List<int>(Enumerable.Range(0, 10))
					},
					new ToolbarRow {
						Index = 2,
						Title = "Row2",
						CommandIndexes = new List<int>(Enumerable.Range(10, 10))
					}
				}
			};
		}

		public ToolbarRow GetRow(int index)
		{
			if (index == CreateRowIndex) {
				return CreateToolbarRow;
			}
			return Rows[index > CreateRowIndex ? index - 1 : index];
		}

		public void SortRows()
		{
			Rows.Sort((row1, row2) => {
				int i1 = row1.Index;
				int i2 = row2.Index;
				if (i1 == i2) {
					return 0;
				}
				if (i1 < i2) {
					return -1;
				}
				return 1;
			});
		}

		public bool ContainsIndex(int index)
		{
			foreach (var row in Rows) {
				if (row.CommandIndexes.Contains(index)) {
					return true;
				}
			}
			return false;
		}
	}
}

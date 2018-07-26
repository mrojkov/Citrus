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
		public class ToolbarPanel
		{
			[YuzuRequired]
			public List<int> CommandIndexes { get; set; } = new List<int>();

			private readonly Widget toolbarContainer;
			private readonly Toolbar toolbar;

			public ToolbarPanel()
			{
				toolbarContainer = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new HBoxLayout { Spacing = 4 },
					LayoutCell = new LayoutCell { StretchY = 0 },
				};
				toolbar = new Toolbar(toolbarContainer);
			}

			public void Rebuild(Widget widget)
			{
				toolbar.Clear();
				int count = ToolbarCommandRegister.RegisteredCommands.Count;
				foreach (var index in CommandIndexes) {
					if (index >= count) {
						continue;
					}
					toolbar.Add(ToolbarCommandRegister.RegisteredCommands[index]);
				}
				toolbar.Rebuild();
				widget.Nodes.Add(toolbarContainer);
			}

			public static ToolbarPanel FromCommands(params ICommand[] commands)
			{
				var toolbarPanel = new ToolbarPanel {
					CommandIndexes = new List<int>()
				};
				foreach (var command in commands) {
					toolbarPanel.CommandIndexes.Add(ToolbarCommandRegister.RegisteredCommands.IndexOf(command));
				}
				return toolbarPanel;
			}
		}

		public class ToolbarRow
		{
			[YuzuRequired]
			public List<ToolbarPanel> Panels { get; set; } = new List<ToolbarPanel>();

			[YuzuOptional]
			public string Title { get; set; } = "Row";

			private readonly Widget panelsContainer;

			public ToolbarRow()
			{
				panelsContainer = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new HBoxLayout { Spacing = 4 },
					LayoutCell = new LayoutCell { StretchY = 0 },
				};
			}

			public void Rebuild(Widget widget)
			{
				panelsContainer.Nodes.Clear();
				foreach (var panel in Panels) {
					panel.Rebuild(panelsContainer);
				}
				widget.Nodes.Add(panelsContainer);
			}
		}

		[YuzuRequired]
		public List<ToolbarRow> Rows { get; set; } = new List<ToolbarRow>();

		[YuzuRequired]
		public int CreateRowIndex { get; set; } = 0;

		private readonly Widget createToolbarWidget;
		public readonly Toolbar CreateToolbar;

		public ToolbarLayout()
		{
			createToolbarWidget = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new HBoxLayout { Spacing = 4 },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			CreateToolbar = new Toolbar(createToolbarWidget);
		}

		public void Rebuild(Widget widget)
		{
			widget.Nodes.Clear();
			foreach (var row in Rows) {
				row.Rebuild(widget);
			}
			widget.Nodes.Insert(CreateRowIndex, createToolbarWidget);
		}

		public static ToolbarLayout DefaultToolbarLayout()
		{
			return new ToolbarLayout {
				Rows = new List<ToolbarRow> {
					new ToolbarRow {
						Title = "Row1",
						Panels = new List<ToolbarPanel> {
							new ToolbarPanel() {
								CommandIndexes = new List<int>(Enumerable.Range(0, 25))
							}
						}
					}
				}
			};
		}
	}
}

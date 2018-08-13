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

			[YuzuOptional]
			public string Title { get; set; } = "Panel";

			[YuzuRequired]
			public int Index { get; set; }

			[YuzuRequired]
			public bool IsSeparator { get; set; } = false;

			public readonly bool Editable = true;

			private readonly Widget toolbarContainer;
			public readonly Toolbar Toolbar;

			public ToolbarPanel()
			{
				toolbarContainer = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new HBoxLayout { Spacing = 4 },
					LayoutCell = new LayoutCell { StretchY = 0 },
				};
				Toolbar = new Toolbar(toolbarContainer);
			}

			public ToolbarPanel(bool editable) : this()
			{
				Editable = editable;
			}

			public void Rebuild(Widget widget)
			{
				toolbarContainer.Unlink();
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

			public static ToolbarPanel FromCommands(params ICommand[] commands)
			{
				var toolbarPanel = new ToolbarPanel();
				foreach (var command in commands) {
					toolbarPanel.CommandIndexes.Add(ToolbarCommandRegister.RegisteredCommands.IndexOf(command));
				}
				return toolbarPanel;
			}
		}

		[YuzuRequired]
		public List<ToolbarPanel> Panels { get; set; } = new List<ToolbarPanel>();

		public readonly ToolbarPanel CreateToolbarPanel;

		[YuzuRequired]
		public int CreatePanelIndex {
			get => CreateToolbarPanel.Index;
			set => CreateToolbarPanel.Index = value;
		}

		public Toolbar CreateToolbar { get => CreateToolbarPanel.Toolbar; }

		public ToolbarLayout()
		{
			CreateToolbarPanel = new ToolbarPanel(false) {
				Index = 1,
				Title = "Create tools panel",
			};
		}

		public void Rebuild(Widget widget)
		{
			widget.Nodes.Clear();
			var rowWidget = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			widget.AddNode(rowWidget);
			foreach (var row in GetAllPanels()) {
				if (row.IsSeparator) {
					rowWidget = new Frame {
						ClipChildren = ClipMethod.ScissorTest,
						Layout = new HBoxLayout(),
						LayoutCell = new LayoutCell { StretchY = 0 },
					};
					widget.AddNode(rowWidget);
					continue;
				}
				row.Rebuild(rowWidget);
			}
		}

		public IEnumerable<ToolbarPanel> GetAllPanels()
		{
			for (int i = 0; i < CreatePanelIndex; ++i) {
				yield return Panels[i];
			}
			yield return CreateToolbarPanel;
			for (int i = CreatePanelIndex; i < Panels.Count; ++i) {
				yield return Panels[i];
			}
		}

		public static ToolbarLayout DefaultToolbarLayout()
		{
			return new ToolbarLayout {
				Panels = {
					new ToolbarPanel {
						Index = 0,
						Title = "Panel1",
						CommandIndexes = new List<int>(Enumerable.Range(0, 3))
					},
					new ToolbarPanel {
						Index = 2,
						Title = "Panel2",
						CommandIndexes = new List<int>(Enumerable.Range(3, 22))
					},
				}
			};
		}

		public ToolbarPanel GetPanel(int index)
		{
			if (index == CreatePanelIndex) {
				return CreateToolbarPanel;
			}
			return Panels[index > CreatePanelIndex ? index - 1 : index];
		}

		public void SortPanels()
		{
			Panels.Sort((panel1, panel2) => {
				int i1 = panel1.Index;
				int i2 = panel2.Index;
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
			foreach (var panel in Panels) {
				if (panel.CommandIndexes.Contains(index)) {
					return true;
				}
			}
			return false;
		}
	}
}

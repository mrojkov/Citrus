using Lime;
using System.Collections.Generic;
using System.Linq;
using Tangerine.UI.Docking;
using Yuzu;

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

			public ToolbarLayout ParentLayout { get; set; }
			public Toolbar Toolbar { get; private set; }
			public readonly bool Editable = true;

			private readonly Widget toolbarContainer;
			private Image drag;

			public ToolbarPanel()
			{
				toolbarContainer = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchY = 0 },
				};
				Application.InvokeOnNextUpdate(() => {
					drag = new Image {
						Texture = IconPool.GetTexture("Tools.ToolbarSeparator"),
						LayoutCell = new LayoutCell(Alignment.Center),
						MinMaxSize = new Vector2(16),
						Color = IsSeparator ? Color4.Transparent : Color4.White,
						HitTestTarget = true
					};
					if (!IsSeparator) {
						drag.Tasks.Add(DragTask);
					}
					toolbarContainer.Nodes.Insert(0, drag);
				});
				Toolbar = new Toolbar(toolbarContainer);
			}

			private IEnumerator<object> DragTask()
			{
				var input = drag.Input;
				while (true) {
					if (!input.WasMousePressed()) {
						yield return null;
						continue;
					}
					input.ConsumeKey(Key.Mouse0);
					while (input.IsMousePressed()) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						foreach (var panel in ParentLayout.GetAllPanels(appendSeparator: true)) {
							var container = panel.toolbarContainer;
							var pos = container.LocalMousePosition();
							if (panel == this || pos.Y <= 0 || pos.Y >= container.Height || pos.X < 0 || pos.X > container.Width) {
								continue;
							}
							bool isDifferentRow = container.Parent != toolbarContainer.Parent;
							bool isBefore = !panel.IsSeparator && panel.Index < Index && pos.X <= container.Width / 2;
							bool isAfter = !panel.IsSeparator && panel.Index > Index && pos.X >= container.Width / 2;
							if (isDifferentRow || isBefore || isAfter) {
								ParentLayout.MovePanel(this, panel.Index);
								ParentLayout.Rebuild(DockManager.Instance.ToolbarArea);
								goto Next;
							}
						}
						Next:
						yield return null;
					}
				}
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

			private class SeparatorWidget : Widget
			{
				public SeparatorWidget()
				{
					MinMaxWidth = 10;
				}

				public override void Render()
				{
					base.Render();
					PrepareRendererState();
					Renderer.DrawLine(Width / 2, 5, Width / 2, Height - 5, Color4.Gray);
				}
			}
		}

		[YuzuRequired]
		public List<ToolbarPanel> Panels { get; set; } = new List<ToolbarPanel>();

		[YuzuRequired]
		public int CreatePanelIndex {
			get => CreateToolbarPanel.Index;
			set => CreateToolbarPanel.Index = value;
		}

		public Toolbar CreateToolbar { get => CreateToolbarPanel.Toolbar; }
		public readonly ToolbarPanel CreateToolbarPanel;
		private readonly ToolbarPanel separator = new ToolbarPanel { IsSeparator = true };

		public ToolbarLayout()
		{
			CreateToolbarPanel = new ToolbarPanel(false) {
				Index = 1,
				Title = "Create tools panel",
				ParentLayout = this
			};
		}

		private static Frame CreateRowWidget()
		{
			return new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
		}

		public void Rebuild(Widget widget)
		{
			widget.Nodes.Clear();
			var rowWidget = CreateRowWidget();
			widget.AddNode(rowWidget);
			foreach (var panel in GetAllPanels(appendSeparator: true)) {
				panel.ParentLayout = this;
				panel.Rebuild(rowWidget);
				if (panel.IsSeparator) {
					rowWidget = CreateRowWidget();
					widget.AddNode(rowWidget);
				}
			}
		}

		public IEnumerable<ToolbarPanel> GetAllPanels(bool appendSeparator = false)
		{
			for (int i = 0; i < CreatePanelIndex; ++i) {
				yield return Panels[i];
			}
			yield return CreateToolbarPanel;
			for (int i = CreatePanelIndex; i < Panels.Count; ++i) {
				yield return Panels[i];
			}
			if (appendSeparator && !Panels.Last().IsSeparator) {
				separator.Index = Panels.Count;
				yield return separator;
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
						CommandIndexes = new List<int>(Enumerable.Range(3, 23))
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

		public void MovePanel(ToolbarPanel panel, int index)
		{
			RemovePanel(panel);
			InsertPanel(panel, index);
		}

		public void RemovePanel(ToolbarPanel panel)
		{
			if (panel.Index < CreatePanelIndex) {
				CreateToolbarPanel.Index -= 1;
			}
			if (panel.Index > CreatePanelIndex) {
				panel.Index -= 1;
			}
			if (panel != CreateToolbarPanel) {
				Panels.RemoveAt(panel.Index);
			}
			for (int i = panel.Index; i < Panels.Count; ++i) {
				Panels[i].Index -= 1;
			}
		}

		public void InsertPanel(ToolbarPanel panel, int index)
		{
			panel.Index = index;
			if (panel != CreateToolbarPanel) {
				if (index > CreatePanelIndex) {
					index -= 1;
				} else {
					CreatePanelIndex += 1;
				}
			}
			for (int i = index; i < Panels.Count; ++i) {
				Panels[i].Index += 1;
			}
			if (panel != CreateToolbarPanel) {
				Panels.Insert(index, panel);
			}
		}
	}
}

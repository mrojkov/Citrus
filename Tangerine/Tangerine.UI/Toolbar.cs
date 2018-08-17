using Lime;
using System.Collections.Generic;
using Tangerine.UI;

namespace Tangerine
{
	public class Toolbar
	{
		private readonly Widget widget;
		private ToolbarLayout toolbarLayout;

		public Toolbar(Widget container, ToolbarLayout toolbarLayout)
		{
			this.toolbarLayout = toolbarLayout;
			widget = new Widget {
				Layout = new VBoxLayout(),
				LayoutCell = new LayoutCell { StretchY = 0 }
			};
			container.Nodes.Add(widget);
			DecorateToolbar(widget);
		}

		public void Rebuild(ToolbarLayout toolbarLayout)
		{
			this.toolbarLayout = toolbarLayout;
			Rebuild();
		}

		public void Rebuild()
		{
			widget.Nodes.Clear();
			for (int i = 0; i < toolbarLayout.Rows.Count; ++i) {
				var row = toolbarLayout.Rows[i];
				var rowWidget = new Widget {
					MinMaxHeight = Metrics.ToolbarHeight,
					LayoutCell = new LayoutCell { StretchY = 0 },
					Layout = new HBoxLayout()
				};
				for (int j = 0; j < row.Panels.Count; ++j) {
					var panel = row.Panels[j];
					var panelWidget = new Widget {
						Layout = new HBoxLayout { Spacing = 1, CellDefaults = new LayoutCell(Alignment.LeftCenter) },
						LayoutCell = new LayoutCell(Alignment.LeftCenter) { StretchY = 0 },
					};
					if (panel.Draggable) {
						int rowIndex = i;
						int panelIndex = j;
						panelWidget.Awoke += n => {
							PanelAwake(n, rowIndex, panelIndex);
						};
					}
					foreach (var id in panel.CommandIds) {
						if (CommandRegistry.TryGetCommandInfo(id, out CommandInfo commandInfo)) {
							var command = commandInfo.Command;
							var button =
								command.Icon != null ?
								new ToolbarButton(command.Icon) :
								new ToolbarButton(commandInfo.Title);
							button.Clicked += () => CommandQueue.Instance.Add((Command)command);
							button.Updating += _ => {
								button.Texture = command.Icon;
								button.Selected = command.Checked;
								button.Enabled = command.Enabled;
								button.Tip = button.Text = command.Text;
							};
							panelWidget.AddNode(button);
						}
					}
					rowWidget.AddNode(panelWidget);
				}
				rowWidget.AddNode(new Widget());
				widget.AddNode(rowWidget);
			}
		}

		private void PanelAwake(Node node, int rowIndex, int panelIndex)
		{
			var drag = new Image {
				Texture = IconPool.GetTexture("Tools.ToolbarSeparator"),
				LayoutCell = new LayoutCell(Alignment.Center),
				MinMaxSize = new Vector2(16),
				HitTestTarget = true
			};
			node.Tasks.Add(() => DragTask(node.AsWidget, drag.Input, rowIndex, panelIndex));
			node.Nodes.Insert(0, drag);
		}

		private IEnumerator<object> DragTask(Widget panelContainer, WidgetInput input, int rowIndex, int panelIndex)
		{
			int newRowIndex = rowIndex;
			int newPanelIndex = panelIndex;
			var currentPanel = panelContainer;
			while (true) {
				if (!input.WasMousePressed()) {
					yield return null;
					continue;
				}
				widget.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(w => {
					w.PrepareRendererState();
					var pos = currentPanel.CalcPositionInSpaceOf(w);
					Renderer.DrawRect(pos, pos + panelContainer.Size, Color4.Blue.Transparentify(0.7f));
					Renderer.DrawRectOutline(pos, pos + panelContainer.Size, Color4.Blue);
				}));
				while (input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					for (int i = 0; i < widget.Nodes.Count; ++i) {
						var rowWidget = (Widget)widget.Nodes[i];
						if (!IsMouseOver(rowWidget, out Vector2 pos)) {
							continue;
						}
						for (int j = 0; j < rowWidget.Nodes.Count; ++j) {
							var panelWidget = (Widget)rowWidget.Nodes[j];
							if (!IsMouseOver(panelWidget, out pos)) {
								continue;
							}
							if ((newRowIndex != i || newPanelIndex != j) && (rowIndex != i || j != rowWidget.Nodes.Count - 1)) {
								currentPanel = panelWidget;
								newRowIndex = i;
								newPanelIndex = j;
							}
							goto Next;
						}
					}
					Next:
					yield return null;
				}
				widget.CompoundPostPresenter.RemoveAt(widget.CompoundPostPresenter.Count - 1);
				if (newRowIndex == rowIndex && newPanelIndex == panelIndex) {
					continue;
				}
				var row = toolbarLayout.Rows[newRowIndex];
				var panel = toolbarLayout.Rows[rowIndex].Panels[panelIndex];
				if (newRowIndex != rowIndex) {
					toolbarLayout.RemovePanel(panel);
					toolbarLayout.InsertPanel(row, panel, newPanelIndex);
				} else {
					toolbarLayout.SwapPanels(row.Panels[newPanelIndex], panel);
				}
				rowIndex = newRowIndex;
				panelIndex = newPanelIndex;
				toolbarLayout.RefreshAfterLoad();
				Rebuild();
			}
		}

		private static bool IsMouseOver(Widget widget, out Vector2 pos)
		{
			pos = widget.LocalMousePosition();
			return pos.Y >= 0 && pos.Y <= widget.Height && pos.X >= 0 && pos.X <= widget.Width;
		}

		static void DecorateToolbar(Widget widget)
		{
			widget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				if (w.Width > 0) {
					w.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Toolbar.Background);
				}
			}));
		}
	}
}

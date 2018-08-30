using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public class ToolbarView
	{
		private readonly Widget widget;
		private ToolbarModel toolbarModel;

		public ToolbarView(Widget container, ToolbarModel toolbarModel)
		{
			this.toolbarModel = toolbarModel;
			widget = new Widget {
				Layout = new VBoxLayout(),
				LayoutCell = new LayoutCell { StretchY = 0 }
			};
			container.Nodes.Add(widget);
			DecorateToolbar(widget);
		}

		public void Rebuild(ToolbarModel newToolbarModel)
		{
			toolbarModel = newToolbarModel;
			Rebuild();
		}

		public void Rebuild()
		{
			widget.Nodes.Clear();
			for (var i = 0; i < toolbarModel.Rows.Count; ++i) {
				var row = toolbarModel.Rows[i];
				var rowWidget = new Widget {
					MinMaxHeight = Metrics.ToolbarHeight,
					LayoutCell = new LayoutCell { StretchY = 0 },
					Layout = new HBoxLayout()
				};
				for (var j = 0; j < row.Panels.Count; ++j) {
					var panel = row.Panels[j];
					var panelWidget = new Widget {
						Layout = new HBoxLayout { Spacing = 1, DefaultCell = new DefaultLayoutCell(Alignment.LeftCenter) },
						LayoutCell = new LayoutCell(Alignment.LeftCenter) { StretchY = 0 },
					};
					if (panel.Draggable) {
						var rowIndex = i;
						var panelIndex = j;
						panelWidget.Awoke += n => {
							PanelAwake(n, rowIndex, panelIndex);
						};
					}
					foreach (var id in panel.CommandIds) {
						if (!CommandRegistry.TryGetCommandInfo(id, out var commandInfo)) {
							continue;
						}
						var command = commandInfo.Command;
						var button =
							command.Icon != null ?
								new ToolbarButton(command.Icon) :
								new ToolbarButton(commandInfo.Title);
						button.Clicked += () => CommandQueue.Instance.Add((Command)command);
						button.Updating += _ => {
							button.Texture = command.Icon;
							button.Checked = command.Checked;
							button.Enabled = command.Enabled;
							button.Tip = button.Text = command.Text;
						};
						panelWidget.AddNode(button);
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
			var rootWidget = widget.GetRoot().AsWidget;
			var newRowIndex = rowIndex;
			var newPanelIndex = panelIndex;
			var presenter = new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (panelContainer == null) {
					return;
				}
				var aabb = panelContainer.CalcAABBInSpaceOf(rootWidget);
				Renderer.DrawRect(aabb.A, aabb.B, ColorTheme.Current.Toolbar.PanelPlacementHighlightBackground);
				Renderer.DrawRectOutline(aabb.A, aabb.B, ColorTheme.Current.Toolbar.PanelPlacementHighlightBorder);
			});
			while (true) {
				if (!input.WasMousePressed() || Core.Project.Current == Core.Project.Null) {
					yield return null;
					continue;
				}
				rootWidget.CompoundPostPresenter.Add(presenter);
				while (input.IsMousePressed()) {
					Vector2 pos;
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					for (var i = 0; i < widget.Nodes.Count; ++i) {
						var rowWidget = (Widget)widget.Nodes[i];
						if (!IsMouseOver(rowWidget, out pos)) {
							continue;
						}
						for (var j = 0; j < rowWidget.Nodes.Count; ++j) {
							var panelWidget = (Widget)rowWidget.Nodes[j];
							if (!IsMouseOver(panelWidget, out pos)) {
								continue;
							}
							if ((newRowIndex != i || newPanelIndex != j) && (rowIndex != i || j != rowWidget.Nodes.Count - 1)) {
								panelContainer = panelWidget;
								newRowIndex = i;
								newPanelIndex = j;
							}
							goto Next;
						}
					}

					panelContainer = null;
					if (!IsMouseOver(widget, out pos)) {
						newPanelIndex = 0;
						if (pos.Y < 0) {
							newRowIndex = -1;
						}
						if (pos.Y > widget.Height) {
							newRowIndex = toolbarModel.Rows.Count;
						}
					}
					Next:
					yield return null;
				}
				rootWidget.CompoundPostPresenter.Remove(presenter);
				if (newRowIndex == rowIndex && newPanelIndex == panelIndex) {
					continue;
				}
				ToolbarModel.ToolbarRow row;
				if (newRowIndex < 0 || newRowIndex == toolbarModel.Rows.Count) {
					row = new ToolbarModel.ToolbarRow();
					toolbarModel.InsertRow(row, newRowIndex < 0 ? 0 : newRowIndex);
					if (newRowIndex < 0) {
						++rowIndex;
					}
				} else {
					row = toolbarModel.Rows[newRowIndex];
				}
				var oldRow = toolbarModel.Rows[rowIndex];
				var panel = oldRow.Panels[panelIndex];
				if (newRowIndex != rowIndex) {
					toolbarModel.RemovePanel(panel);
					toolbarModel.InsertPanel(row, panel, newPanelIndex);
				} else {
					toolbarModel.SwapPanels(row.Panels[newPanelIndex], panel);
				}
				if (oldRow.Panels.Count == 0) {
					toolbarModel.RemoveRow(oldRow);
				}
				toolbarModel.RefreshAfterLoad();
				Rebuild();
				yield break;
			}
		}

		private static bool IsMouseOver(Widget widget, out Vector2 pos)
		{
			pos = widget.LocalMousePosition();
			return pos.Y >= 0 && pos.Y <= widget.Height && pos.X >= 0 && pos.X <= widget.Width;
		}

		private static void DecorateToolbar(Widget widget)
		{
			widget.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				if (!(w.Width > 0)) {
					return;
				}
				w.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, w.Size, ColorTheme.Current.Toolbar.Background);
			}));
		}
	}
}

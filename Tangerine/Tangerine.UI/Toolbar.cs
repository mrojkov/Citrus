using Lime;
using System.Collections.Generic;
using Tangerine.UI;

namespace Tangerine
{
	public class Toolbar
	{
		private readonly Widget widget;
		private ToolbarLayout toolbarLayout;

		public Toolbar(Widget container)
		{
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
			foreach (var row in toolbarLayout.Rows) {
				var rowWidget = new Widget {
					MinMaxHeight = Metrics.ToolbarHeight,
					LayoutCell = new LayoutCell { StretchY = 0 },
					Layout = new HBoxLayout()
				};
				foreach (var panel in row.Panels) {
					var panelWidget = new Widget {
						Layout = new HBoxLayout { Spacing = 1, CellDefaults = new LayoutCell(Alignment.LeftCenter) },
						LayoutCell = new LayoutCell { StretchY = 0 },
					};
					panelWidget.Awoke += PanelAwake;
					foreach (var id in panel.CommandIds) {
						if (CommandRegister.TryGetCommand(id, out ICommand command)) {
							var button = new ToolbarButton(command.Icon ?? new SerializableTexture());
							button.Clicked += () => CommandQueue.Instance.Add((Command)command);
							button.Updating += _ => {
								button.Texture = command.Icon ?? new SerializableTexture();
								button.Selected = command.Checked;
								button.Enabled = command.Enabled;
								button.Tip = command.Text ?? "";
							};
							panelWidget.AddNode(button);
						}
					}
					rowWidget.AddNode(panelWidget);
				}
				widget.AddNode(rowWidget);
			}
		}

		private void PanelAwake(Node node)
		{
			var drag = new Image {
				Texture = IconPool.GetTexture("Tools.ToolbarSeparator"),
				LayoutCell = new LayoutCell(Alignment.Center),
				MinMaxSize = new Vector2(16),
				HitTestTarget = true
			};
			drag.Clicked += () => node.Tasks.Add(DragTask);
			node.Nodes.Insert(0, drag);
		}

		private IEnumerator<object> DragTask()
		{
			yield return null;
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

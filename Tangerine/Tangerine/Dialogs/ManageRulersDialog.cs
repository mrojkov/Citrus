using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lime;
using Tangerine.UI;
using Tangerine.Core;

namespace Tangerine
{
	public class ManageRulersDialog
	{
		private const float RowHeight = 20f;

		public ManageRulersDialog()
		{
			Button cancelButton;
			Button okButton;
			var window = new Window(new WindowOptions {
				Title = "Manage Rulers",
				Style = WindowStyle.Dialog,
				ClientSize = new Vector2(300, 200),
				Visible = false
			});
			var collection = new ObservableCollection<Ruler>(ProjectUserPreferences.Instance.Rulers);
			var lines = new List<RulerLine>();
			ThemedScrollView container;
			WindowWidget rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(container = new ThemedScrollView {
						Padding = new Thickness { Right = 10 },
					}),
					new Widget {
						Padding = new Thickness { Top = 10 },
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			container.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(
				w => {
					if (w.Parent == null) return;
					var listView = (ThemedScrollView)w;
					w.PrepareRendererState();
					var i = (int)(listView.ScrollPosition / RowHeight);
					var start = 0f;
					while (true) {
						float height;
						if (start == 0f) {
							height = RowHeight - listView.ScrollPosition % RowHeight;
						} else {
							height = start + RowHeight <= w.Size.Y ? RowHeight : w.Size.Y - start;
						}
						var color = i % 2 == 0
						? ColorTheme.Current.Inspector.StripeBackground2
						: ColorTheme.Current.Inspector.StripeBackground1;
						Renderer.DrawRect(new Vector2(0, start), new Vector2(w.Size.X, start + height), color);
						start += height;
						i++;
						if (start >= w.Size.Y) {
							break;
						}
					}
				}));
			container.Content.Layout = new VBoxLayout { Spacing = 4 };
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			container.Content.AddNode(list);
			list.Components.Add(new WidgetFactoryComponent<Ruler>(ruler => new RulerRowView(ruler,
				// Delete button clicked
				() => {
					collection.Remove(ruler);
					container.ScrollPosition = container.ScrollPosition > RowHeight ? container.ScrollPosition - RowHeight : 0;
				},
				// Edit button clicked
				() => {
					if (ruler.AnchorToRoot) {
						var size = Document.Current.Container.AsWidget.Size / 2;
						foreach (var line in ruler.Lines) {
							line.Value += (line.RulerOrientation == RulerOrientation.Horizontal ? size.Y : size.X);
						}
					}
					collection.Remove(ruler);
					lines.AddRange(ruler.Lines);
				}), collection));
			okButton.Clicked += () => {
				var temp = ProjectUserPreferences.Instance.Rulers.ToList();
				foreach (var ruler in temp.Except(collection)) {
					ProjectUserPreferences.Instance.Rulers.Remove(ruler);
				}
				ProjectUserPreferences.Instance.ActiveRuler.Lines.AddRange(lines);
				if (!ProjectUserPreferences.Instance.RulerVisible && lines.Count != 0) {
					(SceneViewCommands.ToggleDisplayRuler as Command)?.Issue();
				}
				Core.UserPreferences.Instance.Save();
				Application.InvalidateWindows();
				window.Close();

			};
			cancelButton.Clicked += () => window.Close();
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					UserPreferences.Instance.Load();
				}
			});
			okButton.SetFocus();
			window.ShowModal();
		}

		internal class RulerRowView : Widget
		{
			private ToolbarButton editButton;

			public RulerRowView(Ruler ruler, Action onDelete, Action onEdit)
			{
				ThemedDeleteButton deleteButton;
				ThemedSimpleText label;
				Layout = new HBoxLayout();
				Nodes.Add(label = new ThemedSimpleText {
					Padding = new Thickness { Left = 10 },
				});
				this.AddChangeWatcher(() => ruler.Name, (name) => label.Text = name);
				Nodes.Add(new Widget());
				Nodes.Add(editButton = new ToolbarButton() {
					Anchors = Anchors.Right,
					LayoutCell = new LayoutCell(Alignment.LeftTop),
					Texture = IconPool.GetTexture("Tools.Edit"),
					Highlightable = false,
				});
				Nodes.Add(deleteButton = new ThemedDeleteButton {
					Anchors = Anchors.Right,
					LayoutCell = new LayoutCell(Alignment.LeftTop)
				});
				deleteButton.Clicked = onDelete;
				editButton.Clicked = onEdit;
				MinMaxHeight = RowHeight;
			}
		}
	}
}

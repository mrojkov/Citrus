using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lime;
using Tangerine.UI;
using Tangerine.Core;

namespace Tangerine
{
	public class DeleteRulerDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly Frame Frame;

		public DeleteRulerDialog()
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(300, 150),
				FixedSize = true,
				Title = "Rulers",
				MinimumDecoratedSize = new Vector2(200, 100)
			});
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			var collection = new ObservableCollection<RulerData>(Project.Current.Rulers);
			ThemedScrollView Container;
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(Container = new ThemedScrollView {
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
			Container.Content.Layout = new VBoxLayout { Spacing = 4 };
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			Container.Content.AddNode(list);
			list.Components.Add(new WidgetFactoryComponent<RulerData>((w) => new RulerRowView(w, collection), collection));

			okButton.Clicked += () => {
				window.Close();
				Core.UserPreferences.Instance.Save();
				var temp = Project.Current.Rulers.ToList();
				foreach (var overlay in temp.Except(collection)) {
					Project.Current.RemoveRuler(overlay);
				}
			};
			cancelButton.Clicked += () => {
				window.Close();
				Core.UserPreferences.Instance.Load();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					Core.UserPreferences.Instance.Load();
				}
			});
			okButton.SetFocus();
		}

		internal class RulerRowView : Widget
		{
			private ThemedSimpleText Label;
			private ThemedDeleteButton deleteButton;
			private static IPresenter StripePresenter = new DelegatePresenter<Widget>(
				w => {
					if (w.Parent != null) {
						var i = w.Parent.AsWidget.Nodes.IndexOf(w);
						w.PrepareRendererState();
						Renderer.DrawRect(Vector2.Zero, w.Size,
							i % 2 == 0 ? ColorTheme.Current.Inspector.StripeBackground2 : ColorTheme.Current.Inspector.StripeBackground1);
					}
				});

			public RulerRowView(RulerData overlay, IList<RulerData> overlays) : base()
			{
				Layout = new HBoxLayout();
				Nodes.Add(Label = new ThemedSimpleText {
					Padding = new Thickness { Left = 10 },
				});
				this.AddChangeWatcher(() => overlay.Name, (name) => Label.Text = name);
				Nodes.Add(new Widget());
				Nodes.Add(deleteButton = new ThemedDeleteButton {
					Anchors = Anchors.Right,
					LayoutCell = new LayoutCell(Alignment.LeftTop)
				});
				CompoundPresenter.Add(StripePresenter);
				deleteButton.Clicked = () => overlays.Remove(overlay);
				MinMaxHeight = 20;
			}
		}
	}
}
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class OrangePluginOptionsDialog
	{
		readonly OrangePluginPanel pluginPanel;
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button runButton;
		readonly Button closeButton;
		readonly Frame Frame;

		public OrangePluginOptionsDialog()
		{
			var uiBuidler = ((OrangeInterface)Orange.UserInterface.Instance).PluginUIBuilder;
			pluginPanel = (OrangePluginPanel)uiBuidler.SidePanel;
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(400, 300),
				FixedSize = true,
				Title = pluginPanel.Title,
				MinimumDecoratedSize = new Vector2(400, 300)
			});
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			ThemedScrollView сontainer;
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(сontainer = new ThemedScrollView {
						Padding = new Thickness { Right = 10 },
					}),
					new Widget {
						Padding = new Thickness { Top = 10 },
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(runButton = new ThemedButton { Text = "Build&Run" }),
							(closeButton = new ThemedButton { Text = "Close" })
						}
					}
				}
			};
			сontainer.Content.Layout = new VBoxLayout { Spacing = 4 };

			foreach (var pluginCheckBox in pluginPanel.CheckBoxes) {
				var checkBoxWidget = new PluginCheckBoxWidget(pluginCheckBox);
				сontainer.Content.AddNode(checkBoxWidget);
			}

			runButton.Clicked += () => {
				((Command)OrangeCommands.Run).Issue();
				window.Close();
			};
			closeButton.Clicked += () => {
				window.Close();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
				}
			});
			runButton.SetFocus();
		}

		private class PluginCheckBoxWidget : Widget
		{
			public PluginCheckBoxWidget(OrangePluginPanel.PluginCheckBox pluginCheckBox)
			{
				Layout = new HBoxLayout { Spacing = 8 };
				CheckBox = new ThemedCheckBox();
				Checked = pluginCheckBox.Active;
				CheckBox.Changed += args => {
					pluginCheckBox.Active = Checked;
					pluginCheckBox.Toogle();
				};
				AddNode(CheckBox);
				Label = new ThemedSimpleText(pluginCheckBox.Label) {
					HitTestTarget = true,
					Clicked = CheckBox.Toggle
				};
				AddNode(Label);
			}

			public SimpleText Label { get; }
			public CheckBox CheckBox { get; }

			public bool Checked
			{
				get { return CheckBox.Checked; }
				set { CheckBox.Checked = value; }
			}
		}
	}
}

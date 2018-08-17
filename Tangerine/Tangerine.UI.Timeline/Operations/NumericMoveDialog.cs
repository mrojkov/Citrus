using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class NumericMoveDialog
	{
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Button okButton;
		private readonly Button cancelButton;
		private readonly Widget container;
		public int Shift { get; set; }

		public NumericMoveDialog()
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(250, 70),
				FixedSize = true,
				Title = "Numeric move",
				Visible = false,
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(container = new Widget {
						Layout = new VBoxLayout(),
					}),
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell {
							StretchY = 0
						},
						Padding = new Thickness { Top = 5 },
						Nodes = {
							new Widget { MinMaxHeight = 0 },
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						},
					}
				}
			};
			var editor = new IntPropertyEditor(new PropertyEditorParams(container, this, nameof(Shift), "Shift"));
			cancelButton.Clicked += () => {
				window.Close();
			};
			okButton.Clicked += () => {
				editor.Submit();
				Document.Current.History.DoTransaction(() => {
					Operations.DragKeyframes.Perform(new IntVector2(Shift, 0), removeOriginals: true);
					Operations.ShiftGridSelection.Perform(new IntVector2(Shift, 0));
				});
				window.Close();
			};
			window.ShowModal();
		}
	}
}

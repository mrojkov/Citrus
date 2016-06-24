using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class WidgetKeyHandler
	{
		private Widget widget;
		private Key key;

		public event Action KeyPressed;
		public event Action KeyReleased;
		public event Action KeyRepeated;

		public WidgetKeyHandler(Widget widget, Key key)
		{
			this.widget = widget;
			this.key = key;
			var keys = (widget.FocusOptions = widget.FocusOptions ?? new FocusOptions());
			keys.WantedKeys.Set(key, true);
			widget.Tasks.Add(MainTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				if (KeyPressed != null && widget.Input.WasKeyPressed(key)) {
					KeyPressed.Invoke();
				}
				if (KeyReleased != null && widget.Input.WasKeyReleased(key)) {
					KeyReleased.Invoke();
				}
				if (KeyRepeated != null && widget.Input.WasKeyRepeated(key)) {
					KeyRepeated.Invoke();
				}
				yield return null;
			}
		}
	}

	public class MarkerPropertiesDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly ComboBox actionSelector;
		readonly EditBox markerIdEditor;
		readonly EditBox jumpToEditor;

		public MarkerPropertiesDialog(Marker marker)
		{
			window = new Window(new WindowOptions { ClientSize = new Vector2(300, 150), FixedSize = false, Title = "Marker properties" });
			rootWidget = new DefaultWindowWidget(window, continuousRendering: false) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 8 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColCount = 2,
							RowCount = 6,
							Spacing = 8,
							Cols = { 
								new LayoutCell(Alignment.RightCenter, 0.5f, 0), 
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new SimpleText { Text = "Marker Id" },
							(markerIdEditor = new EditBox { Text = marker.Id }),
							new SimpleText { Text = "Action" },
							(actionSelector = new ComboBox {
								Items = {
									new ComboBox.Item("Play", MarkerAction.Play),
									new ComboBox.Item("Jump", MarkerAction.Jump),
									new ComboBox.Item("Stop", MarkerAction.Stop),
									new ComboBox.Item("Destroy", MarkerAction.Destroy),
								},
								Value = marker.Action
							}),
							new SimpleText { Text = "Jump to" },
							(jumpToEditor = new EditBox { Text = marker.JumpTo }),
						}
					},
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new Button { Text = "Ok" }),
							(cancelButton = new Button { Text = "Cancel" }),
						}
					}
				}
			};
			new KeyboardFocusSwitcher(rootWidget);
			new WidgetKeyHandler(rootWidget, KeyBindings.CloseDialog).KeyPressed += window.Close;
			okButton.Clicked += () => { Apply(); window.Close(); };
			cancelButton.Clicked += window.Close;
			KeyboardFocus.Instance.SetFocus(okButton);
		}

		private void Apply()
		{
		}
	}
}
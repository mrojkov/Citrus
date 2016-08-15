using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class MarkerPropertiesDialog
	{
		bool result;
		readonly Marker marker;
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly DropDownList actionSelector;
		readonly EditBox markerIdEditor;
		readonly EditBox jumpToEditor;

		public MarkerPropertiesDialog(Marker marker)
		{
			this.marker = marker.Clone();
			window = new Window(new WindowOptions { FixedSize = true, Title = "Marker properties", Visible = false, Style = WindowStyle.Dialog });
			rootWidget = new InvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColCount = 2,
							RowCount = 3,
							Spacing = 8,
							Cols = { 
								new LayoutCell(Alignment.RightCenter, 0.5f, 0), 
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new SimpleText("Marker Id"),
							(markerIdEditor = new EditBox { Text = marker.Id, MinSize = DesktopTheme.Metrics.DefaultEditBoxSize * new Vector2(2, 1) }),
							new SimpleText("Action"),
							(actionSelector = new DropDownList {
								Items = {
									new DropDownList.Item("Play", MarkerAction.Play),
									new DropDownList.Item("Jump", MarkerAction.Jump),
									new DropDownList.Item("Stop", MarkerAction.Stop),
									new DropDownList.Item("Destroy", MarkerAction.Destroy),
								},
								Value = marker.Action
							}),
							new SimpleText("Jump to"),
							(jumpToEditor = new EditBox { Text = marker.JumpTo }),
						}
					},
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new Button("Ok")),
							(cancelButton = new Button("Cancel")),
						}
					}
				}
			};
			new TabTraverseController(rootWidget);
			new WidgetKeyHandler(rootWidget, KeyBindings.CloseDialog).KeyPressed += window.Close;
			okButton.Clicked += () => {
				result = true;
				this.marker.Id = markerIdEditor.Text;
				this.marker.Action = (MarkerAction)actionSelector.Value;
				this.marker.JumpTo = jumpToEditor.Text;
				window.Close();
			};
			cancelButton.Clicked += window.Close;
			KeyboardFocus.Instance.SetFocus(okButton);
		}

		public Marker Show()
		{
			result = false;
			window.ShowDialog();
			return result ? marker : null;
		}
	}
}
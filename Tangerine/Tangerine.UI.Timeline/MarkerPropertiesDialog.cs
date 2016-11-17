using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class MarkerPropertiesDialog
	{
		public enum Result
		{
			Ok,
			Cancel,
			Delete
		}

		public Result Show(Marker marker, bool canDelete)
		{
			Widget buttonsPanel;
			Button deleteButton;
			Button okButton;
			Button cancelButton;
			DropDownList actionSelector;
			EditBox markerIdEditor;
			EditBox jumpToEditor;
			Result result;
			var window = new Window(new WindowOptions { FixedSize = true, Title = "Marker properties", Visible = false, Style = WindowStyle.Dialog });
			var rootWidget = new InvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColCount = 2,
							RowCount = 3,
							Spacing = 8,
							ColDefaults = { 
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
					(buttonsPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new Button("Ok")),
							(cancelButton = new Button("Cancel")),
						}
					})
				}
			};
			if (canDelete) {
				deleteButton = new Button("Delete");
				buttonsPanel.AddNode(deleteButton);
				deleteButton.Clicked += () => {
					result = Result.Delete;
					window.Close();
				};
			}
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.Input.KeyPressed += (input, key) => {
				if (key == Key.Escape) {
					input.ConsumeKey(key);
					window.Close();
				}
			};
			okButton.Clicked += () => {
				result = Result.Ok;
				window.Close();
			};
			cancelButton.Clicked += window.Close;
			okButton.SetFocus();
			result = Result.Cancel;
			window.ShowModal();
			if (result == Result.Ok) {
				marker.Id = markerIdEditor.Text;
				marker.Action = (MarkerAction)actionSelector.Value;
				marker.JumpTo = jumpToEditor.Text;
			}
			return result;
		}
	}
}
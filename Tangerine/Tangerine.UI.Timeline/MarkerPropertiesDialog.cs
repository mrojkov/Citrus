using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

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
			ThemedDropDownList jumpToSelector;
			Result result;
			var window = new Window(new WindowOptions { FixedSize = true, Title = "Marker properties", Visible = false, Style = WindowStyle.Dialog });
			jumpToSelector = new ThemedDropDownList();
			jumpToSelector.Items.Add(new ThemedDropDownList.Item(string.Empty, null));
			foreach (var m in Document.Current.Container.Markers.Where(m => !string.IsNullOrEmpty(m.Id))) {
				jumpToSelector.Items.Add(new ThemedDropDownList.Item(m.Id, m));
			}
			jumpToSelector.Text = marker.JumpTo;
			var rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColumnCount = 2,
							RowCount = 3,
							Spacing = 8,
							ColumnDefaults = {
								new LayoutCell(Alignment.RightCenter, 0.5f, 0),
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new ThemedSimpleText("Marker Id"),
							(markerIdEditor = new ThemedEditBox { Text = marker.Id, MinSize = Theme.Metrics.DefaultEditBoxSize * new Vector2(2, 1) }),
							new ThemedSimpleText("Action"),
							(actionSelector = new ThemedDropDownList {
								Items = {
									new DropDownList.Item("Play", MarkerAction.Play),
									new DropDownList.Item("Jump", MarkerAction.Jump),
									new DropDownList.Item("Stop", MarkerAction.Stop),
								},
								Value = marker.Action
							}),
							new ThemedSimpleText("Jump to"),
							jumpToSelector,
						}
					},
					(buttonsPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton("Ok")),
							(cancelButton = new ThemedButton("Cancel")),
						}
					})
				}
			};
			if (canDelete) {
				deleteButton = new ThemedButton("Delete");
				buttonsPanel.AddNode(deleteButton);
				deleteButton.Clicked += () => {
					result = Result.Delete;
					window.Close();
				};
			}
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.Add(new KeyPressHandler(Key.Escape,
				(input, key) => {
					input.ConsumeKey(key);
					window.Close();
				}));
			rootWidget.LateTasks.Add(new KeyPressHandler(Key.Enter,
				(input, key) => {
					input.ConsumeKey(key);
					result = Result.Ok;
					window.Close();
				}));
			okButton.Clicked += () => {
				result = Result.Ok;
				window.Close();
			};
			cancelButton.Clicked += window.Close;
			markerIdEditor.SetFocus();
			result = Result.Cancel;
			window.ShowModal();
			if (result == Result.Ok) {
				marker.Id = markerIdEditor.Text;
				marker.Action = (MarkerAction)actionSelector.Value;
				marker.JumpTo = jumpToSelector.Text;
			}
			return result;
		}
	}
}

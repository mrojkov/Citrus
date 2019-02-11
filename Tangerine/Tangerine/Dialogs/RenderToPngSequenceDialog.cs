using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.PropertyEditors;

namespace Tangerine.Dialogs
{
	public class RenderToPngSequenceDialog
	{
		public enum Result
		{
			Ok,
			Cancel
		}

		public class RenderToPngSequenceOptions
		{
			public static string LastOpenedDirectory;

			[TangerineValidRange(1, 1000, WarningLevel = ValidationResult.Error)]
			public int FPS { get; set; }

			private string folder = LastOpenedDirectory ?? Path.GetDirectoryName(Project.Current?.CitprojPath ?? "C:\\");

			public string Folder
			{
				get => folder;
				set => LastOpenedDirectory = folder = value;
			}
		}

		public Result Show(out RenderToPngSequenceOptions options)
		{
			Widget buttonPanel;
			Button okButton;
			Button cancelButton;
			Widget inspectorPanel;
			var result = Result.Cancel;
			options = new RenderToPngSequenceOptions {FPS = 60};
			var window = new Window(new WindowOptions {
				Title = "Options",
				Visible = false,
				Style = WindowStyle.Dialog,
			});
			var rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16f },
				MinSize = new Vector2(256, 0),
				Nodes = {
					(inspectorPanel = new Widget {
						Layout = new VBoxLayout { Spacing = 2f }
					}),
					(buttonPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8f },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton("Ok")),
							(cancelButton = new ThemedButton("Cancel")),
						}
					})
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.Add(new KeyPressHandler(Key.Escape,
				(input, key) => {
					input.ConsumeKey(key);
					window.Close();
				}
			));
			new IntPropertyEditor(new PropertyEditorParams(inspectorPanel, options, "FPS") {
				PropertySetter = (o, name, value) => ((RenderToPngSequenceOptions) o).FPS = (int)value,
				DisplayName = "FPS",
				PropertyInfo = typeof(RenderToPngSequenceOptions).GetProperty(nameof(RenderToPngSequenceOptions.FPS))
			});
			new FolderPropertyEditor(new PropertyEditorParams(inspectorPanel, options, "Folder") {
				PropertySetter = (o, name, value) => ((RenderToPngSequenceOptions)o).Folder = (string)value,
				DisplayName = "Folder",
				PropertyInfo = typeof(RenderToPngSequenceOptions).GetProperty(nameof(RenderToPngSequenceOptions.Folder)),
			}) {
				ShowPrefix = false
			};
			okButton.Clicked += () => {
				result = Result.Ok;
				window.Close();
			};
			cancelButton.Clicked += window.Close;
			window.ShowModal();
			return result;
		}
	}
}

using Lime;
using System.Collections.Generic;

namespace Tangerine.UI.FilesystemView
{
	public class AddressBar : Toolbar
	{
		public enum AddressBarState
		{
			PathBar,
			Editor
		}
		private string buffer;
		private AddressBarState state;
		private PathBar pathBar;
		private ThemedEditBox editor;
		private FilesystemView view;

		public string Path
		{
			get {
				return buffer;
			}
			set {
				buffer = AdjustFolderPath(value);
			}
		}

		public AddressBar(FilesystemView view, Model model)
		{
			this.view = view;
			Layout = new StackLayout();
			buffer = Path = model.CurrentPath;
			state = AddressBarState.PathBar;
			CreatePathBar();
			CreateEditor();

			Updating += (float delta) => {
				if (editor.IsFocused() && state != AddressBarState.Editor) {
					state = AddressBarState.Editor;
					editor.Text = buffer;
					DeletePathBar();
				}
			};
		}

		public static string AdjustFolderPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else if (
						path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar &&
						path[path.Length - 2] != ':'
					) {
				return path.Remove(path.Length - 1);
			} else if (
				path.EndsWith(".scene") ||
				path.EndsWith(".tan") ||
				path.EndsWith(".t3d")
				) {
				var i = path.Length - 1;
				var c = path[path.Length - 1];
				while (c != System.IO.Path.DirectorySeparatorChar) {
					path = path.Remove(i);
					i--;
					c = path[i];
				}
				return path.Remove(i);
			}
			return path;
		}

		private void FlipState()
		{
			if (state == AddressBarState.Editor) {
				state = AddressBarState.PathBar;
				editor.Text = "";
				Nodes.Remove(editor);
				CreatePathBar();
				Nodes.Add(editor);
			} else {
				state = AddressBarState.Editor;
				DeletePathBar();
				editor.Text = buffer;
			}
		}

		private void CreateEditor()
		{
			Nodes.Add(editor = new ThemedEditBox());
			editor.LayoutCell = new LayoutCell(Alignment.LeftCenter);
			editor.Updating += (float delta) => {
				if (editor.Input.WasKeyPressed(Key.Enter)) {
					if (view.Open(editor.Text)) {
						buffer = AdjustFolderPath(editor.Text);
						FlipState();
					} else {
						editor.Text = buffer;
					}
				}
			};
		}

		private void CreatePathBar()
		{
			Nodes.Add(pathBar = new PathBar(view, this));
			pathBar.LayoutCell = new LayoutCell(Alignment.LeftCenter);
			pathBar.Updating += (float delta) => {
				if (pathBar.IsMouseOver() && pathBar.Input.WasMouseReleased(Key.Mouse0)) {
					FlipState();
				}
			};
		}

		private void DeletePathBar()
		{
			Nodes.Remove(pathBar);
			pathBar.Updating -= (float delta) => {
				if (
					pathBar.IsMouseOver() &&
					pathBar.Input.WasMouseReleased(Key.Mouse0)
				) {
					FlipState();
				}
			};
			pathBar = null;
		}
	}

	public class PathBar : Widget
	{
		private string buffer;
		private string[] topFoldersPaths;
		private int countOfFolders;
		private FilesystemView view;
		private PathRootButton rootButton;
		private PathFolderButton[] folderButtons;
		private PathArrowButton[] arrowButtons;

		public PathBar(FilesystemView view, AddressBar addressBar)
		{
			this.view = view;
			buffer = addressBar.Path;
			Layout = new HBoxLayout();
			LayoutCell = new LayoutCell(Alignment.LeftCenter);
			CreateButtons();

			Updating += (float delta) => {
				if (!buffer.Equals(addressBar.Path)) {
					buffer = addressBar.Path;
					UpdatePathBar();
				}
			};
		}

		private void CreateButtons()
		{
			countOfFolders = ToСountOfFolders(buffer);
			topFoldersPaths = FillTopFoldersPaths(buffer, countOfFolders);
			folderButtons = new PathFolderButton[countOfFolders];
			arrowButtons = new PathArrowButton[countOfFolders + 1];

			Nodes.Add(rootButton = new PathRootButton(buffer, view));
			Nodes.Add(arrowButtons[0] = new PathArrowButton(System.IO.Path.GetPathRoot(buffer), view));
			for (var i = 0; i < countOfFolders; i++) {
				Nodes.Add(folderButtons[i] = new PathFolderButton(topFoldersPaths[i], view));
				Nodes.Add(arrowButtons[i + 1] = new PathArrowButton(topFoldersPaths[i], view));
			}
		}

		private void DestroyButtons()
		{
			Nodes.Remove(rootButton);
			Nodes.Remove(arrowButtons[0]);
			for (var i = 0; i < countOfFolders; i++) {
				Nodes.Remove(arrowButtons[i + 1]);
				Nodes.Remove(folderButtons[i]);
			}
		}

		private void UpdatePathBar()
		{
			DestroyButtons();
			CreateButtons();
		}

		public static int ToСountOfFolders(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return -1;
			}
			var folders = 0;
			var i = 0;
			while (i != path.Length) {
				if (
					path[i] == System.IO.Path.DirectorySeparatorChar &&
					i + 1 != path.Length
				) {
					folders++;
				}
				i++;
			}
			return folders;
		}

		public static string[] FillTopFoldersPaths(string path, int countOfFolders)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}
			var topFolders = new string[countOfFolders];
			var i = countOfFolders - 1;
			if (countOfFolders != 0) {
				topFolders[i] = path;
				i--;
				while (i != -1) {
					topFolders[i] = System.IO.Path.GetDirectoryName(topFolders[i + 1]);
					i--;
				}
			}
			return topFolders;
		}
	}

	public class PathFolderButton : ThemedButton
	{
		public PathFolderButton(string path, FilesystemView view) : base()
		{
			Text = GetNameOfFolder(path);
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 3).X;
			Gestures.Add(new ClickGesture(0, () => view.Open(path)));
		}

		public static string GetNameOfFolder(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else {
				int i;
				for (i = path.Length - 1; i >= 0; i--) {
					if (path[i] == System.IO.Path.DirectorySeparatorChar) {
						i++;
						break;
					}
				}
				return path.Substring(i);
			}
		}
	}

	public class PathRootButton : ThemedButton
	{
		public PathRootButton(string path, FilesystemView view) : base()
		{
			Text = GetNameOfRoot(path);
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 4).X;
			Gestures.Add(new ClickGesture(0, () => view.Open(System.IO.Path.GetPathRoot(path))));
		}

		public static string GetNameOfRoot(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else {
				path = System.IO.Path.GetPathRoot(path);
				return path.Remove(path.Length - 1);
			}
		}
	}

	public class PathArrowButton : ThemedButton
	{
		public enum PathArrowButtonState
		{
			Right,
			Down
		}
		private string path;
		private PathArrowButtonState state;
		private DirectoryPicker picker;
		private FilesystemView view;

		public PathArrowButton(string path, FilesystemView view) : base()
		{
			this.path = path;
			this.view = view;

			Gestures.Add(new ClickGesture(0, FlipState));
			Text = ">";
			state = PathArrowButtonState.Right;
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 5).X;
		}

		private void FlipState()
		{
			if (state == PathArrowButtonState.Right) {
				Text = "v";
				state = PathArrowButtonState.Down;
				picker = new DirectoryPicker(GlobalPosition + new Vector2(0, Height), path, view);
				picker.Deactivated += () => {
					picker.Close();
					FlipState();
				};
			} else {
				Text = ">";
				state = PathArrowButtonState.Right;
				picker.Close();
			}
		}
	}

	public class DirectoryPicker : Window
	{
		public WindowWidget rootWidget;
		private Widget widget;
		private bool IsMouseInside = false;
		private List<FilesystemItem> filesystemItems;
		private List<string> internalFolders;
		private FilesystemView view;

		public DirectoryPicker(Vector2 localPosition, string path, FilesystemView view) :
			base(new WindowOptions {
				Style = WindowStyle.Borderless,
				Centered = false
			})
		{
			this.view = view;

			ClientPosition = Window.Current.LocalToDesktop(localPosition);
			
			internalFolders = GetInternalFoldersPaths(path);
			filesystemItems = GetFilesystemItems(internalFolders);

			widget = new Widget {
				Layout = new VBoxLayout()
			};
			for (var i = 0; i < filesystemItems.Count; i++) {
				widget.Nodes.Add(filesystemItems[i]);
			}
			Closed += widget.Nodes.Clear;

			rootWidget = new ThemedInvalidableWindowWidget(this) {
				LayoutBasedWindowSize = true,
				Layout = new VBoxLayout { Spacing = 4 },
				Nodes = { widget }
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
		}

		public static List<string> GetInternalFoldersPaths(string path)
		{
			var model = new Model(path);
			var foldersPaths = new List<string>();
			foreach (var item in model.EnumerateDirectories(path)) {
				foldersPaths.Add(item);
			}
			return foldersPaths;
		}

		private List<FilesystemItem> GetFilesystemItems(List<string> paths)
		{
			var items = new List<FilesystemItem>();
			foreach (var path in paths) {
				FilesystemItem item;
				items.Add(item = new FilesystemItem(path, true));
				item.Updating += (float delta) => {
					if (IsMouseEntering(item)) {
						Invalidate();
					}
					if (item.Input.WasMouseReleased()) {
						Close();
						view.Open(item.FilesystemPath);
					}
				};
			}
			return items;
		}

		private bool IsMouseEntering(Widget widget)
		{
			if (
				widget.LocalMousePosition().X >= 0 &&
				widget.LocalMousePosition().Y >= 0 &&
				widget.LocalMousePosition().X <= widget.Width &&
				widget.LocalMousePosition().Y <= widget.Height
			) {
				if (!IsMouseInside) {
					IsMouseInside = true;
					return true;
				} else {
					return false;
				}
			} else {
				IsMouseInside = false;
				return false;
			}
		}
	}
}

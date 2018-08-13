using Lime;
using System.Collections.Generic;
using System.Linq;

namespace Tangerine.UI.FilesystemView
{
	public class AddressBar : Toolbar
	{
		public enum AddressBarState
		{
			PathBar,
			Editor
		}
		private string buffer = "C:\\";
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
				buffer = AdjustPath(value);
			}
		}

		public AddressBar(FilesystemView view)
		{
			this.view = view;
			Layout = new StackLayout();
			state = AddressBarState.PathBar;
			CreatePathBar();
			CreateEditor();
			Updating += (float delta) => {
				if (
					editor.IsFocused() &&
					state != AddressBarState.Editor
				) {
					state = AddressBarState.Editor;
					editor.Text = buffer;
					DeletePathBar();
				}
				if (
					state == AddressBarState.Editor &&
					!editor.IsFocused()
				) {
					FlipState();
				}
			};
		}
		
		private IEnumerator<object> ShowAlertTask(string message)
		{
			yield return Task.WaitWhile(() => Input.ConsumeKeyPress(Key.Enter));

			var dialog = new AlertDialog(message);
			dialog.Show();
		}

		private string AdjustPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return buffer;
			}
			if (path.Length < 3) {
				Tasks.Add(ShowAlertTask("The size of the path is less than the permissible."));
				return buffer;
			}

			char[] charsToTrim = { '.', ' ' };
			path = path.Trim(charsToTrim);

			if (path.Contains(System.IO.Path.AltDirectorySeparatorChar.ToString())) {
				Tasks.Add(ShowAlertTask("The path is in an invalid format."));
				return buffer;
			}

			//If the user added many slashes
			string doubleDirectorySeparator = string.Empty;
			doubleDirectorySeparator += System.IO.Path.DirectorySeparatorChar;
			doubleDirectorySeparator += System.IO.Path.DirectorySeparatorChar;
			if (path.Contains(doubleDirectorySeparator)) {
				Tasks.Add(ShowAlertTask("The path is in an invalid format."));
				return buffer;
			}

			if (
				path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar &&
				path[path.Length - 2] != System.IO.Path.VolumeSeparatorChar
			) {
				path = path.Remove(path.Length - 1);
			}

			return path;
		}
		
		public static string PathToFolderPath(string path)
		{
			if (System.IO.Path.GetExtension(path) != string.Empty) {
				if (!System.IO.Directory.Exists(path)) {
					var i = path.Length - 1;
					var c = path[path.Length - 1];
					while (c != System.IO.Path.DirectorySeparatorChar) {
						path = path.Remove(i);
						i--;
						c = path[i];
					}
				}
			}
			if (
				path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar &&
				path[path.Length - 2] != System.IO.Path.VolumeSeparatorChar
			) {
				path = path.Remove(path.Length - 1);
			}
			return path;
		}

		public void SetFocusOnEditor()
		{
			if (state != AddressBarState.Editor) {
				FlipState();
				editor.SetFocus();
			}
		}

		private void FlipState()
		{
			if (state == AddressBarState.Editor) {
				state = AddressBarState.PathBar;
				editor.Text = "";
				CreatePathBar();
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
					var adjustedText = AdjustPath(editor.Text);
					if (view.Open(adjustedText)) {
						buffer = PathToFolderPath(adjustedText);
						FlipState();
					} else {
						editor.Text = buffer;
					}
				}
			};
		}

		private void CreatePathBar()
		{
			Nodes.Push(pathBar = new PathBar(view, this));
			pathBar.LayoutCell = new LayoutCell(Alignment.LeftCenter);
			pathBar.Updating += UpdatingPathBar;
		}

		private void UpdatingPathBar(float delta)
		{
			if (pathBar.IsMouseOver() && pathBar.Input.WasMouseReleased(Key.Mouse0)) {
				FlipState();
			}
		}

		private void DeletePathBar()
		{
			Nodes.Remove(pathBar);
			pathBar.Updating -= UpdatingPathBar;
			pathBar = null;
		}
	}

	public class PathBar : Widget
	{
		private string buffer;
		private int countOfFolders;
		private FilesystemView view;
		private PathBarButton[] buttons;
		private PathArrowButton rootArrowButton;

		public PathBar(FilesystemView view, AddressBar addressBar)
		{
			this.view = view;
			buffer = addressBar.Path;
			Layout = new HBoxLayout();
			LayoutCell = new LayoutCell(Alignment.LeftCenter);
			Padding = new Thickness(1);
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
			countOfFolders = GetСountOfFolders(buffer);
			var topFoldersPaths = GetTopFoldersPaths(buffer, countOfFolders);
			buttons = new PathBarButton[countOfFolders];
			
			Nodes.Add(rootArrowButton = new PathArrowButton(view));
			for (var i = 0; i < countOfFolders; i++) {
				Nodes.Add(buttons[i] = new PathBarButton(view, topFoldersPaths[i]));
			}
		}

		private void DestroyButtons()
		{
			for (var i = countOfFolders - 1; i >= 0; i--) {
				Nodes.Remove(buttons[i]);
			}
			Nodes.Remove(rootArrowButton);
		}

		private void UpdatePathBar()
		{
			DestroyButtons();
			CreateButtons();
		}

		public static int GetСountOfFolders(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return -1;
			}
			var folders = 0;
			for (var i = 0; i < path.Length; i++) {
				if (
					path[i] == System.IO.Path.DirectorySeparatorChar ||
					i == path.Length - 1
				) {
					folders++;
				}
			}
			return folders;
		}

		public static string[] GetTopFoldersPaths(string path, int countOfFolders)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}
			if (countOfFolders > 0) {
				var topFolders = new string[countOfFolders];
				var i = countOfFolders - 1;
				topFolders[i] = path;
				i--;
				while (i != -1) {
					topFolders[i] = System.IO.Path.GetDirectoryName(topFolders[i + 1]);
					i--;
				}
				return topFolders;
			} else {
				return null;
			}
		}
	}


	public enum PathBarButtonState
	{
		Normal,
		Hover,
		Press
	}

	public class PathBarButton : Widget
	{
		private PathBarButtonState state;
		private PathFolderButton folderButton;
		private PathArrowButton arrowButton;

		public PathBarButton(FilesystemView view, string path) : base()
		{
			Layout = new HBoxLayout();
			HitTestTarget = true;

			folderButton = new PathFolderButton(view, path);
			arrowButton = new PathArrowButton(view, path);

			Nodes.Add(folderButton);
			Nodes.Add(arrowButton);

			Updating += (float delta) => {
				if (arrowButton.ArrowState == PathArrowButtonState.Expanded) {
					state = PathBarButtonState.Press;
				} else { 
					if (IsMouseOverThisOrDescendant()) {
						if (
							folderButton.WasClicked() ||
							arrowButton.WasClicked()
						) {
							state = PathBarButtonState.Press;
						} else {
							state = PathBarButtonState.Hover;
						}
					} else {
						state = PathBarButtonState.Normal;
					}
				}
				folderButton.SetState(state);
				arrowButton.SetState(state);
			};
		}
	}

	public class PathButtonPresenter : ThemedButton.ButtonPresenter
	{
		private ColorGradient innerGradient;
		private Color4 outline;

		public void SetState(PathBarButtonState state)
		{
			CommonWindow.Current.Invalidate();
			switch (state) {
				case PathBarButtonState.Normal:
					innerGradient = Theme.Colors.PathBarButtonNormal;
					outline = Theme.Colors.PathBarButtonOutlineNormal;
					break;
				case PathBarButtonState.Hover:
					innerGradient = Theme.Colors.PathBarButtonHover;
					outline = Theme.Colors.PathBarButtonOutlineHover;
					break;
				case PathBarButtonState.Press:
					innerGradient = Theme.Colors.PathBarButtonPress;
					outline = Theme.Colors.PathBarButtonOutlinePress;
					break;
			}
		}

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, innerGradient);
			Renderer.DrawRectOutline(Vector2.Zero, widget.Size, outline);
		}
	}

	public class PathFolderButton : ThemedButton
	{
		private new PathButtonPresenter Presenter;
		public PathArrowButton arrowButton;
		public PathBarButtonState State;

		public PathFolderButton(FilesystemView view, string path) : base()
		{
			Text = GetName(path);
			MinMaxHeight = 20;
			Presenter = new PathButtonPresenter();
			base.Presenter = Presenter;
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 3).X;
			Gestures.Add(new ClickGesture(0, () => view.Open(path)));
			Gestures.Add(new ClickGesture(1, () => SystemShellContextMenu.Instance.Show(path)));
		}

		public void SetState(PathBarButtonState state)
		{
			Presenter.SetState(state);
		}

		public static string GetName(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else if(
				path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar &&
				path[path.Length - 2] == System.IO.Path.VolumeSeparatorChar
			) { // Root
				return path.Remove(path.Length - 1);
			} else { // Folder
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

	public enum PathArrowButtonState
	{
		Collapsed,
		Expanded
	}

	public class PathArrowButton : ThemedButton
	{
		private string path;
		private DirectoryPicker picker;
		private FilesystemView view;
		private new PathButtonPresenter Presenter;
		public PathArrowButtonState ArrowState;
		public PathBarButtonState State;
		public PathFolderButton folderButton;

		public PathArrowButton(FilesystemView view, string path = null) : base()
		{
			this.path = path;
			this.view = view;
			MinMaxHeight = 20;
			Presenter = new PathButtonPresenter();
			base.Presenter = Presenter;
			if (path == null) {
				Updating += (float delta) => {
					if (ArrowState == PathArrowButtonState.Expanded) {
						State = PathBarButtonState.Press;
					} else {
						if (IsMouseOverThisOrDescendant()) {
							if (WasClicked()) {
								State = PathBarButtonState.Press;
							} else {
								State = PathBarButtonState.Hover;
							}
						} else {
							State = PathBarButtonState.Normal;
						}
					}
					Presenter.SetState(State);
				};
			}
			Gestures.Add(new ClickGesture(0, FlipState));
			Text = ">";
			ArrowState = PathArrowButtonState.Collapsed;
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 5).X;
		}

		public void SetState(PathBarButtonState state)
		{
			Presenter.SetState(state);
		}

		private void FlipState()
		{
			if (ArrowState == PathArrowButtonState.Collapsed) {
				Text = "v";
				ArrowState = PathArrowButtonState.Expanded;
				var indent = 14;
				var pickerPosition = Window.Current.LocalToDesktop(GlobalPosition + new Vector2(-indent, Height));
				picker = new DirectoryPicker(pickerPosition, view, path);
				picker.Window.Deactivated += () => {
					picker.Window.Close();
					FlipState();
				};
			} else {
				Text = ">";
				ArrowState = PathArrowButtonState.Collapsed;
				picker.Window.Close();
			}
		}
	}

	public class DirectoryPicker
	{
		private FilesystemView view;
		private bool IsMouseInside = false;
		private ThemedScrollView scrollView;

		public Window Window { get; }

		public DirectoryPicker(Vector2 globalPosition, FilesystemView view, string path = null)
		{
			this.view = view;

			List<FilesystemItem> filesystemItems = new List<FilesystemItem>();
			if (path == null) {
				var logicalDrives = System.IO.Directory.GetLogicalDrives();
				var availableRoots = GetAvailableRootsPathsFromLogicalDrives(logicalDrives);
				filesystemItems = GetFilesystemItems(availableRoots);
			} else {
				var internalFolders = GetInternalFoldersPaths(path);
				filesystemItems = GetFilesystemItems(internalFolders);
			}

			const int MaxItemsOnWindow = 19; // Like in Windows File Explorer
			var itemsCount = System.Math.Min(filesystemItems.Count, MaxItemsOnWindow);
			var clientSize = new Vector2(FilesystemItem.ItemWidth, (FilesystemItem.IconSize + 2 * FilesystemItem.ItemPadding) * itemsCount);
			Window = new Window(new WindowOptions {
				ClientSize = clientSize,
				FixedSize = true,
				MinimumDecoratedSize = clientSize,
				Style = WindowStyle.Borderless,
				Centered = false,
				Visible = false
			});

			scrollView = new ThemedScrollView();
			var list = new Widget {
				Layout = new VBoxLayout()
			};
			list.Nodes.AddRange(filesystemItems);
			scrollView.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			scrollView.Content.AddNode(list);
			var rootWidget = new ThemedInvalidableWindowWidget(Window) {
				Layout = new VBoxLayout(),
				Padding = new Thickness(5),
				Nodes = {
					scrollView
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.Presenter = new DelegatePresenter<Widget>(_ => {
				rootWidget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, Window.ClientSize, Theme.Colors.DirectoryPickerBackground);
			});
			rootWidget.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(_ => {
				rootWidget.PrepareRendererState();
				Renderer.DrawRectOutline(Vector2.Zero, Window.ClientSize, Theme.Colors.DirectoryPickerOutline, thickness: 2);
			}));
			Window.ClientSize = clientSize + new Vector2(rootWidget.Padding.Left * 2);

			Window.Visible = true;
			Window.ClientPosition = globalPosition;
		}

		public static List<string> GetInternalFoldersPaths(string path)
		{
			var foldersPaths = new List<string>();
			foreach (var item in System.IO.Directory.EnumerateDirectories(path).OrderBy(f => f)) {
				foldersPaths.Add(item);
			}
			return foldersPaths;
		}

		private List<FilesystemItem> GetFilesystemItems(List<string> paths)
		{
			var items = new List<FilesystemItem>();
			foreach (var path in paths) {
				FilesystemItem item;
				items.Add(item = new FilesystemItem(path));
				item.CompoundPresenter.Add(new DelegatePresenter<Widget>(_ => {
					if (item.IsMouseOverThisOrDescendant()) {
						item.PrepareRendererState();
						Renderer.DrawRect(Vector2.Zero, item.Size, Theme.Colors.HoveredBackground);
					}
				}));
				item.Updating += (float delta) => {
					if (IsMouseEntering(item)) {
						Window.Invalidate();
					}
					if (item.Input.WasMouseReleased(0)) {
						Window.Close();
						view.Open(item.FilesystemPath);
					} else if (item.Input.WasMouseReleased(1)) {
						SystemShellContextMenu.Instance.Show(item.FilesystemPath);
					}
				};
			}
			return items;
		}

		public static List<string> GetAvailableRootsPathsFromLogicalDrives(string[] logicalDrives)
		{
			var countOfRealRoots = 0;
			foreach (var path in logicalDrives) {
				if (System.IO.Directory.Exists(path)) {
					countOfRealRoots++;
				}
			}
			List<string> availableRoots = new List<string>();
			var i = 0;
			foreach (var root in logicalDrives) {
				if (System.IO.Directory.Exists(root)) {
					availableRoots.Add(root);
					i++;
					if (i == countOfRealRoots) break;
				}
			}
			return availableRoots;
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

using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class AddressBar : Toolbar
	{
		public enum AddressBarState
		{
			PathBar,
			Editor
		}
		private Model model;
		private AddressBarState state;
		private PathBar pathBar;
		private ThemedEditBox editor;
		private Func<string, bool> openPath;

		public AddressBar(Func<string, bool> openPath, Model model)
		{
			this.openPath = openPath;
			this.model = model;
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
					editor.Text = model.CurrentPath;
					RemovePathBar();
				}
				if (
					state == AddressBarState.Editor &&
					!editor.IsFocused()
				) {
					FlipState();
				}
			};
		}

		private string AdjustPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return model.CurrentPath;
			}
			if (path.Length < 3) {
				AlertDialog.Show("The size of the path is less than the permissible.");
				return model.CurrentPath;
			}

			if (path.Contains("..")) {
				var amountOfCharacters = 0;
				if (path.Contains("/../")) {
					amountOfCharacters = 4;
				} else if (path.Contains("\\..\\")) {
					amountOfCharacters = 4;
				} else if (path.Contains("/..")) {
					amountOfCharacters = 3;
				} else if (path.Contains("\\..")) {
					amountOfCharacters = 3;
				} else if (path.Contains("..")) {
					amountOfCharacters = 2;
				}

				if (amountOfCharacters != 0) {

					if (new DirectoryInfo(path.Remove(path.Length - amountOfCharacters)).Parent == null) {
						path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
					} else if (amountOfCharacters == 2) {
						// Uri does not work with two dots
						path = Path.GetDirectoryName(path.Remove(path.Length - amountOfCharacters));
					} else {
						path = Path.GetFullPath((new Uri(path)).LocalPath);
					}
				}
			}

			char[] charsToTrim = { '.', ' ' };
			path = path.Trim(charsToTrim);

			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			//If the user added many slashes
			string doubleDirectorySeparator = string.Empty;
			doubleDirectorySeparator += Path.DirectorySeparatorChar;
			doubleDirectorySeparator += Path.DirectorySeparatorChar;
			if (path.Contains(doubleDirectorySeparator)) {
				AlertDialog.Show("The path is in an invalid format.");
				return model.CurrentPath;
			}

			if (
				path[path.Length - 1] == Path.DirectorySeparatorChar &&
				path[path.Length - 2] != Path.VolumeSeparatorChar
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
				RemovePathBar();
				editor.Text = model.CurrentPath;
			}
		}

		private void CreateEditor()
		{
			Nodes.Add(editor = new ThemedEditBox());
			editor.LayoutCell = new LayoutCell(Alignment.LeftCenter);
			editor.Updating += (float delta) => {
				if (editor.Input.WasKeyPressed(Key.Enter)) {
					var adjustedText = AdjustPath(editor.Text);
					if (openPath(adjustedText)) {
						FlipState();
					} else {
						editor.Text = model.CurrentPath;
					}
				}
			};
		}

		private void CreatePathBar()
		{
			Nodes.Push(pathBar = new PathBar(openPath, model));
			pathBar.LayoutCell = new LayoutCell(Alignment.LeftCenter);
		}

		private void RemovePathBar()
		{
			Nodes.Remove(pathBar);
			pathBar = null;
		}
	}

	public class PathBar : Widget
	{
		private List<string> topFoldersPaths;
		private Func<string, bool> openPath;
		private PathBarButton[] buttons;
		private PathArrowButton rootArrowButton;
		private Model model;

		public PathBar(Func<string, bool> openPath, Model model)
		{
			this.model = model;
			this.openPath = openPath;
			Layout = new HBoxLayout();
			LayoutCell = new LayoutCell(Alignment.LeftCenter);
			Padding = new Thickness(1);

			this.AddChangeWatcher(() => model.CurrentPath, (p) => {
				UpdatePathBar();
			});
		}

		private void CreateButtons()
		{
			topFoldersPaths = GetTopFoldersPaths(model.CurrentPath);
			buttons = new PathBarButton[topFoldersPaths.Count];

			Nodes.Add(rootArrowButton = new PathArrowButton(openPath));
			for (var i = topFoldersPaths.Count - 1; i >= 0; i--) {
				Nodes.Add(buttons[i] = new PathBarButton(openPath, topFoldersPaths[i]));
			}
		}

		private void RemoveButtons()
		{
			if (buttons != null) {
				for (var i = buttons.Length - 1; i >= 0; i--) {
					Nodes.Remove(buttons[i]);
				}
				Nodes.Remove(rootArrowButton);
			}
		}

		private void UpdatePathBar()
		{
			RemoveButtons();
			CreateButtons();
		}

		public static List<string> GetTopFoldersPaths(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}
			var topFolders = new List<string>();
			topFolders.Add(path);
			var p = Path.GetDirectoryName(topFolders[topFolders.Count - 1]);
			while (p != null) {
				topFolders.Add(p);
				p = Path.GetDirectoryName(topFolders[topFolders.Count - 1]);
			}
			return topFolders;
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

		public PathBarButton(Func<string, bool> openPath, string path) : base()
		{
			Layout = new HBoxLayout();
			HitTestTarget = true;

			folderButton = new PathFolderButton(openPath, path);
			arrowButton = new PathArrowButton(openPath, path);

			Nodes.Add(folderButton);
			Nodes.Add(arrowButton);

			Updating += (float delta) => {
				if (arrowButton.Expanded) {
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
		private PathBarButtonState state;

		public void SetState(PathBarButtonState state)
		{
			if (this.state != state) {
				this.state = state;
				CommonWindow.Current.Invalidate();
			}
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
		public PathBarButtonState State;

		public PathFolderButton(Func<string, bool> openPath, string path) : base()
		{
			Text = GetName(path);
			Presenter = new PathButtonPresenter();
			base.Presenter = Presenter;
			MinMaxHeight = 20;
			MinMaxWidth = Renderer.MeasureTextLine(Text, Theme.Metrics.TextHeight, 0).X + 7;
			Clicked += () => openPath(path);
#if WIN
			UpdateHandler showContextMenu = null;
			showContextMenu = delta => {
				Updating -= showContextMenu;
				SystemShellContextMenu.Instance.Show(path);
			};
			// TODO: Immediate showing context menu causes weird crash in GestureManager
			Gestures.Add(new ClickGesture(1, () => Updating += showContextMenu));
#endif // WIN
		}

		public void SetState(PathBarButtonState state)
		{
			Presenter.SetState(state);
		}

		public static string GetName(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else if (
				path[path.Length - 1] == Path.DirectorySeparatorChar &&
				path[path.Length - 2] == Path.VolumeSeparatorChar
			) {
				// Root
				return path.Remove(path.Length - 1);
			} else {
				// Folder
				int i;
				for (i = path.Length - 1; i >= 0; i--) {
					if (path[i] == Path.DirectorySeparatorChar) {
						i++;
						break;
					}
				}
				return path.Substring(i);
			}
		}
	}

	public class PathArrowButton : ThemedButton
	{
		private string path;
		private DirectoryPicker picker;
		private Func<string, bool> openPath;
		private new PathButtonPresenter Presenter;
		private Image icon;
		public bool Expanded { get; private set; }
		public PathBarButtonState State;

		public PathArrowButton(Func<string, bool> openPath, string path = null) : base()
		{
			this.path = path;
			this.openPath = openPath;
			MinMaxHeight = 20;
			Presenter = new PathButtonPresenter();
			base.Presenter = Presenter;
			if (path == null) {
				Updating += (float delta) => {
					var prevState = State;
					if (Expanded) {
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
					if (prevState != State) {
						Presenter.SetState(State);
					}
				};
			}
			Gestures.Add(new ClickGesture(0, FlipState));
			Layout = new HBoxLayout();
			MinMaxSize = new Vector2(11, 20);
			Nodes.Add(icon = new Image {
				LayoutCell = new LayoutCell {
					Alignment = new Alignment { X = HAlignment.Center, Y = VAlignment.Center }
				},
				MinMaxSize = new Vector2(11, 6),
				Texture = IconPool.GetTexture("Filesystem.PathSeparatorCollapsed")
			});
			Expanded = false;
		}

		public void SetState(PathBarButtonState state)
		{
			Presenter.SetState(state);
		}

		private void FlipState()
		{
			if (!Expanded) {
				Expanded = true;
				icon.Texture = IconPool.GetTexture("Filesystem.PathSeparatorExpanded");
				var indent = 14;
				var pickerPosition = Window.Current.LocalToDesktop(GlobalPosition + new Vector2(-indent, Height));
				picker = new DirectoryPicker(openPath, pickerPosition, path);		
				picker.Closing += FlipState;
			} else {
				Expanded = false;
				icon.Texture = IconPool.GetTexture("Filesystem.PathSeparatorCollapsed");
				picker.Close();
			}
		}
	}

	public class DirectoryPicker
	{
		private Func<string, bool> openPath;
		private ThemedScrollView scrollView;
		private Window window;
		private WindowWidget rootWidget;
		private bool closed;
		public event Action Closing;

		public DirectoryPicker(Func<string, bool> openPath, Vector2 globalPosition, string path = null)
		{
			this.openPath = openPath;

			List<FilesystemItem> filesystemItems = new List<FilesystemItem>();
			if (path == null) {
				var logicalDrives = Directory.GetLogicalDrives();
				var availableRoots = GetAvailableRootsPathsFromLogicalDrives(logicalDrives);
				filesystemItems = GetFilesystemItems(availableRoots);
			} else {
				var internalFolders = GetInternalFoldersPaths(path);
				filesystemItems = GetFilesystemItems(internalFolders);
			}

			scrollView = new ThemedScrollView();
			scrollView.Content.Layout = new VBoxLayout();
			scrollView.Content.Padding = new Thickness(4);
			scrollView.Content.Nodes.AddRange(filesystemItems);

			// Like in Windows File Explorer
			const int MaxItemsOnPicker = 19; 
			var itemsCount = Math.Min(filesystemItems.Count, MaxItemsOnPicker);
			var clientSize = new Vector2(
				FilesystemItem.ItemWidth,
				(FilesystemItem.IconSize + 2 * FilesystemItem.ItemPadding) * itemsCount) + new Vector2(scrollView.Content.Padding.Left * 2);
			scrollView.MinMaxSize = clientSize;

			var windowOptions = new WindowOptions {
				ClientSize = scrollView.MinSize,
				MinimumDecoratedSize = scrollView.MinSize,
				FixedSize = true,
				Style = WindowStyle.Borderless,
				Centered = globalPosition == Vector2.Zero,
				Visible = false,
			};
			window = new Window(windowOptions);
			window.Deactivated += Close;

			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Layout = new VBoxLayout(),
				LayoutBasedWindowSize = true,
				Nodes = {
					scrollView
				}
			};

			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => Window.Current.Invalidate());

			rootWidget.Presenter = new DelegatePresenter<Widget>(_ => {
				rootWidget.PrepareRendererState();
				Renderer.DrawRect(Vector2.One, rootWidget.ContentSize, Theme.Colors.DirectoryPickerBackground);
			});
			rootWidget.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(_ => {
				rootWidget.PrepareRendererState();
				Renderer.DrawRectOutline(Vector2.Zero, rootWidget.ContentSize, Theme.Colors.DirectoryPickerOutline, thickness: 1);
			}));

			window.Visible = true;
			if (globalPosition != Vector2.Zero) {
				window.ClientPosition = globalPosition;
			}
		}

		public void Close()
		{
			if (!closed) {
				closed = true;
				Closing?.Invoke();
				window.Close();
			}
		}
	
		public static List<string> GetInternalFoldersPaths(string path)
		{
			var foldersPaths = new List<string>();
			foreach (var item in Directory.EnumerateDirectories(path).OrderBy(f => f)) {
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
						Renderer.DrawRect(Vector2.Zero, item.Size, Theme.Colors.DirectoryPickerItemHoveredBackground);
					}
				}));
				item.Updating += (float delta) => {
					if (item.Input.WasMouseReleased(0)) {
						window.Close();
						openPath(item.FilesystemPath);
					} else if (item.Input.WasMouseReleased(1)) {
						SystemShellContextMenu.Instance.Show(item.FilesystemPath);
					}
				};
			}
			return items;
		}

		public static List<string> GetAvailableRootsPathsFromLogicalDrives(string[] logicalDrives)
		{
			var realRootsCount = 0;
			foreach (var path in logicalDrives) {
				if (Directory.Exists(path)) {
					realRootsCount++;
				}
			}
			List<string> availableRoots = new List<string>();
			var i = 0;
			foreach (var root in logicalDrives) {
				if (Directory.Exists(root)) {
					availableRoots.Add(root);
					i++;
					if (i == realRootsCount) break;
				}
			}
			return availableRoots;
		}
	}
}

using Lime;
using System.Collections.Generic;
using System.IO;

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
		private Model model;
		private PathBar pathBar;
		private ThemedEditBox editor;
		private AddressBarState state;
		private FilesystemView view;
		private WidgetBoundsPresenter outlineRect;

		public string Path
		{
			get {
				if (state == AddressBarState.PathBar) {
					return buffer;
				} else {
					return editor.TextWidget.Text;
				}
			}
			set {
				if (state == AddressBarState.PathBar) {
					buffer = AdjustFolderPath(value);
				} else {
					editor.TextWidget.Text = AdjustFolderPath(value);
				}
			}
		}

		public AddressBar(FilesystemView view, Model model)
		{
			this.view = view;
			this.model = model;
			Layout = new HBoxLayout();
			Padding.Right += 4;
			buffer = Path = model.CurrentPath;
			outlineRect = new WidgetBoundsPresenter(Color4.Gray);
			CompoundPostPresenter.Add(outlineRect);

			state = AddressBarState.PathBar;
			CreatePathBar();
			//Updating += (float delta) => {
			//if(state == AddressBarState.PathBar) {
			//	if (pathBar.IsMouseOver() && ) {
			//		ChangeState();
			//	}
			//} else {

			//}
			//if () {
			// Run pathBar
			// Если кликнули и не попали не по одному из ПОТОМКОВ пасбара, а по пустому месту самого пасбара
			// changeState()
			//} else {
			// Run editor
			// Если нажали Ентер при вводе пути - чекаем путь
			// Если путь правильный
			// Записываем в буфер AdjustFolderPath

			// changeState()
			//}
			//};
		}

		public static string AdjustFolderPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			} else if (path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar && path[path.Length - 2] != ':') {
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

		private void ChangeState()
		{
			if (state == AddressBarState.Editor) {
				state = AddressBarState.PathBar;
				DeleteEditor();
				CreatePathBar();
			} else {
				state = AddressBarState.Editor;
				DeletePathBar();
				CreateEditor();
			}
		}

		private void CreateEditor()
		{
			Nodes.Add(editor = new ThemedEditBox());
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Updating += (float delta) => {
				if (editor.Input.WasKeyPressed(Key.Enter)) {
					if (view.Open(Path)) {
						Path = AdjustFolderPath(Path);
						buffer = Path;
						ChangeState();
					} else {
						Path = buffer;
					}
				}
			};
		}

		private void DeleteEditor()
		{
			Nodes.Remove(editor);
			editor.Updating -= (float delta) => {
				if (editor.Input.WasKeyPressed(Key.Enter)) {
					if (view.Open(Path)) {
						Path = AdjustFolderPath(Path);
						buffer = Path;
						ChangeState();
					} else {
						Path = buffer;
					}
				}
			};
			editor = null;
		}

		private void CreatePathBar()
		{
			Nodes.Add(pathBar = new PathBar(view, this));
			pathBar.LayoutCell = new LayoutCell(Alignment.Center);
			pathBar.Updating += (float delta) => {
				if (pathBar.IsMouseOver() && pathBar.Input.WasMouseReleased(Key.Mouse0)) {
					ChangeState();
				}
			};
		}

		private void DeletePathBar()
		{
			Nodes.Remove(pathBar);
			pathBar.Updating -= (float delta) => {
				if (pathBar.IsMouseOver() && pathBar.Input.WasMouseReleased(Key.Mouse0)) {
					ChangeState();
				}
			};
			pathBar = null;
		}
	}

	public class PathBar : ThemedTabBar
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
			arrowButtons = new PathArrowButton[countOfFolders];

			Nodes.Add(rootButton = new PathRootButton(view, buffer));
			for (var i = 0; i < countOfFolders; i++) {
				Nodes.Add(folderButtons[i] = new PathFolderButton(view, topFoldersPaths[i]));
				Nodes.Add(arrowButtons[i] = new PathArrowButton(view, topFoldersPaths[i]));
			}
		}

		private void DestroyButtons()
		{
			Nodes.Remove(rootButton);
			for (var i = 0; i < countOfFolders; i++) {
				Nodes.Remove(arrowButtons[i]);
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
				if (path[i] == System.IO.Path.DirectorySeparatorChar && i + 1 != path.Length) {
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
					topFolders[i] = Path.GetDirectoryName(topFolders[i + 1]);
					i--;
				}
			}
			return topFolders;
		}
	}

	public class PathFolderButton : ThemedTab
	{
		public PathFolderButton(FilesystemView view, string path) : base()
		{
			Text = GetNameOfFolder(path);

			Gestures.Add(new ClickGesture(0, () => {
				view.Open(path);
			}));
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

	public class PathRootButton : ThemedTab
	{
		public PathRootButton(FilesystemView view, string path) : base()
		{
			Text = GetNameOfRoot(path);

			Gestures.Add(new ClickGesture(0, () => {
				view.Open(System.IO.Path.GetPathRoot(path));
			}));
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

	public class PathArrowButton : ThemedTab
	{
		public enum PathArrowButtonState
		{
			Right,
			Down
		}
		private List<FilesystemItem> filesystemItems;
		private List<string> internalFolders;
		private string path;
		private Model model;
		private FilesystemView view;
		private PathArrowButtonState state;

		public PathArrowButton(FilesystemView view, string path) : base()
		{
			this.path = path;
			this.view = view;
			model = new Model(path);
			ToRight();
			Gestures.Add(new ClickGesture(0, () => {
				ChangeState();
			}));
		}

		private void ToRight()
		{
			Text = ">";
			state = PathArrowButtonState.Right;
		}

		private void ToDown()
		{
			Text = "v";
			state = PathArrowButtonState.Down;
			internalFolders = GetInternalFolders(path);
			filesystemItems = GetFilesystemItems(internalFolders);
			var menu = new Menu();
			foreach (var item in filesystemItems) {
				var command = new Command(Path.GetFileName(item.FilesystemPath), () => { view.Open(item.FilesystemPath); });
				menu.Add(command);
			}
			menu.Popup();
		}

		private void ChangeState()
		{
			if (state == PathArrowButtonState.Right) {
				ToDown();
			} else {
				ToRight();
			}
		}

		private List<string> GetInternalFolders(string path)
		{
			var foldersPaths = new List<string>();
			foreach (var item in model.EnumerateDirectories(path)) {
				foldersPaths.Add(item);
			}
			return foldersPaths;
		}

		private List<FilesystemItem> GetFilesystemItems(List<string> paths)
		{
			var items = new List<FilesystemItem>();
			foreach(var path in paths) {
				items.Add(new FilesystemItem(path));
			}
			return items;
		}
	}
}

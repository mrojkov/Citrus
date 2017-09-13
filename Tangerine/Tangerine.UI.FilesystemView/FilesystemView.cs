using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	// Single instance of view on filesystem, cooking rules editor and preview
	// Created and managed by FilesystemPane
	public class FilesystemView
	{
		public Widget RootWidget;
		private ThemedScrollView scrollView;
		private FilesystemToolbar toolbar;
		private Model model;
		private Vector2 rectSelectionBeginPoint;
		private Vector2 rectSelectionEndPoint;
		private bool rectSelecting;
		private readonly Selection selection = new Selection();
		private Lime.FileSystemWatcher fsWatcher;
		private CookingRulesEditor crEditor;
		private Preview preview;
		private List<Tuple<string, Selection>> navHystory = new List<Tuple<string, Selection>>();
		private int navHystoryIndex = -1;
		private NodeToggler toggleCookingRules;
		private NodeToggler togglePreview;
		private bool beginDrag;
		private Vector2 dragStartPosition;
		private ThemedHSplitter cookingRulesSplitter;
		private ThemedVSplitter selectionPreviewSplitter;
		private Icon lastKeyboardSelectedIcon;
		private Icon lastKeyboardRangeSelectionEndIcon;

		public void Split(SplitterType type)
		{
			FilesystemPane.Split(this, type);
		}

		public void Close()
		{
			FilesystemPane.Close(this);
		}

		public void GoBackward()
		{
			var newIndex = navHystoryIndex - 1;
			if (newIndex < 0 || newIndex >= navHystory.Count) {
				return;
			}
			var i = navHystory[newIndex];
			GoTo(i.Item1);
			foreach (var s in i.Item2) {
				selection.Select(s);
			}
			navHystoryIndex = newIndex;
		}

		public void GoForward()
		{
			var newIndex = navHystoryIndex + 1;
			if (newIndex >= navHystory.Count) {
				return;
			}
			var i = navHystory[newIndex];
			GoTo(i.Item1);
			foreach (var s in i.Item2) {
				selection.Select(s);
			}
			navHystoryIndex = newIndex;
		}

		private void AddToNavHystory(string path)
		{
			if (navHystory.Count > 0 && navHystory[navHystoryIndex].Item1 == path) {
				return;
			}
			var i = new Tuple<string, Selection>(path, selection.Clone());
			navHystory.Add(i);
			int newIndex = navHystoryIndex + 1;
			navHystory.RemoveRange(newIndex, navHystory.Count - newIndex - 1);
			navHystoryIndex = newIndex;
		}

		private void InvalidateFSWatcher(string path)
		{
			fsWatcher?.Dispose();
			fsWatcher = new Lime.FileSystemWatcher(path, includeSubdirectories: false);
			// TODO: throttle
			Action OnFsWatcherChanged = () => {
				InvalidateView(model.CurrentPath);
			};
			fsWatcher.Deleted += (p) => {
				selection.Deselect(p);
				OnFsWatcherChanged();
			};
			fsWatcher.Created += (p) => OnFsWatcherChanged();
			fsWatcher.Renamed += (p) => OnFsWatcherChanged();
		}

		public FilesystemView()
		{
			RootWidget = new Widget() { Id = "FSRoot" };
			RootWidget.FocusScope = new KeyboardFocusScope(RootWidget);
			scrollView = new ThemedScrollView {
				TabTravesable = new TabTraversable()
			};
			// TODO: Display path
			RootWidget.AddChangeWatcher(() => model.CurrentPath, (path) => toolbar.Path = path.ToString());
			crEditor = new CookingRulesEditor(NavigateAndSelect);
			crEditor.RootWidget.TabTravesable = new TabTraversable();
			preview = new Preview();
			preview.RootWidget.TabTravesable = new TabTraversable();
		}

		// Component with user preferences should be added to rootWidget at this moment
		public void Initialize()
		{
			var up = RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode;
			toolbar = new FilesystemToolbar(this);
			toolbar.TabTravesable = new TabTraversable();
			model = new Model(up.Path);
			InitializeWidgets();
			selectionPreviewSplitter.Stretches = Splitter.GetStretchesList(up.SelectionPreviewSplitterStretches, 1, 1);
			cookingRulesSplitter.Stretches = Splitter.GetStretchesList(up.CookingRulesSplitterStretches, 1, 1);
			toggleCookingRules = new NodeToggler(crEditor.RootWidget, () => { crEditor.Invalidate(selection); });
			togglePreview = new NodeToggler(preview.RootWidget, () => { preview.Invalidate(selection); });
			if (!up.ShowCookingRulesEditor) {
				toggleCookingRules.Toggle();
			}
			if (!up.ShowSelectionPreview) {
				togglePreview.Toggle();
			}
			foreach (var n in RootWidget.Descendants) {
				var w = n.AsWidget;
				if (w.TabTravesable != null) {
					w.CompoundPostPresenter.Add(new Theme.KeyboardFocusBorderPresenter(2.0f));
					w.HitTestTarget = true;
					w.Updated += (dt) => {
						if (w.IsMouseOver() && !w.IsFocused()) {
							if (w.Input.WasKeyPressed(Key.Mouse0)) {
								w.SetFocus();
							}
							w.Input.ConsumeKey(Key.Mouse0);
						}
					};
				}
			}
		}

		private void NavigateAndSelect(string filename)
		{
			GoTo(Path.GetDirectoryName(filename));
			selection.Select(filename);
		}

		void InitializeWidgets()
		{
			RootWidget.AddChangeWatcher(() => selection.Version, Selection_Changed);
			scrollView.Content.Layout = new FlowLayout { Spacing = 1.0f };
			scrollView.Padding = new Thickness(5.0f);
			scrollView.Content.CompoundPostPresenter.Insert(0, new DelegatePresenter<Widget>(RenderFilesWidgetRectSelection));
			scrollView.Updated += ScrollViewUpdated;
			scrollView.Content.Presenter = new DelegatePresenter<Widget>((w) => {
				w.PrepareRendererState();
				var wp = w.ParentWidget;
				var p = wp.Padding;
				Renderer.DrawRect(-w.Position + Vector2.Zero - new Vector2(p.Left, p.Top),
					-w.Position + wp.Size + new Vector2(p.Right, p.Bottom), Theme.Colors.WhiteBackground);
			});
			RootWidget.AddChangeWatcher(() => rectSelectionEndPoint, WhenSelectionRectChanged);
			RootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => {
				if (value != null && scrollView.Content == value.Parent) {
					Window.Current.Invalidate();
				}
			});
			RootWidget.AddChangeWatcher(() => model.CurrentPath, (p) => {
				var up = RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode;
				up.Path = p;
				AddToNavHystory(p);
				selection.Clear();
				InvalidateView(p);
				InvalidateFSWatcher(p);
				preview.ClearTextureCache();
				lastKeyboardSelectedIcon = scrollView.Content.Nodes.FirstOrNull() as Icon;
			});
			RootWidget.Layout = new VBoxLayout();
			RootWidget.AddNode((cookingRulesSplitter = new ThemedHSplitter {
				Nodes = {
					(new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							toolbar,
							(selectionPreviewSplitter = new ThemedVSplitter {
								Nodes = {
									scrollView,
									preview.RootWidget,
								}
							})
						}}),
						crEditor.RootWidget,
				}
			}));
		}

		private void Selection_Changed(int version)
		{
			crEditor.Invalidate(selection);
			preview.Invalidate(selection);
			Window.Current.Invalidate();
		}

		private void InvalidateView(string path)
		{
			scrollView.Content.Nodes.Clear();
			foreach (var item in model.EnumerateItems()) {
				var iconWidget = new Icon(item);
				iconWidget.CompoundPostPresenter.Insert(0, new DelegatePresenter<Icon>(RenderIconSelection));
				iconWidget.Updated += (dt) => {
					if (!iconWidget.IsMouseOver()) return;
					var input = iconWidget.Input;
					if (input.WasKeyPressed(Key.Mouse0DoubleClick)) {
						input.ConsumeKey(Key.Mouse0DoubleClick);
						Open(item);
					}
					if (iconWidget.Input.WasKeyReleased(Key.Mouse1)) {
						iconWidget.Input.ConsumeKey(Key.Mouse1);
						SystemShellContextMenu.Instance.Show(item);
					}
					if (iconWidget.Input.WasKeyReleased(Key.Mouse0)) {
						if (!selection.Contains(item)) {
							iconWidget.Input.ConsumeKey(Key.Mouse0);
							if (!iconWidget.Input.IsKeyPressed(Key.Control)) {
								selection.Clear();
							}
							selection.Select(item);
						}
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse0)) {
						input.ConsumeKey(Key.Mouse0);
						if (input.IsKeyPressed(Key.Control) && !input.IsKeyPressed(Key.Shift)) {
							input.ConsumeKey(Key.Control);
							if (selection.Contains(item)) {
								selection.Deselect(item);
							} else {
								selection.Select(item);
							}
							// TODO: Ctrl + Shift, Shift clicks
						} else {
							if (!selection.Contains(item)) {
								selection.Clear();
								selection.Select(item);
							}
							beginDrag = true;
							dragStartPosition = Window.Current.Input.MousePosition;
						}
						Window.Current?.Invalidate();
					}
				};
				scrollView.Content.AddNode(iconWidget);
			}
		}

		private void Open(string path)
		{
			var attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
				GoTo(path);
			} else {
				if (path.EndsWith(".scene") || path.EndsWith(".tan")) {
					string localPath;
					if (!Project.Current.TryGetAssetPath(path, out localPath)) {
						AlertDialog.Show("Can't open a document outside the project directory");
					} else {
						Project.Current.OpenDocument(localPath);
					}
				}
			}
		}

		private void OpenSpecial(string path)
		{
			System.Diagnostics.Process.Start(path);
		}

		private void RenderIconSelection(Icon icon)
		{
			if (selection.Contains(icon.FilesystemPath)) {
				icon.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, icon.Size, Theme.Colors.SelectedBackground.Transparentify(0.5f));
				Renderer.DrawRectOutline(Vector2.Zero, icon.Size, Theme.Colors.SelectedBackground);
			} else if (icon.IsMouseOver()) {
				icon.PrepareRendererState();
				Renderer.DrawRect(
					Vector2.Zero,
					icon.Size,
					Theme.Colors.SelectedBackground.Transparentify(0.8f));
			}
		}

		private void WhenSelectionRectChanged(Vector2 value)
		{
			if (!rectSelecting) {
				return;
			}
			var p0 = rectSelectionBeginPoint;
			var p1 = rectSelectionEndPoint;
			var r0 = new Rectangle(new Vector2(Mathf.Min(p0.X, p1.X), Mathf.Min(p0.Y, p1.Y)),
				new Vector2(Mathf.Max(p0.X, p1.X), Mathf.Max(p0.Y, p1.Y)));
			foreach (var n in scrollView.Content.Nodes) {
				var ic = n as Icon;
				var r1 = new Rectangle(ic.Position, ic.Position + ic.Size);
				if (Rectangle.Intersect(r0, r1) != Rectangle.Empty) {
					selection.Select(ic.FilesystemPath);
				} else {
					if (selection.Contains(ic.FilesystemPath)) {
						selection.Deselect(ic.FilesystemPath);
					}
				}
			}
		}

		private const float typeNavigationInterval = 0.5f;
		private float typeNavigationTimeout = 0.0f;
		private string typeNavigationPrefix = string.Empty;

		private void ScrollViewUpdated(float dt)
		{
			if (beginDrag) {
				if ((dragStartPosition - Window.Current.Input.MousePosition).Length > 5.0f) {
					beginDrag = false;
					CommonWindow.Current.DragFiles(selection.ToArray());
				}
				if (Window.Current.Input.WasKeyReleased(Key.Mouse0)) {
					beginDrag = false;
				}
				return;
			}
			var input = scrollView.Input;
			if (scrollView.IsMouseOver()) {
				if (!rectSelecting && input.WasKeyPressed(Key.Mouse0)) {
					input.ConsumeKey(Key.Mouse0);
					rectSelecting = true;
					rectSelectionBeginPoint = scrollView.Content.Input.LocalMousePosition;
				}
				if (input.WasKeyPressed(Key.Mouse1)) {
					input.ConsumeKey(Key.Mouse1);
					rectSelecting = false;
					selection.Clear();
					SystemShellContextMenu.Instance.Show(model.CurrentPath);
				}
			}
			if (rectSelecting) {
				if (Window.Current.Input.WasKeyReleased(Key.Mouse0)) {
					Window.Current.Input.ConsumeKey(Key.Mouse0);
					scrollView.SetFocus();
					rectSelecting = false;
				}
				rectSelectionEndPoint = scrollView.Content.Input.LocalMousePosition;
				Window.Current.Invalidate();
			}

			typeNavigationTimeout -= dt;
			if (scrollView.IsFocused()) {
				{ // text input
					if (!string.IsNullOrEmpty(input.TextInput)) {
						if (typeNavigationTimeout <= 0.0f) {
							typeNavigationPrefix = string.Empty;
						}
						typeNavigationTimeout = typeNavigationInterval;
						var prevPrefix = typeNavigationPrefix;
						bool offset = false;
						if (prevPrefix == input.TextInput) {
							offset = true;
						} else {
							typeNavigationPrefix += input.TextInput;
						}
						var matches = scrollView.Content.Nodes
							.Select(i => i as Icon)
							.Where(i => {
								var a = Path.GetFileName(i.FilesystemPath);
								var b = typeNavigationPrefix;
								return a.StartsWith(b, true, CultureInfo.CurrentCulture);
							})
							.ToList();
						if (matches.Count != 0) {
							var index = matches.IndexOf(lastKeyboardSelectedIcon);
							if (index == -1) {
								index = 0;
							}
							if (offset) {
								index = (index + 1) % matches.Count;
							}
							selection.Clear();
							selection.Select(matches[index].FilesystemPath);
							lastKeyboardSelectedIcon = matches[index];
						}
					}
				}

				{ // other shortcuts
					if (Cmds.Cancel.WasIssued()) {
						typeNavigationTimeout = typeNavigationInterval;
						typeNavigationPrefix = string.Empty;
					} else if (Window.Current.Input.WasKeyReleased(Key.Menu)) {
						if (!selection.Empty) {
							Window.Current.Input.ConsumeKey(Key.Menu);
							SystemShellContextMenu.Instance.Show(selection.ToArray(), lastKeyboardSelectedIcon.GlobalPosition);
						}
					} else if (Cmds.GoBack.WasIssued()) {
						GoBackward();
					} else if (Cmds.GoForward.WasIssued()) {
						GoForward();
					} else if (Cmds.GoUp.WasIssued() || Cmds.GoUpAlso.WasIssued()) {
						GoUp();
					} else if (Cmds.Enter.WasIssued()) {
						if (lastKeyboardSelectedIcon != null) {
							Open(lastKeyboardSelectedIcon.FilesystemPath);
						}
					} else if (Cmds.EnterSpecial.WasIssued()) {
						if (lastKeyboardSelectedIcon != null) {
							OpenSpecial(lastKeyboardSelectedIcon.FilesystemPath);
						}
					} else if (Command.SelectAll.WasIssued()) {
						selection.Clear();
						selection.SelectRange(scrollView.Content.Nodes.Select(n => (n as Icon).FilesystemPath));
					}
				}

				{ // navigation hotkeys
					int indexDelta = 0;
					bool select = false;
					bool toggle = false;
					var index = 0;
					var maxIndex = scrollView.Content.Nodes.Count - 1;
					if (lastKeyboardSelectedIcon != null) {
						index = scrollView.Content.Nodes.IndexOf(lastKeyboardSelectedIcon);
					}

					for (int navType = 0; navType < navCommands.Count; navType++) {
						for (int navOffset = 0; navOffset < navCommands[navType].Count; navOffset++) {
							var cmd = navCommands[navType][navOffset];
							if (cmd.WasIssued()) {
								select = navType == 1;
								toggle = navType == 2;
								switch (navOffset) {
								case 0: indexDelta = -1; break;
								case 1: indexDelta = 1; break;
								case 2: indexDelta = -(scrollView.Content.Layout as FlowLayout).ColumnCount(0); break;
								case 3: indexDelta = (scrollView.Content.Layout as FlowLayout).ColumnCount(0); break;
								case 4: indexDelta = -(scrollView.Content.Layout as FlowLayout).ColumnCount(0) * 1; break; // * rows per screen
								case 5: indexDelta = (scrollView.Content.Layout as FlowLayout).ColumnCount(0); break;
								case 6: indexDelta = -index; break;
								case 7: indexDelta = maxIndex - index; break;
								}
							}
						}
					}
					if (indexDelta != 0) {
						if (select) {
							int selectionEndIndex = lastKeyboardRangeSelectionEndIcon != null
								? scrollView.Content.Nodes.IndexOf(lastKeyboardRangeSelectionEndIcon)
								: index;
							int newIndex = selectionEndIndex + indexDelta;
							if (newIndex >= 0 && newIndex <= maxIndex) {
								selection.Clear();
								for (int i = Math.Min(index, newIndex); i <= Math.Max(index, newIndex); i++) {
									var path = (scrollView.Content.Nodes[i] as Icon).FilesystemPath;
									selection.Select(path);
								}
								lastKeyboardRangeSelectionEndIcon = scrollView.Content.Nodes[newIndex] as Icon;
							}
						} else if (toggle) {

						} else {
							int newIndex = index + indexDelta;
							if (newIndex >= 0 && newIndex <= maxIndex) {
								lastKeyboardSelectedIcon = scrollView.Content.Nodes[newIndex] as Icon;
								lastKeyboardRangeSelectionEndIcon = null;
								var path = lastKeyboardSelectedIcon.FilesystemPath;
								selection.Clear();
								selection.Select(path);
							}
						}
					}
				}
				foreach (var cmd in consumingCommands) {
					cmd.Consume();
				}
			}
		}

		private static class Cmds
		{
			public static readonly ICommand Left = new Command(Key.Left);
			public static readonly ICommand Right = new Command(Key.Right);
			public static readonly ICommand Up = new Command(Key.Up);
			public static readonly ICommand Down = new Command(Key.Down);
			public static readonly ICommand PageUp = new Command(Key.PageUp);
			public static readonly ICommand PageDown = new Command(Key.PageDown);
			public static readonly ICommand Home = new Command(Key.Home);
			public static readonly ICommand End = new Command(Key.End);
			public static readonly ICommand SelectLeft = new Command(Modifiers.Shift, Key.Left);
			public static readonly ICommand SelectRight = new Command(Modifiers.Shift, Key.Right);
			public static readonly ICommand SelectUp = new Command(Modifiers.Shift, Key.Up);
			public static readonly ICommand SelectDown = new Command(Modifiers.Shift, Key.Down);
			public static readonly ICommand SelectPageUp = new Command(Modifiers.Shift, Key.PageUp);
			public static readonly ICommand SelectPageDown = new Command(Modifiers.Shift, Key.PageDown);
			public static readonly ICommand SelectHome = new Command(Modifiers.Shift, Key.Home);
			public static readonly ICommand SelectEnd = new Command(Modifiers.Shift, Key.End);
			public static readonly ICommand ToggleLeft = new Command(Modifiers.Command, Key.Left);
			public static readonly ICommand ToggleRight = new Command(Modifiers.Command, Key.Right);
			public static readonly ICommand ToggleUp = new Command(Modifiers.Command, Key.Up);
			public static readonly ICommand ToggleDown = new Command(Modifiers.Command, Key.Down);
			public static readonly ICommand TogglePageUp = new Command(Modifiers.Command, Key.PageUp);
			public static readonly ICommand TogglePageDown = new Command(Modifiers.Command, Key.PageDown);
			public static readonly ICommand ToggleHome = new Command(Modifiers.Command, Key.Home);
			public static readonly ICommand ToggleEnd = new Command(Modifiers.Command, Key.End);
			public static readonly ICommand SelectAll = new Command(Modifiers.Command, Key.A);
			public static readonly ICommand Cancel = new Command(Key.Escape);
			public static readonly ICommand Enter = new Command(Key.Enter);
			public static readonly ICommand EnterSpecial = new Command(Modifiers.Command, Key.Enter);
			public static readonly ICommand GoUp = new Command(Key.BackSpace);
			// Also go up on Alt + Up
			public static readonly ICommand GoUpAlso = new Command(Modifiers.Alt, Key.Up);
			public static readonly ICommand GoBack = new Command(Modifiers.Alt, Key.Left);
			public static readonly ICommand GoForward = new Command(Modifiers.Alt, Key.Right);
			public static readonly ICommand Select = new Command(Modifiers.Command, Key.Space);
		}

		static readonly List<List<ICommand>> navCommands = new List<List<ICommand>> {
			// simple navigation
			new List<ICommand> {
				Cmds.Left,
				Cmds.Right,
				Cmds.Up,
				Cmds.Down,
				Cmds.PageUp,
				Cmds.PageDown,
				Cmds.Home,
				Cmds.End,
			},
			// Range-select (shift) navigation
			new List<ICommand> {
				Cmds.SelectLeft,
				Cmds.SelectRight,
				Cmds.SelectUp,
				Cmds.SelectDown,
				Cmds.SelectPageUp,
				Cmds.SelectPageDown,
				Cmds.SelectHome,
				Cmds.SelectEnd,
			},
			// Toggle-select (hold ctrl, navigate, toggle with space)
			new List<ICommand> {
				Cmds.ToggleLeft,
				Cmds.ToggleRight,
				Cmds.ToggleUp,
				Cmds.ToggleDown,
				Cmds.TogglePageUp,
				Cmds.TogglePageDown,
				Cmds.ToggleHome,
				Cmds.ToggleEnd,
			},
		};

		static readonly List<ICommand> consumingCommands =
			Command.Editing.Union(
			Key.Enumerate().Where(k => k.IsPrintable()).Select(i => new Command(i)).Union(
			Key.Enumerate().Where(k => k.IsPrintable()).Select(i => new Command(new Shortcut(Modifiers.Shift, i)))).Union(
				new[] {
					Cmds.SelectAll,
					Command.SelectAll,
					Cmds.Cancel,
					Cmds.Enter,
					Cmds.GoUp,
					Cmds.GoUpAlso,
					Cmds.GoBack,
					Cmds.GoForward,
					Cmds.GoUpAlso,
					Cmds.EnterSpecial,
				}
			).Union(
				navCommands.SelectMany(i => i)
			)).ToList();

		private void RenderFilesWidgetRectSelection(Widget canvas)
		{
				if (!rectSelecting) {
					return;
				}
				canvas.PrepareRendererState();
				Renderer.DrawRect(rectSelectionBeginPoint, rectSelectionEndPoint, Theme.Colors.SelectedBackground.Transparentify(0.5f));
				Renderer.DrawRectOutline(rectSelectionBeginPoint, rectSelectionEndPoint, Theme.Colors.SelectedBackground.Darken(0.2f));
		}

		public void GoUp()
		{
			model.GoUp();
		}

		public void GoTo(string path)
		{
			model.GoTo(path);
		}

		private class NodeToggler
		{
			private Node savedParent;
			private int savedIndex;
			private Node node;
			private Action invalidator;
			public NodeToggler(Node n, Action invalidator)
			{
				node = n;
				this.invalidator = invalidator;
			}
			public void Toggle()
			{
				if (node.Parent != null) {
					savedParent = node.Parent;
					savedIndex = savedParent.Nodes.IndexOf(node);
					node.Unlink();
				} else {
					savedParent.Nodes.Insert(Mathf.Clamp(savedIndex, 0, savedParent.Nodes.Count), node);
					invalidator?.Invoke();
				}
			}
		}

		public void TogglePreview()
		{
			togglePreview.Toggle();
		}

		public void ToggleCookingRules()
		{
			toggleCookingRules.Toggle();
		}
	}
}

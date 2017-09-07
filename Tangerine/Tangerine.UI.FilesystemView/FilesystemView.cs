using System;
using System.Collections.Generic;
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
			RootWidget = new Widget();
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
					w.CompoundPostPresenter.Add(new Theme.KeyboardFocusBorderPresenter());
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
			selection.Changed += Selection_Changed;
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

		private void Selection_Changed()
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
						OnDoubleClick(item);
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

		private void OnDoubleClick(string path)
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

		private void ScrollViewUpdated(float delta)
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

			if (scrollView.IsFocused()) {
				int indexDelta = 0;
				if (input.WasKeyPressed(shRight) || input.WasKeyRepeated(shRight)) {
					indexDelta = 1;
				} else if (input.WasKeyPressed(shLeft) || input.WasKeyRepeated(shLeft)) {
					indexDelta = -1;
				} else if (input.WasKeyPressed(shUp) || input.WasKeyRepeated(shUp)) {
					indexDelta = -(scrollView.Content.Layout as FlowLayout).ColumnCount(0);
				} else if (input.WasKeyPressed(shDown) || input.WasKeyRepeated(shDown)) {
					indexDelta = (scrollView.Content.Layout as FlowLayout).ColumnCount(0);
				}
				input.ConsumeKey(shRight);
				input.ConsumeKey(shLeft);
				input.ConsumeKey(shUp);
				input.ConsumeKey(shDown);
				if (indexDelta != 0) {
					if (lastKeyboardSelectedIcon == null) {
						return;
					}
					var index = scrollView.Content.Nodes.IndexOf(lastKeyboardSelectedIcon);
					if (index + indexDelta < scrollView.Content.Nodes.Count && index + indexDelta >= 0) {
						lastKeyboardSelectedIcon = scrollView.Content.Nodes[index + indexDelta] as Icon;
						selection.Clear();
						selection.Select(lastKeyboardSelectedIcon.FilesystemPath);
					}
				}
			}
		}

		private Key shRight = Key.MapShortcut(Key.Right);
		private Key shLeft = Key.MapShortcut(Key.Left);
		private Key shUp = Key.MapShortcut(Key.Up);
		private Key shDown = Key.MapShortcut(Key.Down);

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

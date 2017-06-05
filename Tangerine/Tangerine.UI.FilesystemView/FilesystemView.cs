using System;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class FilesystemView : IDocumentView
	{
		public static FilesystemView Instance;
		private Widget dockPanelWidget;
		private Widget rootWidget;
		private ScrollViewWidget scrollView;
		private readonly FilesystemToolbar toolbar;
		private readonly Model model = new Model();
		private Vector2 rectSelectionBeginPoint;
		private Vector2 rectSelectionEndPoint;
		private bool rectSelecting;
		private readonly Selection selection = new Selection();
		private Lime.FileSystemWatcher fsWatcher;
		private CookingRulesEditor crEditor;

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

		public FilesystemView(DockPanel dockPanel)
		{
			dockPanelWidget = dockPanel.ContentWidget;
			rootWidget = new Widget();
			toolbar = new FilesystemToolbar();
			scrollView = new ScrollViewWidget();
			rootWidget.AddChangeWatcher(() => model.CurrentPath, (path) => dockPanel.Title = $"Filesystem: {path}");
			crEditor = new CookingRulesEditor(NavigateAndSelect);
			InitializeWidgets();
		}

		private void NavigateAndSelect(string filename)
		{
			GoTo(Path.GetDirectoryName(filename));
			selection.Clear();
			selection.Select(filename);
		}

		void InitializeWidgets()
		{
			selection.Changed += Selection_Changed;
			scrollView.HitTestTarget = true;
			scrollView.Content.Layout = new FlowLayout { Spacing = 1.0f };
			scrollView.Padding = new Thickness(5.0f);
			scrollView.Content.CompoundPostPresenter.Insert(0, new DelegatePresenter<Widget>(RenderFilesWidgetRectSelection));
			scrollView.Updated += ScrollViewUpdated;
			scrollView.Content.Presenter = new DelegatePresenter<ScrollView.ScrollViewContentWidget>((canvas) => {
				canvas.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, canvas.Size, DesktopTheme.Colors.WhiteBackground);
			});
			rootWidget.AddChangeWatcher(() => rectSelectionEndPoint, WhenSelectionRectChanged);
			rootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => {
				if (value != null && scrollView.Content == value.Parent) {
					Window.Current.Invalidate();
				}
			});
			rootWidget.AddChangeWatcher(() => model.CurrentPath, (p) => {
				InvalidateView(p);
				InvalidateFSWatcher(p);
			});
			rootWidget.Layout = new VBoxLayout();
			rootWidget.AddNode(new HSplitter {
				Layout = new HBoxLayout(),
				Nodes = {
					(new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							toolbar,
							scrollView,
						}}),
						crEditor.RootWidget,
				}
			});
		}

		private void Selection_Changed()
		{
			crEditor.InvalidateForSelection(selection);
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
						model.GoTo(item);
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse1)) {
						iconWidget.Input.ConsumeKey(Key.Mouse1);
						SystemShellContextMenu.Instance.Show(item);
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
							selection.Clear();
							selection.Select(item);
						}
						Window.Current?.Invalidate();
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse1)) {
						// TODO: Context menu
					}
				};
				scrollView.Content.AddNode(iconWidget);
			}
		}

		private void RenderIconSelection(Icon icon)
		{
			if (selection.Contains(icon.FilesystemPath)) {
				icon.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, icon.Size, DesktopTheme.Colors.SelectedBackground.Transparentify(0.5f));
				Renderer.DrawRectOutline(Vector2.Zero, icon.Size, DesktopTheme.Colors.SelectedBackground);
			} else if (icon.IsMouseOver()) {
				icon.PrepareRendererState();
				Renderer.DrawRect(
					Vector2.Zero,
					icon.Size,
					DesktopTheme.Colors.SelectedBackground.Transparentify(0.8f));
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
					rectSelecting = false;
				}
				rectSelectionEndPoint = scrollView.Content.Input.LocalMousePosition;
				Window.Current.Invalidate();
			}
		}

		private void RenderFilesWidgetRectSelection(Widget canvas)
		{
				if (!rectSelecting) {
					return;
				}
				canvas.PrepareRendererState();
				Renderer.DrawRect(rectSelectionBeginPoint, rectSelectionEndPoint, DesktopTheme.Colors.SelectedBackground.Transparentify(0.5f));
				Renderer.DrawRectOutline(rectSelectionBeginPoint, rectSelectionEndPoint, DesktopTheme.Colors.SelectedBackground.Darken(0.2f));
		}

		public void Attach()
		{
			Instance = this;
			dockPanelWidget.PushNode(rootWidget);
			rootWidget.SetFocus();
		}

		public void Detach()
		{
			Instance = null;
			rootWidget.Unlink();
		}

		public void GoUp()
		{
			model.GoUp();
		}

		public void GoTo(string path)
		{
			model.GoTo(path);
		}

		private Node crEditorSavedParentWidget;
		private int crEditorSavedIndexInParent;

		public void ToggleCookingRules()
		{
			var crRoot = crEditor.RootWidget;
			if (crRoot.Parent != null) {
				crEditorSavedParentWidget = crRoot.Parent;
				crEditorSavedIndexInParent = crEditorSavedParentWidget.Nodes.IndexOf(crRoot);
				crRoot.Unlink();
			} else {
				crEditorSavedParentWidget.Nodes.Insert(crEditorSavedIndexInParent, crRoot);
				crEditor.InvalidateForSelection(selection);
			}
		}
	}
}

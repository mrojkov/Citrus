using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class FilesystemView : IDocumentView
	{
		public static FilesystemView Instance;
		public Widget PanelWidget;
		public Widget RootWidget;
		public ScrollViewWidget FilesWidget;
		public Toolbar Toolbar;
		Model Model = new Model();
		private Vector2 rectSelectionBeginPoint;
		private Vector2 rectSelectionEndPoint;
		private bool rectSelecting;
		private Widget CookingRulesRoot;

		public FilesystemView(Widget panelWidget)
		{
			PanelWidget = panelWidget;
			RootWidget = new Widget();
			Toolbar = new Toolbar();
			CookingRulesRoot = new Widget();
			FilesWidget = new ScrollViewWidget();
			InitializeWidgets();
		}

		void InitializeWidgets()
		{
			FilesWidget.HitTestTarget = true;
			FilesWidget.Content.Layout = new FlowLayout { Spacing = 1.0f };
			FilesWidget.Padding = new Thickness(5.0f);
			FilesWidget.CompoundPostPresenter.Insert(0, new DelegatePresenter<ScrollViewWidget>(RenderFilesWidgetRectSelection));
			FilesWidget.Updated += FilesWidget_Updated;
			RootWidget.AddChangeWatcher(() => rectSelectionEndPoint, WhenSelectionRectChanged);
			RootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => {
				if (value != null && FilesWidget.Content.Nodes.Contains(value)) {
					Window.Current.Invalidate();
				}
			});
			RootWidget.AddChangeWatcher(() => Model.CurrentPath, WhenModelCurrentPathChanged);
			RootWidget.Layout = new VBoxLayout();
			RootWidget.AddNode(Toolbar.RootWidget);
			RootWidget.AddNode(new HSplitter
			{
				Layout = new HBoxLayout(),
				Nodes =
				{
					FilesWidget,
					CookingRulesRoot
				}
			});
		}

		private void WhenModelCurrentPathChanged(string value)
		{
			FilesWidget.Content.Nodes.Clear();
			foreach (var item in Model.EnumerateItems()) {
				var iconWidget = new Icon(item);
				iconWidget.CompoundPostPresenter.Insert(0, new DelegatePresenter<Icon>(RenderIconSelection));
				iconWidget.Updated += (dt) => {
					if (!iconWidget.IsMouseOver()) return;
					var input = iconWidget.Input;
					if (input.WasKeyPressed(Key.Mouse0DoubleClick)) {
						input.ConsumeKey(Key.Mouse0DoubleClick);
						Model.GoTo(item);
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse0)) {
						input.ConsumeKey(Key.Mouse0);
						if (input.IsKeyPressed(Key.Control) && !input.IsKeyPressed(Key.Shift)) {
							input.ConsumeKey(Key.Control);
							if (Model.Selection.Contains(item)) {
								Model.Selection.Remove(item);
							} else {
								Model.Selection.Add(item);
							}
							// TODO: Ctrl + Shift, Shift clicks
						} else {
							Model.Selection.Clear();
							Model.Selection.Add(item);
						}
						Window.Current.Invalidate();
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse1)) {
						// TODO: Context menu
					}
				};
				FilesWidget.Content.AddNode(iconWidget);
			}
		}

		private void RenderIconSelection(Icon icon)
		{
			if (Model.Selection.Contains(icon.FilesystemPath)) {
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
			foreach (var n in FilesWidget.Content.Nodes) {
				var ic = n as Icon;
				var r1 = new Rectangle(ic.Position, ic.Position + ic.Size);
				if (Rectangle.Intersect(r0, r1) != Rectangle.Empty) {
					Model.Selection.Add(ic.FilesystemPath);
				} else {
					if (Model.Selection.Contains(ic.FilesystemPath)) {
						Model.Selection.Remove(ic.FilesystemPath);
					}
				}
			}
		}

		private void FilesWidget_Updated(float delta)
		{
			var input = FilesWidget.Input;
			if (FilesWidget.IsMouseOver()) {
				if (!rectSelecting && input.WasKeyPressed(Key.Mouse0)) {
					input.ConsumeKey(Key.Mouse0);
					rectSelecting = true;
					rectSelectionBeginPoint = input.LocalMousePosition;
				}
			}
			if (rectSelecting) {
				if (Window.Current.Input.WasKeyReleased(Key.Mouse0)) {
					Window.Current.Input.ConsumeKey(Key.Mouse0);
					rectSelecting = false;
				}
				rectSelectionEndPoint = input.LocalMousePosition;
				Window.Current.Invalidate();
			}
		}

		private void RenderFilesWidgetRectSelection(ScrollViewWidget canvas)
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
			PanelWidget.PushNode(RootWidget);
			RootWidget.SetFocus();
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}

		public void GoUp()
		{
			Model.GoUp();
		}

		public void GoTo(string path)
		{
			Model.GoTo(path);
		}

		public void ToggleCookingRules()
		{
			CookingRulesRoot.Visible = !CookingRulesRoot.Visible;
		}
	}
}

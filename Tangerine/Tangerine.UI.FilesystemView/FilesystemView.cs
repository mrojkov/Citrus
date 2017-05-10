using System;
using System.IO;
using System.Linq;
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
		readonly Model Model = new Model();
		private Vector2 rectSelectionBeginPoint;
		private Vector2 rectSelectionEndPoint;
		private bool rectSelecting;
		private readonly ScrollViewWidget CookingRulesRoot;
		private readonly Selection selection = new Selection();

		public FilesystemView(Widget panelWidget)
		{
			PanelWidget = panelWidget;
			RootWidget = new Widget();
			Toolbar = new Toolbar();
			CookingRulesRoot = new ScrollViewWidget();
			FilesWidget = new ScrollViewWidget();
			InitializeWidgets();
		}

		void InitializeWidgets()
		{
			selection.Changed += Selection_Changed;
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
			CookingRulesRoot.Content.Layout = new VBoxLayout();

		}

		private static Stream GenerateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static string WriteObjectToString<T>(T instance, Lime.Serialization.Format format)
		{
			using (var stream = GenerateStreamFromString("")) {
				Lime.Serialization.WriteObject<T>("", stream, instance, format);
				var reader = new StreamReader(stream);
				stream.Seek(0, SeekOrigin.Begin);
				return reader.ReadToEnd();
			}
		}

		private void Selection_Changed()
		{
			if (selection.Empty) {
				return;
			}
			CookingRulesRoot.Content.Nodes.Clear();
			var t = Orange.CookingRulesBuilder.Build(Orange.The.Workspace.AssetFiles,
				Orange.UserInterface.Instance.GetActivePlatform(), Orange.The.Workspace.ActiveTarget);

			foreach (var item in selection) {
				var key = item;
				if (key.StartsWith(Orange.Workspace.Instance.AssetsDirectory)) {
					key = key.Substring(Orange.Workspace.Instance.AssetsDirectory.Length);
					if (key.StartsWith("\\") || key.StartsWith("/")) {
						key = key.Substring(1);
					}
				}
				key = key.Replace('\\', '/');
				if (t.ContainsKey(key)) {
					CookingRulesRoot.Content.AddNode(new SimpleText
					{
						Text = WriteObjectToString(t[key].ResultingRules, Serialization.Format.JSON),
						//AutoSizeConstraints = false,
					});
				}
			}
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
			foreach (var n in FilesWidget.Content.Nodes) {
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

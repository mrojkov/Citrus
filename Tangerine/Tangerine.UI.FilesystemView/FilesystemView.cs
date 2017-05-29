using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Lime;
using Orange;
using Tangerine.Core;
using Yuzu;
using Yuzu.Metadata;

namespace Tangerine.UI.FilesystemView
{
	public class FilesystemView : IDocumentView
	{
		public static FilesystemView Instance;
		public Widget PanelWidget;
		public Widget RootWidget;
		public ScrollViewWidget FilesScrollView;
		public FilesystemToolbar FilesystemToolbar;
		public Toolbar CookingRulesToolbar;
		readonly Model Model = new Model();
		private Vector2 rectSelectionBeginPoint;
		private Vector2 rectSelectionEndPoint;
		private bool rectSelecting;
		private readonly ScrollViewWidget CookingRulesScrollView;
		private readonly Selection selection = new Selection();
		private Widget cookingRulesRoot;

		public FilesystemView(DockPanel dockPanel)
		{
			PanelWidget = dockPanel.ContentWidget;
			RootWidget = new Widget();
			FilesystemToolbar = new FilesystemToolbar();
			CookingRulesToolbar = new Toolbar();
			CookingRulesScrollView = new ScrollViewWidget();
			FilesScrollView = new ScrollViewWidget();
			RootWidget.AddChangeWatcher(() => Model.CurrentPath, (path) => dockPanel.Title = $"Filesystem: {path}");
			InitializeWidgets();
		}

		void InitializeWidgets()
		{
			DropDownList targetSelector;
			CookingRulesToolbar.Nodes.AddRange(
				(targetSelector = new DropDownList {
					LayoutCell = new LayoutCell(Alignment.Center)
				})
			);
			foreach (var t in Orange.The.Workspace.Targets) {
				targetSelector.Items.Add(new DropDownList.Item(t.Name, t));
			}
			targetSelector.Changed += (value) => {
				ActiveTarget = (Target)value.Value;
				Selection_Changed();
			};
			targetSelector.Index = 0;
			ActiveTarget = Orange.The.Workspace.Targets.First();
			selection.Changed += Selection_Changed;
			FilesScrollView.HitTestTarget = true;
			FilesScrollView.Content.Layout = new FlowLayout { Spacing = 1.0f };
			FilesScrollView.Padding = new Thickness(5.0f);
			FilesScrollView.CompoundPostPresenter.Insert(0, new DelegatePresenter<ScrollViewWidget>(RenderFilesWidgetRectSelection));
			FilesScrollView.Updated += FilesScrollViewUpdated;
			FilesScrollView.Content.Presenter = new DelegatePresenter<ScrollView.ScrollViewContentWidget>((canvas) => {
				canvas.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, canvas.Size, DesktopTheme.Colors.WhiteBackground);
			});
			RootWidget.AddChangeWatcher(() => rectSelectionEndPoint, WhenSelectionRectChanged);
			RootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => {
				if (value != null && FilesScrollView.Content.Nodes.Contains(value)) {
					Window.Current.Invalidate();
				}
			});
			RootWidget.AddChangeWatcher(() => Model.CurrentPath, WhenModelCurrentPathChanged);
			RootWidget.Layout = new VBoxLayout();
			RootWidget.AddNode(new HSplitter {
				Layout = new HBoxLayout(),
				Nodes = {
					(new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							FilesystemToolbar,
							FilesScrollView,
						}}),
					(cookingRulesRoot = new Widget {
						Layout = new VBoxLayout(),
						Nodes =
						{
							CookingRulesToolbar,
							CookingRulesScrollView
						}})
				}
			});
			CookingRulesScrollView.Content.Layout = new VBoxLayout();
		}

		public Target ActiveTarget { get; set; }

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
			CookingRulesScrollView.Content.Nodes.Clear();
			var targetDir = new System.IO.FileInfo(selection.First()).Directory.FullName;
			if (!targetDir.StartsWith(Orange.The.Workspace.AssetsDirectory)) {
				// We're somewhere outside the project directory
				return;
			}
			var t = Orange.CookingRulesBuilder.Build(new FileEnumerator(Orange.The.Workspace.AssetsDirectory, targetDir), ActiveTarget);

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
					var meta = Meta.Get(typeof(ParticularCookingRules), new CommonOptions());

					foreach (var yi in meta.Items) {
						var parent = t[key];
						Widget propertyContainer;
						Widget inheritorsContainerWrapper;
						var topContainer = new Widget
						{
							Layout = new VBoxLayout(),
							Nodes =
							{
								(propertyContainer = new Widget {
										Layout = new HBoxLayout(),
										PostPresenter = new LayoutDebugPresenter(Color4.Red, 0.5f)
									}),
								(inheritorsContainerWrapper = new Widget {
									Visible = false,
									Layout = new VBoxLayout(),
									Padding = new Thickness
									{
										Left = 30.0f
									},
									PostPresenter = new LayoutDebugPresenter(Color4.Red, 0.5f)
								})
							},
							PostPresenter = new LayoutDebugPresenter(Color4.Red, 0.5f)
						};
						CookingRulesScrollView.Content.AddNode(topContainer);
						int odd = 0;
						bool rootAdded = false;
						while (parent != null) {
							var isRoot = parent == t[key];
							var closedParent = parent;
							foreach (var kv in parent.Enumerate()) {
								odd++;
								if (isRoot && !rootAdded) {
									rootAdded = true;
									SimpleText computedValueText;
									ToolbarButton unfoldButton = null;
									Widget sectionWidget = null;
									propertyContainer.Nodes.Add((sectionWidget = new Widget
									{
										Layout = new HBoxLayout(),
										PostPresenter = new LayoutDebugPresenter(Color4.Red, 0.5f),
										Nodes = {
										(unfoldButton = new ToolbarButton(IconPool.GetTexture("Filesystem.Folded")) {
											Highlightable = false,
											Clicked = () => {
												inheritorsContainerWrapper.Visible = !inheritorsContainerWrapper.Visible;
												unfoldButton.Texture = IconPool.GetTexture(inheritorsContainerWrapper.Visible ? "Filesystem.Unfolded" : "Filesystem.Folded");
											}
										}),
										(computedValueText = new SimpleText())
									}}));
									computedValueText.AddChangeWatcher(() => yi.GetValue(closedParent.CommonRules),
										(o) => computedValueText.Text = $"{yi.Name} : {yi.GetValue(closedParent.CommonRules)}");
								}
								if (!kv.Value.FieldOverrides.Contains(yi)) {
									continue;
								}
								{
									var container = new Widget {
										Padding = new Thickness(2.0f),
										Nodes = {
											new SimpleText(string.IsNullOrEmpty(parent.SourceFilename)
												? "Default"
												: parent.SourceFilename.Substring(The.Workspace.AssetsDirectory.Length)) {
													FontHeight = 16,
													AutoSizeConstraints = false,
													OverflowMode = TextOverflowMode.Ellipsis
												}
										},
										Layout = new HBoxLayout(),
										CompoundPostPresenter = {
											//new DelegatePresenter<Widget>((canvas) => {
											//	canvas.PrepareRendererState();
											//	Renderer.DrawRectOutline(Vector2.Zero, canvas.Size, DesktopTheme.Colors.SeparatorColor, 0.5f);
											//}),
											new LayoutDebugPresenter(Color4.Red, 0.5f)
										}
									};
									inheritorsContainerWrapper.Nodes.Add(container);
									var editorParams = new PropertyEditorParams(container, parent.CommonRules, yi.Name) {
										PropertySetter = (owner, name, value) => {
											yi.SetValue(owner, value);
											closedParent.CommonRules.Override(name);
											closedParent.Save();
										},
									};
									CreatePropertyEditorForType(yi, editorParams);
								}
								//if (odd % 2 == 1) {
								//	container.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>((canvas) => {
								//		canvas.PrepareRendererState();
								//		Renderer.DrawRect(Vector2.Zero, canvas.Size, Color4.Black.Transparentify(0.5f));
								//	}));
								//}
							}
							parent = parent.Parent;
						}
					}
				}
			}
		}

		private void CreatePropertyEditorForType(Meta.Item yi, IPropertyEditorParams editorParams)
		{
			if (yi.Type == typeof(PVRFormat)) {
				new EnumPropertyEditor<PVRFormat>(editorParams);
			} else if (yi.Type == typeof(DDSFormat)) {
				new EnumPropertyEditor<DDSFormat>(editorParams);
			} else if (yi.Type == typeof(AtlasOptimization)) {
				new EnumPropertyEditor<AtlasOptimization>(editorParams);
			} else if (yi.Type == typeof(ModelCompression)) {
				new EnumPropertyEditor<ModelCompression>(editorParams);
			} else if (yi.Type == typeof(string)) {
				new StringPropertyEditor(editorParams);
			} else if (yi.Type == typeof(int)) {
				new IntPropertyEditor(editorParams);
			} else if (yi.Type == typeof(bool)) {
				new BooleanPropertyEditor(editorParams);
			} else if (yi.Type == typeof(float)) {
				new FloatPropertyEditor(editorParams);
			}
		}

		private void WhenModelCurrentPathChanged(string value)
		{
			FilesScrollView.Content.Nodes.Clear();
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
						Window.Current?.Invalidate();
					}
					if (iconWidget.Input.WasKeyPressed(Key.Mouse1)) {
						// TODO: Context menu
					}
				};
				FilesScrollView.Content.AddNode(iconWidget);
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
			foreach (var n in FilesScrollView.Content.Nodes) {
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

		private void FilesScrollViewUpdated(float delta)
		{
			var input = FilesScrollView.Input;
			if (FilesScrollView.IsMouseOver()) {
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

		private Node CookingRulesScrollViewParent;
		private int CrookingRulesScrollViewIndexInParent;
		public void ToggleCookingRules()
		{
			if (cookingRulesRoot.Parent != null) {
				CookingRulesScrollViewParent = cookingRulesRoot.Parent;
				CrookingRulesScrollViewIndexInParent = CookingRulesScrollViewParent.Nodes.IndexOf(cookingRulesRoot);
				cookingRulesRoot.Unlink();
			} else {
				CookingRulesScrollViewParent.Nodes.Insert(CrookingRulesScrollViewIndexInParent, cookingRulesRoot);
			}
		}
	}
}

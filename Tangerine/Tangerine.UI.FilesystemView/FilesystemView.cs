using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Lime;
using Orange;
using Tangerine.Core;
using Yuzu;
using Yuzu.Metadata;

using CookingRulesCollection = System.Collections.Generic.Dictionary<string, Orange.CookingRules>;

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
		private Lime.FileSystemWatcher fsWatcher;
		private double timeSinceLastFSEvent = 0.0;

		public FilesystemView(DockPanel dockPanel)
		{
			fsWatcher = new Lime.FileSystemWatcher(Model.CurrentPath) {
				IncludeSubdirectories = false
			};
			// TODO: throttle
			Action OnFsWatcherChanged = () => {
				// TODO: update selection
				WhenModelCurrentPathChanged(Model.CurrentPath);
			};
			fsWatcher.Deleted += (path) => {
				selection.Deselect(path);
				OnFsWatcherChanged();
			};
			fsWatcher.Created += (path) => {
				OnFsWatcherChanged();
			};
			fsWatcher.Renamed += (path) => {
				OnFsWatcherChanged();
			};
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

		private class CookingRulesPropertyOverrideComponent : NodeComponent
		{
			public CookingRules Rules;
			public Yuzu.Metadata.Meta.Item YuzuItem;

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

		private bool IsInAssetDir(string path)
		{
			return AssetPath.CorrectSlashes(path).StartsWith(AssetPath.CorrectSlashes(The.Workspace.AssetsDirectory));
		}

		private string NormalizePath(string path)
		{
			if (!IsInAssetDir(path)) {
				throw new ConstraintException("Normalized path must be in asset directory");
			}
			path = path.Replace('\\', '/');
			path = path.Substring(Orange.Workspace.Instance.AssetsDirectory.Length);
			if (path.StartsWith("/")) {
				path = path.Substring(1);
			}
			return path;
		}

		private CookingRules FindFirstAncestor(CookingRulesCollection cookingRules, string path)
		{
			path = NormalizePath(path);
			while (!cookingRules.ContainsKey(path)) {
				path = Path.GetDirectoryName(path);
			}
			return cookingRules[path];
		}

		private CookingRules GetAssociatedCookingRules(CookingRulesCollection cookingRules, string path, bool createIfNotExists = false)
		{
			Action<string, CookingRules> ignoreRules = (p, r) => {
				r = r.InheritClone();
				r.Ignore = true;
				cookingRules[NormalizePath(p)] = r;
			};
			path = AssetPath.CorrectSlashes(path);
			string key = NormalizePath(path);
			CookingRules cr = null;
			if (File.GetAttributes(path) == FileAttributes.Directory) {
				// Directory
				var crPath = AssetPath.Combine(path, Orange.CookingRulesBuilder.CookingRulesFilename);
				if (cookingRules.ContainsKey(key)) {
					cr = cookingRules[key];
					if (cr.SourceFilename != crPath) {
						if (createIfNotExists) {
							cr = cr.InheritClone();
							cookingRules[key] = cr;
							ignoreRules(crPath, cr);
						} else {
							return null;
						}
					}
				} else {
					throw new Lime.Exception("CookingRule record for directory should already be present in collection");
				}
				cr.SourceFilename = crPath;
			} else {
				bool isPerDirectory = Path.GetFileName(path) == CookingRulesBuilder.CookingRulesFilename;
				bool isPerFile = path.EndsWith(".txt") && File.Exists(path.Remove(path.Length - 4));
				string filename = isPerFile ? path.Remove(path.Length - 4) : path;
				if (isPerDirectory || isPerFile) {
					// Cooking Rules File itself
					if (cookingRules.ContainsKey(key)) {
						cr = cookingRules[key].Parent;
					} else {
						throw new Lime.Exception("CookingRule record for cooking rules file itself should already be present in collection");
						//cr = FindFirstAncestor(cookingRules, path).InheritClone();
						//cr.SourceFilename = path;
						//ignoreRules(path, cr);
						//if (isPerDirectory) {
						//	foreach (var kv in cookingRules) {
						//		if (kv.Key.StartsWith(Path.GetDirectoryName(path))) {
						//			cookingRules[kv.Key] = cr;
						//		}
						//	}
						//} else if (isPerFile) {
						//	cookingRules[filename] = cr;
						//}
					}
				} else {
					// Regular File
					var crPath = path + ".txt";
					var crKey = NormalizePath(crPath);
					if (cookingRules.ContainsKey(crKey)) {
						cr = cookingRules[crKey].Parent;
					} else if (!createIfNotExists) {
						return null;
					} else if (cookingRules.ContainsKey(NormalizePath(path))) {
						cr = cookingRules[NormalizePath(path)].InheritClone();
						cr.SourceFilename = crPath;
						ignoreRules(crPath, cr);
						cookingRules[key] = cr;
					} else {
						throw new Lime.Exception("CookingRule record for any regular file should already be present in collection");
					}
				}
			}
			return cr;
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
				CreateCookingRulesEditorForSelectedItem(t, item);
			}
		}

		private void CreateCookingRulesEditorForSelectedItem(CookingRulesCollection t, string item)
		{
			var key = NormalizePath(item);
			if (!t.ContainsKey(key)) {
				throw new Lime.Exception("CookingRulesCollection should already contain a record for the item");
			}
			var meta = Meta.Get(typeof(ParticularCookingRules), new CommonOptions());

			foreach (var yi in meta.Items) {
				CreateInterfaceForSingleProperty(t, item, key, yi);
			}
		}

		private void CreateInterfaceForSingleProperty(CookingRulesCollection t, string item, string key, Meta.Item yi)
		{
			var parent = t[key];
			Widget propertyContainer;
			Widget inheritorsContainerWrapper;
			CreatePropertyTopContainer(out propertyContainer, out inheritorsContainerWrapper);
			int odd = 0;
			bool rootAdded = false;
			while (parent != null) {
				var isRoot = parent == t[key];
				foreach (var kv in parent.Enumerate()) {
					odd++;
					if (isRoot && !rootAdded) {
						rootAdded = true;
						CreateInterfaceForPropertyHeader(t, item, yi, propertyContainer, inheritorsContainerWrapper, parent);
					}
					if (kv.Value.FieldOverrides.Contains(yi)) {
						CreateInterfaceForPropertyParentOverride(yi, parent, inheritorsContainerWrapper);
					}
				}
				parent = parent.Parent;
			}
		}

		private bool IsInterfaceForPropertyPresent(Meta.Item yi, CookingRules rules, Widget inheritorsContainerWrapper)
		{
			foreach (var node in inheritorsContainerWrapper.Nodes) {
				var c = node.Components.Get<CookingRulesPropertyOverrideComponent>();
				if (c.Rules == rules && c.YuzuItem == yi) {
					return true;
				}
			}
			return false;
		}

		private void CreateInterfaceForPropertyParentOverride(Meta.Item yi, CookingRules rules, Widget inheritorsContainerWrapper)
		{
			var container = new Widget
			{
				Padding = new Thickness(2.0f),
				Nodes = {
					new SimpleText(string.IsNullOrEmpty(rules.SourceFilename)
						? "Default"
						: rules.SourceFilename.Substring(The.Workspace.AssetsDirectory.Length)) {
							FontHeight = 16,
							AutoSizeConstraints = false,
							OverflowMode = TextOverflowMode.Ellipsis
						}
				},
				Layout = new HBoxLayout(),
				CompoundPostPresenter = {
					new LayoutDebugPresenter(Color4.Red, 0.5f)
				}
			};
			container.Components.Add(new CookingRulesPropertyOverrideComponent {
				Rules = rules,
				YuzuItem = yi,
			});
			inheritorsContainerWrapper.Nodes.Add(container);
			var editorParams = new PropertyEditorParams(container, rules.CommonRules, yi.Name)
			{
				PropertySetter = (owner, name, value) => {
					yi.SetValue(owner, value);
					rules.CommonRules.Override(name);
					rules.Save();
				},
			};
			CreatePropertyEditorForType(yi, editorParams);
		}

		private void CreateInterfaceForPropertyHeader(CookingRulesCollection t, string item, Meta.Item yi,
			Widget propertyContainer, Widget inheritorsContainerWrapper, CookingRules rules)
		{
			SimpleText computedValueText;
			Button addRemoveField = null;
			Widget sectionWidget = null;

			Func<bool> IsOverridedByAssociatedCookingRules = () => {
				var cr = GetAssociatedCookingRules(t, item);
				return cr != null && cr.CommonRules.FieldOverrides.Contains(yi);
			};

			propertyContainer.Nodes.Add((sectionWidget = new Widget {
				Layout = new HBoxLayout {
					IgnoreHidden = false
				},
				PostPresenter = new LayoutDebugPresenter(Color4.Red, 0.5f),
				Nodes = {
					CreateFoldButton(inheritorsContainerWrapper),
					(computedValueText = new SimpleText()),
					new Widget {
						LayoutCell = new LayoutCell {
							StretchX = 999999,
						}
					},
					(addRemoveField = new Button {
						Text = IsOverridedByAssociatedCookingRules() ? "-" : "+",
						Padding = new Thickness(2.0f),
						Clicked = () => {
							var overrided = IsOverridedByAssociatedCookingRules();
							if (overrided) {
								var cr = GetAssociatedCookingRules(t, item);
								cr.CommonRules.FieldOverrides.Remove(yi);
								cr.Save();
								if (!cr.HasOverrides()) {
									t[NormalizePath(item)] = cr.Parent;
									var acr = GetAssociatedCookingRules(t, cr.SourceFilename);
									t.Remove(NormalizePath(acr.SourceFilename));
									System.IO.File.Delete(cr.SourceFilename);
								}
								List<Node> toUnlink = new List<Node>();
								foreach (var node in inheritorsContainerWrapper.Nodes) {
									var c = node.Components.Get<CookingRulesPropertyOverrideComponent>();
									if (c.Rules == cr && c.YuzuItem == yi) {
										toUnlink.Add(node);
									}
								}
								foreach (var node in toUnlink) {
									node.Unlink();
								}
								addRemoveField.Text = "+";
							} else {
								var cr = GetAssociatedCookingRules(t, item, true);
								cr.CommonRules.Override(yi.Name);
								cr.Save();
								addRemoveField.Text = "-";
								CreateInterfaceForPropertyParentOverride(yi, cr, inheritorsContainerWrapper);
							}
							inheritorsContainerWrapper.Nodes.Sort((a, b) => {
								var ca = a.Components. Get<CookingRulesPropertyOverrideComponent>();
								var cb = b.Components. Get<CookingRulesPropertyOverrideComponent>();
								return string.Compare(ca.Rules.SourceFilename, cb.Rules.SourceFilename);
							});
						}
					})
				}
			}));
			computedValueText.AddChangeWatcher(() => yi.GetValue(rules.CommonRules),
				(o) => computedValueText.Text = $"{yi.Name} : {yi.GetValue(rules.CommonRules)}");
		}

		private Widget CreateFoldButton(Widget container)
		{
			ToolbarButton b = null;
			b = new ToolbarButton(IconPool.GetTexture("Filesystem.Folded")) {
				Highlightable = false,
				Clicked = () => {
					container.Visible = !container.Visible;
					b.Texture = IconPool.GetTexture(container.Visible ? "Filesystem.Unfolded" : "Filesystem.Folded");
				}
			};
			b.Updated += (dt) => {
				b.Visible = container.Nodes.Count != 0;
			};
			return b;
		}

		private void CreatePropertyTopContainer(out Widget propertyContainer, out Widget inheritorsContainerWrapper)
		{
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
			fsWatcher.Path = value;
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
				if (input.WasKeyPressed(Key.Mouse1)) {
					input.ConsumeKey(Key.Mouse1);
					rectSelecting = false;
					selection.Clear();
					SystemShellContextMenu.Instance.Show(Model.CurrentPath);
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

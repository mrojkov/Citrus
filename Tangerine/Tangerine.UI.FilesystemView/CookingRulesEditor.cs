using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Yuzu;
using Yuzu.Metadata;
using CookingRulesCollection = System.Collections.Generic.Dictionary<string, Orange.CookingRules>;

namespace Tangerine.UI.FilesystemView
{
	public class CookingRulesEditor
	{
		// attached to widget responsible for displaying override
		// of a certain field in a cooking rules
		private class PropertyOverrideComponent : NodeComponent
		{
			public CookingRules Rules;
			public Yuzu.Metadata.Meta.Item YuzuItem;
		}

		private Target activeTarget { get; set; }
		private Toolbar toolbar;
		public Widget RootWidget;
		private readonly ScrollViewWidget scrollView;
		private Selection savedSelection;
		private const float RowHeight = 16.0f;

		public CookingRulesEditor()
		{
			scrollView = new ScrollViewWidget();
			scrollView.Content.Layout = new VBoxLayout();
			DropDownList targetSelector;
			toolbar = new Toolbar();
			toolbar.Nodes.AddRange(
				(targetSelector = new DropDownList
				{
					LayoutCell = new LayoutCell(Alignment.Center)
				})
			);
			foreach (var t in Orange.The.Workspace.Targets) {
				targetSelector.Items.Add(new DropDownList.Item(t.Name, t));
			}
			targetSelector.Changed += (value) => {
				activeTarget = (Target)value.Value;
				InvalidateForSelection(savedSelection);
			};
			targetSelector.Index = 0;
			activeTarget = Orange.The.Workspace.Targets.First();
			RootWidget = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					toolbar,
					scrollView
				}
			};
		}

		private static bool IsInAssetDir(string path)
		{
			return AssetPath.CorrectSlashes(path).StartsWith(AssetPath.CorrectSlashes(The.Workspace.AssetsDirectory));
		}

		private static string NormalizePath(string path)
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

		private static CookingRules GetAssociatedCookingRules(CookingRulesCollection crc, string path, bool createIfNotExists = false)
		{
			Action<string, CookingRules> ignoreRules = (p, r) => {
				r = r.InheritClone();
				r.Ignore = true;
				crc[NormalizePath(p)] = r;
			};
			path = AssetPath.CorrectSlashes(path);
			string key = NormalizePath(path);
			CookingRules cr = null;
			if (File.GetAttributes(path) == FileAttributes.Directory) {
				// Directory
				var crPath = AssetPath.Combine(path, Orange.CookingRulesBuilder.CookingRulesFilename);
				if (crc.ContainsKey(key)) {
					cr = crc[key];
					if (cr.SourceFilename != crPath) {
						if (createIfNotExists) {
							cr = cr.InheritClone();
							crc[key] = cr;
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
					if (crc.ContainsKey(key)) {
						cr = crc[key].Parent;
					} else {
						throw new Lime.Exception("CookingRule record for cooking rules file itself should already be present in collection");
					}
				} else {
					// Regular File
					var crPath = path + ".txt";
					var crKey = NormalizePath(crPath);
					if (crc.ContainsKey(crKey)) {
						cr = crc[crKey].Parent;
					} else if (!createIfNotExists) {
						return null;
					} else if (crc.ContainsKey(NormalizePath(path))) {
						cr = crc[NormalizePath(path)].InheritClone();
						cr.SourceFilename = crPath;
						ignoreRules(crPath, cr);
						crc[key] = cr;
					} else {
						throw new Lime.Exception("CookingRule record for any regular file should already be present in collection");
					}
				}
			}
			return cr;
		}

		public void InvalidateForSelection(Selection selection)
		{
			savedSelection = selection;
			scrollView.Content.Nodes.Clear();
			if (selection == null || selection.Empty) {
				return;
			}
			var targetDir = new System.IO.FileInfo(selection.First()).Directory.FullName;
			if (!targetDir.StartsWith(Orange.The.Workspace.AssetsDirectory)) {
				// We're somewhere outside the project directory
				return;
			}
			var t = Orange.CookingRulesBuilder.Build(new FileEnumerator(Orange.The.Workspace.AssetsDirectory, targetDir), activeTarget);
			foreach (var path in selection) {
				CreateEditingInterfaceForPath(t, path);
			}
			scrollView.Content.Presenter = new DelegatePresenter<Widget>((w) => {
				var ico = IconPool.GetTexture("Filesystem.Zebra");
				ico.WrapModeV = ico.WrapModeU = TextureWrapMode.Repeat;
				ico.MinFilter = ico.MagFilter = TextureFilter.Nearest;
				w.PrepareRendererState();
				Renderer.DrawSprite(ico, Color4.White, Vector2.Zero, w.Size, Vector2.Zero, w.Size / (Vector2)ico.ImageSize / RowHeight);
			});
		}

		private void CreateEditingInterfaceForPath(CookingRulesCollection crc, string path)
		{
			var key = NormalizePath(path);
			if (!crc.ContainsKey(key)) {
				throw new Lime.Exception("CookingRulesCollection should already contain a record for the item");
			}
			var meta = Meta.Get(typeof(ParticularCookingRules), new CommonOptions());

			foreach (var yi in meta.Items) {
				CreateWidgetsForSingleField(crc, path, yi);
			}
		}

		private void CreateWidgetsForSingleField(CookingRulesCollection crc, string path, Meta.Item yi)
		{
			var key = NormalizePath(path);
			var parent = crc[key];
			Widget headerWidget;
			Widget overridesWidget;
			var fieldRootWidget = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(headerWidget = new Widget {
						Layout = new HBoxLayout {
							IgnoreHidden = false,
						},
						// TODO: maybe some Metrics.ScrollView.SliderWidth ? (though ScrollView is decorated in DesktopTheme which is inside Lime)
						Padding = new Thickness { Right = 10.0f },
					}),
					(overridesWidget = new Widget {
						Visible = false,
						Layout = new VBoxLayout(),
						Padding = new Thickness {
							Left = 30.0f
						},
					})
				},
			};
			scrollView.Content.AddNode(fieldRootWidget);
			bool rootAdded = false;
			while (parent != null) {
				var isRoot = parent == crc[key];
				foreach (var kv in parent.Enumerate()) {
					if (isRoot && !rootAdded) {
						rootAdded = true;
						CreateHeaderWidgets(crc, path, yi, headerWidget, overridesWidget, parent);
					}
					if (kv.Value.FieldOverrides.Contains(yi)) {
						CreateOverridesWidgets(yi, parent, overridesWidget);
					}
				}
				parent = parent.Parent;
			}
		}

		private void CreateOverridesWidgets(Meta.Item yi, CookingRules rules, Widget overridesWidget)
		{
			var container = new Widget
			{
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
			};
			container.Components.Add(new PropertyOverrideComponent
			{
				Rules = rules,
				YuzuItem = yi,
			});
			overridesWidget.Nodes.Add(container);
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

		private void CreateHeaderWidgets(CookingRulesCollection crc, string path, Meta.Item yi,
			Widget headerWidget, Widget overridesWidget, CookingRules rules)
		{
			SimpleText computedValueText;
			Button createOrDestroyOverride = null;

			Func<ITexture> btnTexture = () => IsOverridedByAssociatedCookingRules(crc, path, yi) ? IconPool.GetTexture("Filesystem.Cross") : IconPool.GetTexture("Filesystem.Plus");

			headerWidget.Nodes.AddRange(
				CreateFoldButton(overridesWidget),
				(computedValueText = new SimpleText {
					VAlignment = VAlignment.Center,
				}),
				new Widget {
					LayoutCell = new LayoutCell {
						StretchX = 999999,
					},
				},
				(createOrDestroyOverride = new ToolbarButton()
				{
					Texture = btnTexture(),
					Padding = new Thickness(2.0f),
					Clicked = () => CreateOrDestroyFieldOverride(crc, path, yi, overridesWidget, createOrDestroyOverride),
				})
			);
			createOrDestroyOverride.Padding = Thickness.Zero;
			createOrDestroyOverride.Size = createOrDestroyOverride.MinMaxSize = RowHeight * Vector2.One;
			computedValueText.AddChangeWatcher(() => yi.GetValue(rules.CommonRules),
				(o) => computedValueText.Text = $"{yi.Name} : {yi.GetValue(rules.CommonRules)}");
		}

		private bool IsOverridedByAssociatedCookingRules(CookingRulesCollection crc, string path, Meta.Item yi)
		{
			var cr = GetAssociatedCookingRules(crc, path);
			return cr != null && cr.CommonRules.FieldOverrides.Contains(yi);
		}

		private void CreateOrDestroyFieldOverride(CookingRulesCollection crc, string path, Meta.Item yi, Widget overridesWidget, Button addRemoveField)
		{
			var overrided = IsOverridedByAssociatedCookingRules(crc, path, yi);
			if (overrided) {
				var cr = GetAssociatedCookingRules(crc, path);
				cr.CommonRules.FieldOverrides.Remove(yi);
				cr.Save();
				if (!cr.HasOverrides()) {
					crc[NormalizePath(path)] = cr.Parent;
					var acr = GetAssociatedCookingRules(crc, cr.SourceFilename);
					crc.Remove(NormalizePath(acr.SourceFilename));
					System.IO.File.Delete(cr.SourceFilename);
				}
				List<Node> toUnlink = new List<Node>();
				foreach (var node in overridesWidget.Nodes) {
					var c = node.Components.Get<PropertyOverrideComponent>();
					if (c.Rules == cr && c.YuzuItem == yi) {
						toUnlink.Add(node);
					}
				}
				foreach (var node in toUnlink) {
					node.Unlink();
				}
				addRemoveField.Texture = IconPool.GetTexture("Filesystem.Plus");
			} else {
				var cr = GetAssociatedCookingRules(crc, path, true);
				cr.CommonRules.Override(yi.Name);
				cr.Save();
				addRemoveField.Texture = IconPool.GetTexture("Filesystem.Cross");
				CreateOverridesWidgets(yi, cr, overridesWidget);
			}
			overridesWidget.Nodes.Sort((a, b) => {
				var ca = a.Components.Get<PropertyOverrideComponent>();
				var cb = b.Components.Get<PropertyOverrideComponent>();
				return string.Compare(ca.Rules.SourceFilename, cb.Rules.SourceFilename);
			});
		}

		private static Widget CreateFoldButton(Widget container)
		{
			ToolbarButton b = null;
			b = new ToolbarButton(IconPool.GetTexture("Filesystem.Folded")) {
				Size = Vector2.One * RowHeight,
				MinMaxSize = Vector2.One * RowHeight,
				Padding = Thickness.Zero,
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

		private static void CreatePropertyEditorForType(Meta.Item yi, IPropertyEditorParams editorParams)
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
	}
}
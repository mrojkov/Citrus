using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Lime;
using Orange;
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

		private Target activeTarget;
		private Toolbar toolbar;
		public Widget RootWidget;
		private readonly ThemedScrollView scrollView;
		private FilesystemSelection savedFilesystemSelection;
		private const float RowHeight = 16.0f;
		private Action<string> navigateAndSelect;

		public CookingRulesEditor(Action<string> navigateAndSelect)
		{
			this.navigateAndSelect = navigateAndSelect;
			scrollView = new ThemedScrollView();
			scrollView.Content.Layout = new VBoxLayout();
			ThemedDropDownList targetSelector;
			toolbar = new Toolbar();
			toolbar.Nodes.AddRange(
				(targetSelector = new ThemedDropDownList {
					LayoutCell = new LayoutCell(Alignment.Center)
				})
			);
			foreach (var t in Orange.The.Workspace.Targets) {
				targetSelector.Items.Add(new ThemedDropDownList.Item(t.Name, t));
			}
			targetSelector.Changed += (value) => {
				if (value.ChangedByUser) {
					activeTarget = (Target)value.Value;
					Invalidate(savedFilesystemSelection);
				}
			};
			activeTarget = (Target)targetSelector.Items.First().Value;
			targetSelector.Index = 0;
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

		private bool IsCookingRulesFileItself(string path)
		{
			path = AssetPath.CorrectSlashes(path);
			if (File.GetAttributes(path) == FileAttributes.Directory) {
				return false;
			}
			bool isPerDirectory = Path.GetFileName(path) == CookingRulesBuilder.CookingRulesFilename;
			bool isPerFile = path.EndsWith(".txt") && File.Exists(path.Remove(path.Length - 4));
			return isPerDirectory || isPerFile;
		}

		private CookingRules GetAssociatedCookingRules(CookingRulesCollection crc, string path, bool createIfNotExists = false)
		{
			Action<string, CookingRules> ignoreRules = (p, r) => {
				r = r.InheritClone(activeTarget);
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
							cr = cr.InheritClone(activeTarget);
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
						cr = crc[NormalizePath(path)].InheritClone(activeTarget);
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

		Texture2D cachedZebraTexture = null;

		public void Invalidate(FilesystemSelection filesystemSelection)
		{
			savedFilesystemSelection = filesystemSelection;
			scrollView.Content.Nodes.Clear();
			if (RootWidget.Parent == null) {
				return;
			}
			if (filesystemSelection == null || filesystemSelection.Empty) {
				return;
			}
			var targetDir = new System.IO.FileInfo(filesystemSelection.First()).Directory.FullName;
			if (Orange.The.Workspace.AssetsDirectory == null || !targetDir.StartsWith(Orange.The.Workspace.AssetsDirectory)) {
				// We're somewhere outside the project directory
				return;
			}
			var t = Orange.CookingRulesBuilder.Build(new FileEnumerator(Orange.The.Workspace.AssetsDirectory, targetDir), activeTarget);
			foreach (var path in filesystemSelection) {
				CreateEditingInterfaceForPath(t, path);
			}
			scrollView.Content.Presenter = new SyncDelegatePresenter<Widget>((w) => {
				if (cachedZebraTexture == null) {
					cachedZebraTexture = new Texture2D();
					cachedZebraTexture.LoadImage(new[] { Theme.Colors.ZebraColor1, Theme.Colors.ZebraColor2 }, 1, 2);
					cachedZebraTexture.TextureParams = new TextureParams {
						WrapMode = TextureWrapMode.Repeat,
						MinMagFilter = TextureFilter.Nearest
					};
				}

				w.PrepareRendererState();
				Renderer.DrawSprite(cachedZebraTexture, Color4.White, Vector2.Zero, w.Size, Vector2.Zero, w.Size / (Vector2)cachedZebraTexture.ImageSize / RowHeight);
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
						}
					}),
				}
			};
			fieldRootWidget.AddChangeWatcher(() => WidgetContext.Current.NodeUnderMouse, (value) => {
				if (value != null && value.Parent == fieldRootWidget) {
					Window.Current?.Invalidate();
				}
			});
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
						CreateOverridesWidgets(crc, key, kv.Key, yi, parent, overridesWidget);
					}
				}
				parent = parent.Parent;
			}
		}

		private void CreateOverridesWidgets(CookingRulesCollection crc, string key, Target target, Meta.Item yi, CookingRules rules, Widget overridesWidget)
		{
			Widget innerContainer;
			var sourceFilenameText = string.IsNullOrEmpty(rules.SourceFilename)
				? "Default"
				: rules.SourceFilename.Substring(The.Workspace.AssetsDirectory.Length);
			var targetName = target == null ? "" : $" ({target.Name})";
			var container = new Widget {
				Padding = new Thickness { Right = 30 },
				Nodes = {
					(innerContainer = new Widget {
						Layout = new HBoxLayout(),
					}),
					new ThemedSimpleText(sourceFilenameText + targetName) {
						FontHeight = 16,
						ForceUncutText = false,
						OverflowMode = TextOverflowMode.Ellipsis,
						HAlignment = HAlignment.Right,
						VAlignment = VAlignment.Center,
						MinSize = new Vector2(100, RowHeight),
						MaxSize = new Vector2(500, RowHeight)
					},
					(new ToolbarButton {
						Texture = IconPool.GetTexture("Filesystem.ArrowRight"),
						Padding = Thickness.Zero,
						Size = RowHeight * Vector2.One,
						MinMaxSize = RowHeight * Vector2.One,
						Clicked = () => navigateAndSelect(rules.SourceFilename),
					})
				},
				Layout = new HBoxLayout(),
			};
			container.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>((w) => {
				var topmostOverride = crc[key];
				while (
					topmostOverride.Parent != null &&
					!(topmostOverride.CommonRules.FieldOverrides.Contains(yi) ||
					RulesForActiveTarget(topmostOverride).FieldOverrides.Contains(yi))
				) {
					topmostOverride = topmostOverride.Parent;
				}
				w.PrepareRendererState();
				if (target != activeTarget || rules != topmostOverride) {
					Renderer.DrawLine(10.0f - 30.0f, w.Height * 0.6f, w.Width - 10.0f, w.Height * 0.6f, Color4.Black.Transparentify(0.5f), 1.0f);
				} else {
					Renderer.DrawRect(Vector2.Right * -20.0f, w.Size, Color4.Green.Lighten(0.5f).Transparentify(0.5f));
				}
			}));
			container.Components.Add(new PropertyOverrideComponent {
				Rules = rules,
				YuzuItem = yi,
			});
			overridesWidget.Nodes.Add(container);
			var targetRuels = RulesForTarget(rules, target);
			var editorParams = new PropertyEditorParams(innerContainer, targetRuels, yi.Name)
			{
				ShowLabel = false,
				PropertySetter = (owner, name, value) => {
					yi.SetValue(owner, value);
					targetRuels.Override(name);
					rules.DeduceEffectiveRules(target);
					rules.Save();
				},
				NumericEditBoxFactory = () => {
					var r = new ThemedNumericEditBox();
					r.MinMaxHeight = r.Height = RowHeight;
					r.TextWidget.VAlignment = VAlignment.Center;
					r.TextWidget.Padding = new Thickness(r.TextWidget.Padding.Left, r.TextWidget.Right(), 0.0f, 0.0f);
					return r;
				},
				DropDownListFactory = () => {
					var r = new ThemedDropDownList();
					r.MinMaxHeight = r.Height = RowHeight;
					return r;
				},
				EditBoxFactory = () => {
					var r = new ThemedEditBox();
					r.MinMaxHeight = r.Height = RowHeight;
					r.TextWidget.Padding = new Thickness(r.TextWidget.Padding.Left, r.TextWidget.Right(), 0.0f, 0.0f);
					return r;
				},
			};
			CreatePropertyEditorForType(yi, editorParams);
		}

		private void CreateHeaderWidgets(CookingRulesCollection crc, string path, Meta.Item yi,
			Widget headerWidget, Widget overridesWidget, CookingRules rules)
		{
			SimpleText computedValueText;
			Button createOrDestroyOverride = null;
			headerWidget.HitTestTarget = true;
			headerWidget.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>((widget) => {
				if (widget.IsMouseOver()) {
					widget.PrepareRendererState();
					Renderer.DrawRect(
						Vector2.Zero,
						widget.Size,
						Theme.Colors.SelectedBackground.Transparentify(0.8f));
				}
			}));
			Func<ITexture> btnTexture = () => IsOverridedByAssociatedCookingRules(crc, path, yi) ? IconPool.GetTexture("Filesystem.Cross") : IconPool.GetTexture("Filesystem.Plus");
			Widget foldButton;
			headerWidget.Nodes.AddRange(
				(foldButton = CreateFoldButton(overridesWidget)),
				(new ThemedSimpleText {
					ForceUncutText = false,
					VAlignment = VAlignment.Center,
					HAlignment = HAlignment.Left,
					OverflowMode = TextOverflowMode.Ellipsis,
					LayoutCell = new LayoutCell { StretchX = 1 },
					Size = new Vector2(150, RowHeight),
					MinSize = new Vector2(100, RowHeight),
					MaxSize = new Vector2(200, RowHeight),
					Text = yi.Name,
				}),
				(computedValueText = new ThemedSimpleText {
					LayoutCell = new LayoutCell { StretchX = 3 },
					ForceUncutText = false,
					HAlignment = HAlignment.Left,
					Size = new Vector2(150, RowHeight),
					MinSize = new Vector2(50, RowHeight),
					MaxSize = new Vector2(300, RowHeight),
				}),
				(createOrDestroyOverride = new ToolbarButton {
					Texture = btnTexture(),
					Clicked = () => CreateOrDestroyFieldOverride(crc, path, yi, overridesWidget, createOrDestroyOverride),
				})
			);
			headerWidget.Clicked = foldButton.Clicked;
			createOrDestroyOverride.Padding = Thickness.Zero;
			createOrDestroyOverride.Size = createOrDestroyOverride.MinMaxSize = RowHeight * Vector2.One;
			if (IsCookingRulesFileItself(path)) {
				rules = GetAssociatedCookingRules(crc, path);
			}
			computedValueText.AddChangeWatcher(() => yi.GetValue(rules.EffectiveRules),
				(o) => computedValueText.Text = rules.FieldValueToString(yi, yi.GetValue(rules.EffectiveRules)));
		}

		private static ParticularCookingRules RulesForTarget(CookingRules CookingRules, Target target)
		{
			return target == null ? CookingRules.CommonRules : CookingRules.TargetRules[target];
		}

		private ParticularCookingRules RulesForActiveTarget(CookingRules CookingRules)
		{
			return RulesForTarget(CookingRules, activeTarget);
		}

		private bool IsOverridedByAssociatedCookingRules(CookingRulesCollection crc, string path, Meta.Item yi)
		{
			var cr = GetAssociatedCookingRules(crc, path);
			return cr != null && RulesForActiveTarget(cr).FieldOverrides.Contains(yi);
		}

		private void CreateOrDestroyFieldOverride(CookingRulesCollection crc, string path, Meta.Item yi, Widget overridesWidget, Button addRemoveField)
		{
			var overrided = IsOverridedByAssociatedCookingRules(crc, path, yi);
			var key = NormalizePath(path);
			if (overrided) {
				var cr = GetAssociatedCookingRules(crc, path);
				var targetRules = RulesForActiveTarget(cr);
				targetRules.FieldOverrides.Remove(yi);
				cr.Save();
				if (!cr.HasOverrides()) {
					var acr = GetAssociatedCookingRules(crc, cr.SourceFilename);
					if (!acr.SourceFilename.EndsWith(key)) {
						crc[key] = cr.Parent;
					}
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
				var targetRules = RulesForActiveTarget(cr);
				targetRules.Override(yi.Name);
				cr.Save();
				addRemoveField.Texture = IconPool.GetTexture("Filesystem.Cross");
				CreateOverridesWidgets(crc, key, activeTarget, yi, cr, overridesWidget);
			}
		}

		private static Widget CreateFoldButton(Widget container)
		{
			var b = new ThemedExpandButton {
				Size = Vector2.One * RowHeight,
				MinMaxSize = Vector2.One * RowHeight,
				Padding = Thickness.Zero,
				Highlightable = false,
			};
			b.Clicked = () => {
				container.Visible = !container.Visible;
			};
			b.Updated += (dt) => {
				b.Visible = container.Nodes.Count != 0;
			};
			return b;
		}

		private static void CreatePropertyEditorForType(Meta.Item yi, IPropertyEditorParams editorParams)
		{
			if (yi.Type.IsEnum) {
				Activator.CreateInstance(typeof(EnumPropertyEditor<>).MakeGenericType(yi.Type), editorParams);
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

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.Docking;
using Tangerine.UI.Inspector;

namespace Tangerine
{
	internal static class Presenters
	{
		public static IPresenter StripePresenter = new SyncDelegatePresenter<Widget>(
			w => {
				if (w.Parent != null) {
					var i = w.Parent.AsWidget.Nodes.IndexOf(w);
					w.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, w.Size,
						i % 2 == 0 ? ColorTheme.Current.Inspector.StripeBackground2 : ColorTheme.Current.Inspector.StripeBackground1);
				}
			});

		public static IPresenter HeaderPresenter = new WidgetFlatFillPresenter(
			ColorTheme.Current.Inspector.StripeBackground1.Darken(0.1f)) {
			IgnorePadding = true
		};

		public static IPresenter WidgetBoundsPresenter = new WidgetBoundsPresenter(Theme.Colors.ControlBorder) { IgnorePadding = true };
	}

	public static class AttachmentMetrics
	{
		public const float ControlWidth = 100;
		public const float EditorWidth = 150;
		public const float RowHeight = 15;
		public const float Spacing = 5;
		public const float ExpandButtonSize = 20;
		public const float ExpandContentPadding = ExpandButtonSize + Spacing;
	}

	public class AttachmentPanel : IDocumentView
	{
		public static AttachmentPanel Instance;
		public readonly Panel Panel;
		public readonly Widget RootWidget;

		private class AttachmentDocument
		{
			public Model3DAttachment Attachment;
			public Model3DAttachmentMeta Meta;
			public DateTime LastMetaWriteTime = DateTime.MinValue;
			public DateTime LastAttachmentWriteTime = DateTime.MinValue;
			public DocumentHistory History = new DocumentHistory();
		}

		private static string model3DContentsPath;
		private static DocumentHistory history => model3DContentsPath != null ? documents[model3DContentsPath].History : null;

		private static Dictionary<string, AttachmentDocument> documents = new Dictionary<string, AttachmentDocument>();
		private readonly ObservableCollection<IMaterial> sourceMaterials = new ObservableCollection<IMaterial>();
		private readonly ObservableCollection<string> sourceAnimationIds = new ObservableCollection<string>();
		private readonly ObservableCollection<Model3DAttachment.MeshOption> meshOptions = new ObservableCollection<Model3DAttachment.MeshOption>();

		private class PanelState
		{
			public static int ActiveTabIndex = 3;
			public float AnimationsScrollPosition = -1;
			public float MaterialsScrollPosition = -1;
		}

		private PanelState panelState = new PanelState();

		public AttachmentPanel(Panel panel)
		{
			RootWidget = new Widget {
				HitTestTarget = true,
				Layout = new VBoxLayout(),
			};
			RootWidget.FocusScope = new KeyboardFocusScope(RootWidget);
			Panel = panel;
			RootWidget.Tasks.AddLoop(() => {
				if (Widget.Focused != RootWidget || history == null) {
					return;
				}
				if (!Command.Undo.IsConsumed()) {
					Command.Undo.Enabled = history.CanUndo();
					if (Command.Undo.WasIssued()) {
						history.Undo();
					}
					Command.Undo.Consume();
				}
				if (!Command.Redo.IsConsumed()) {
					Command.Redo.Enabled = history.CanRedo();
					if (Command.Redo.WasIssued()) {
						history.Redo();
					}
					Command.Redo.Consume();
				}
			});
			RootWidget.AddChangeWatcher(() => model3DContentsPath, path => RefreshPanelTitle());
			RootWidget.AddChangeWatcher(() => history?.IsDocumentModified ?? null, _ => RefreshPanelTitle());
			RootWidget.AddChangeWatcher(CalcSelectedRowsHashcode, _ => {
				// rebuild
				model3DContentsPath = null;
				foreach (var node in RootWidget.Nodes) {
					node.UnlinkAndDispose();
				}
				var rows = Document.Current.Rows;
				Model3D model3d = null;
				foreach (var r in rows) {
					if (r.Selected) {
						var nr = r.Components.Get<Core.Components.NodeRow>();
						if (nr != null && nr.Node is Model3D m3d) {
							if (model3d != null) {
								return;
							}
							model3d = m3d;
						}
					}
				}
				if (model3d != null && !string.IsNullOrEmpty(model3d.ContentsPath)) {
					RootWidget.PushNode(Rebuild(model3d));
				}
			});
		}

		private static void BuildList<TData, TRow>(ObservableCollection<TData> source, Widget container, string title, Func<TData> activator, Widget header) where TRow : DeletableRow<TData>
		{
			ThemedExpandButton markersExpandButton;
			container.AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
				Nodes = {
					(markersExpandButton = new ThemedExpandButton {
						Id = title + "ExpandButton",
						MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					}),
					new ThemedSimpleText { Text = title },
				}
			});
			var list = new Widget {
				Layout = new VBoxLayout(),
				Padding = new Thickness {
					Left = AttachmentMetrics.ExpandContentPadding,
					Top = AttachmentMetrics.Spacing
				},
			};
			list.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var widgetFactory = new AttachmentWidgetFactory<TData>(
				w => (TRow)Activator.CreateInstance(typeof(TRow), new object[] { w, source }), source);
				widgetFactory.AddHeader(header);
				widgetFactory.AddFooter(DeletableRow<TData>.CreateFooter(() => {
					history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(source, source.Count, activator()));
				}));
				list.Components.Add(widgetFactory);
			}));
			container.AddNode(list);
			container.AddChangeWatcher(() => markersExpandButton.Expanded, (e) => list.Visible = e);
		}

		private void RefreshPanelTitle()
		{
			if (model3DContentsPath != null) {
				Panel.Title = $"Model3D Attachment : {model3DContentsPath} {(history.IsDocumentModified ? "(*)" : "")}";
			} else {
				Panel.Title = $"Model3D Attachment";
			}
		}

		private static int CalcSelectedRowsHashcode()
		{
			var r = 0;
			if (Document.Current.InspectRootNode) {
				var rootNode = Document.Current.RootNode;
				r ^= rootNode.GetHashCode();
				foreach (var component in rootNode.Components) {
					r ^= component.GetHashCode();
				}
			} else {
				foreach (var row in Document.Current.Rows) {
					if (row.Selected) {
						r ^= row.GetHashCode();
						var node = row.Components.Get<Core.Components.NodeRow>()?.Node;
						if (node != null) {
							foreach (var component in node.Components) {
								r ^= component.GetHashCode();
							}
						}
					}
				}
			}
			return r;
		}

		private static IPropertyEditorParams Decorate(PropertyEditorParams @params, bool displayLabel = false)
		{
			@params.ShowLabel = displayLabel;
			@params.History = history;
			@params.PropertySetter = SetProperty;
			@params.NumericEditBoxFactory = () => new InspectorContent.TransactionalNumericEditBox(history);
			return @params;
		}

		public Widget Rebuild(Model3D source)
		{
			model3DContentsPath = source.ContentsPath;
			var shouldInvalidateMeta = false;
			var shouldInvalidateAttachment = false;
			var lastMetaWriteTime = DateTime.MinValue;
			var lastAttachmentWriteTime = DateTime.MinValue;
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				lastMetaWriteTime =
					cacheBundle.GetFileLastWriteTime(Path.ChangeExtension(model3DContentsPath,
						Model3DAttachmentMeta.FileExtension));
			}

			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				var path = Path.ChangeExtension(model3DContentsPath, Model3DAttachment.FileExtension);
				if (cacheBundle.FileExists(path)) {
					lastAttachmentWriteTime = cacheBundle.GetFileLastWriteTime(path);
				} else {
					lastAttachmentWriteTime = DateTime.Now;
				}
			}

			if (!documents.TryGetValue(model3DContentsPath, out var doc)) {
				documents.Add(model3DContentsPath, doc = new AttachmentDocument());
				doc.LastMetaWriteTime = lastMetaWriteTime;
				doc.LastAttachmentWriteTime = lastAttachmentWriteTime;
				shouldInvalidateMeta = true;
				shouldInvalidateAttachment = true;
			} else {
				if (lastMetaWriteTime > doc.LastMetaWriteTime) {
					shouldInvalidateMeta = true;
					documents[model3DContentsPath].LastMetaWriteTime = lastMetaWriteTime;
				}
				if (lastAttachmentWriteTime > doc.LastAttachmentWriteTime) {
					shouldInvalidateAttachment = true;
					documents[model3DContentsPath].LastAttachmentWriteTime = lastAttachmentWriteTime;
				}
			}

			Model3DAttachment attachment;
			Model3DAttachmentMeta meta;
			if (shouldInvalidateAttachment) {
				doc.Attachment = attachment = ReadAttachment(source);
			} else {
				attachment = doc.Attachment;
			}

			if (shouldInvalidateMeta) {
				doc.Meta = meta = ReadMeta(source);
			} else {
				meta = doc.Meta;
			}

			SyncSources(meta, attachment);
			var isFirstInvoke = true;
			var content = new ThemedTabbedWidget();
			content.AddTab("General", CreateGeneralPane(attachment), true);
			content.AddTab("Materials Remap", CreateMaterialsRemapPane(attachment));
			content.AddTab("Mesh Options", CreateMeshOptionsPane());
			var animationsPane = CreateAnimationsPane(attachment);
			content.AddTab("Animations", animationsPane);
			content.AddTab("Node Removals", CreateNodeRemovalsPane(attachment));
			content.AddTab("Components", CreateComponentsPane(attachment));
			if (PanelState.ActiveTabIndex != -1) {
				content.ActivateTab(PanelState.ActiveTabIndex);
			}
			content.AddChangeLateWatcher(() => content.ActiveTabIndex, activeTabIndex => {
				PanelState.ActiveTabIndex = activeTabIndex;
				if (PanelState.ActiveTabIndex == 3) {
					var t = animationsPane["Container"];
					foreach (var node in t.Nodes) {
						if (node is AnimationRow ar) {
							(ar["AnimationRowExpandedButton"] as ThemedExpandButton).Expanded = true;
							ar.Expand();
							ar.LateTasks.Add(ExpandMarkers(ar));
						}
					}
					if (panelState.AnimationsScrollPosition != 1) {
						UpdateScrollPosition(animationsPane, panelState.AnimationsScrollPosition);
					}
				}
			});
			Button okButton;
			Widget rootWidget = new Widget {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					content,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Padding = new Thickness { Top = 5 },
						Nodes = {
							(okButton = new ThemedButton { Text = "Apply" }),
						}
					}
				}
			};
			okButton.Clicked += () => {
				try {
					CheckErrors(attachment, source);
					// Since attachment dialog not present as modal window, document can be rolled back with "undo"
					// operation to the state when source isn't presented or source content path isn't set.
					// So check it out before saving.
					if (source.DescendantOf(Document.Current.RootNode) && source.ContentsPath != null) {
						attachment.MeshOptions.Clear();
						foreach (var meshOption in meshOptions) {
							if (meshOption.Opaque == default &&
								meshOption.HitTestTarget == default &&
								meshOption.CullMode == CullMode.Front &&
								meshOption.DisableMerging == default
							) {
								continue;
							}
							attachment.MeshOptions.Add(meshOption);
						}
						SaveAttachment(attachment, source.ContentsPath);
						history.AddSavePoint();
					}
				} catch (Lime.Exception e) {
					new AlertDialog(e.Message).Show();
				}
			};
			return rootWidget;
		}

		private IEnumerator<object> ExpandMarkers(AnimationRow ar)
		{
			(ar["MarkersExpandButton"] as ThemedExpandButton).Expanded = true;
			yield return null;
		}

		private void SyncSources(Model3DAttachmentMeta meta, Model3DAttachment attachment)
		{
			sourceAnimationIds.Clear();
			sourceMaterials.Clear();
			meshOptions.Clear();
			foreach (var animationId in meta.SourceAnimationIds) {
				sourceAnimationIds.Add(animationId);
			}
			foreach (var material in meta.SourceMaterials) {
				sourceMaterials.Add(material);
			}
			var dict = new Dictionary<string, Model3DAttachment.MeshOption>();
			foreach (var option in attachment.MeshOptions) {
				dict[option.Id] = option;
			}
			foreach (var meshId in meta.MeshIds) {
				meshOptions.Add(dict.ContainsKey(meshId) ? dict[meshId] : new Model3DAttachment.MeshOption { Id = meshId, CullMode = CullMode.Front });
			}
			var idx = 0;
			while (idx < attachment.MeshOptions.Count) {
				if (!meta.MeshIds.Contains(attachment.MeshOptions[idx].Id)) {
					attachment.MeshOptions.Remove(attachment.MeshOptions[idx]);
				}
				idx++;
			}
		}

		public void UpdateScrollPosition(ThemedScrollView sv, float pos)
		{
			sv.Update(0.0f);
			sv.LayoutManager.Layout();
			sv.ScrollPosition = Mathf.Clamp(pos, 0, sv.MaxScrollPosition);
		}

		public class MaterialRow : DeletableRow<Model3DAttachment.MaterialRemap>
		{
			private readonly InstancePropertyEditor<IMaterial> editor;

			public MaterialRow(Model3DAttachment.MaterialRemap material, ObservableCollection<Model3DAttachment.MaterialRemap> materials) : base(material, materials)
			{
				Nodes.Clear();
				deleteButton.Unlink();
				Layout = new VBoxLayout();
				var ic = new InspectorContent(this) {
					History = history
				};
				ic.BuildForObjects(new[] { material });
				HeaderContainer = Nodes.First().AsWidget;
				// Change displayed property name to material name.
				editor = ic.ReadonlyEditors.OfType<InstancePropertyEditor<IMaterial>>().First();
				editor.EditorParams.DisplayName = material.SourceName;
				// Remove redundant node.
				HeaderContainer.Nodes.Last().UnlinkAndDispose();
				HeaderContainer.Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing };
				HeaderContainer.Presenter = DefaultPresenter.Instance;
				HeaderContainer.AddNode(deleteButton);
				Padding = Thickness.Zero;
			}

			public void Expand() => editor.Expanded = true;

			public static Widget CreateFooter(Action action)
			{
				return new Widget {
					Padding = new Thickness(AttachmentMetrics.Spacing),
					Anchors = Anchors.LeftRight,
					Layout = new HBoxLayout(),
					Nodes = {
						new ThemedAddButton {
							Clicked = action,
						},
						new ThemedSimpleText {
							Text = "Remap Material",
						}
					}
				};
			}
		}

		private ThemedScrollView CreateMaterialsRemapPane(Model3DAttachment attachment)
		{
			var container = CreateContentWrapper();
			container.Content.Id = "MaterialsRemapScrollView";
			container.Content.Padding = new Thickness(10, AttachmentMetrics.Spacing);
			container.Content.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var content = new Widget {
					Layout = new VBoxLayout(),
				};
				var remappedMaterialsFactory = new AttachmentWidgetFactory<Model3DAttachment.MaterialRemap>(w => {
					var materialRow = new MaterialRow(w, attachment.Materials);
					materialRow.WarningText = "Source material not found";
					materialRow.Tasks.Add(ValidateRowTask(materialRow, () => sourceMaterials.Any(m => m.Id == w.SourceName)));
					return materialRow;
				}, attachment.Materials);
				var remappedMaterialsFooter = MaterialRow.CreateFooter(() => {
					var menu = new Menu();
					foreach (var material in GetValidForRemapMaterials(attachment.Materials, sourceMaterials)) {
						ICommand command = new Command(material.Id, () => {
							history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
								attachment.Materials,
								attachment.Materials.Count,
								new Model3DAttachment.MaterialRemap {
									Material = material.Clone(),
									SourceName = material.Id,
							}));
						});
						menu.Add(command);
					}
					menu.Popup();
				});
				remappedMaterialsFactory.AddFooter(remappedMaterialsFooter);
				content.Components.Add(remappedMaterialsFactory);
				container.Content.AddNode(content);
				var list = new Widget {
					Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness(10f, 5f),
					Visible = false
				};
				list.Components.Add(new CreateContentOnVisibleBehaviour(() => {
					var widgetFactory = new AttachmentWidgetFactory<IMaterial>(
					w => new SourceMaterialRow(w), sourceMaterials);
					list.Components.Add(widgetFactory);
					list.CompoundPostPresenter.Add(Presenters.WidgetBoundsPresenter);
				}));
				var button = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					LayoutCell = new LayoutCell(Alignment.LeftCenter),
				};
				var sourceMaterialNameWidget = new ThemedSimpleText("Source materials") {
					VAlignment = VAlignment.Center,
					ForceUncutText = false,
				};
				var header = new Widget {

					Layout = new HBoxLayout { Spacing =  AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = 4f, Right = 10f, Top = 15f },
					Nodes = { button, sourceMaterialNameWidget }
				};
				list.AddChangeWatcher(() => button.Expanded, v => list.Visible = v);
				container.Content.AddNode(header);
				container.Content.AddNode(list);
				container.Content.AddChangeWatcher(() => container.ScrollPosition, sp => panelState.MaterialsScrollPosition = sp);
			}));
			return container;
		}

		private IEnumerator<object> ValidateRowTask<T>(DeletableRow<T> materialRow, Func<bool> validator)
		{
			while (true) {
				materialRow.State = !validator() ? RowState.Warning : RowState.Default;
				yield return null;
			}
		}

		private IEnumerable<IMaterial> GetValidForRemapMaterials(ICollection<Model3DAttachment.MaterialRemap> attachmentMaterials, ICollection<IMaterial> sourceMaterials)
		{
			return sourceMaterials.Where(i => attachmentMaterials.All(m => m.SourceName != i.Id));
		}

		public class SourceMaterialRow : Widget
		{
			public SourceMaterialRow(IMaterial source)
			{
				var button = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					LayoutCell = new LayoutCell(Alignment.LeftCenter),
				};
				var sourceMaterialNameWidget = new ThemedSimpleText(source.Id) {
					LayoutCell = new LayoutCell(Alignment.LeftCenter) { StretchX = 1 },
				};
				var header = new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Nodes = { button, sourceMaterialNameWidget, new Widget() },
				};
				Layout = new VBoxLayout { Spacing = 5f };
				AddNode(header);
				var wrapper = new Widget {
					Layout = new VBoxLayout(),
					Padding = new Thickness { Left = 4f, Top = 5f, Bottom = 5f, Right = 5f },
					Visible = false
				};
				AddNode(wrapper);
				wrapper.Components.Add(new CreateContentOnVisibleBehaviour(() => {
					wrapper.CompoundPostPresenter.Add(Presenters.WidgetBoundsPresenter);
					var ic = new InspectorContent(wrapper) {
						History = history,
						Enabled = false
					};
					ic.BuildForObjects(new[] { source });
				}));
				wrapper.AddChangeWatcher(() => button.Expanded, v => wrapper.Visible = v);
				Padding = new Thickness(0f, 5f);
				Presenter = Presenters.StripePresenter;
			}
		}

		private static Model3DAttachmentMeta ReadMeta(Model3D source)
		{
			var metaPath = source.ContentsPath + Model3DAttachmentMeta.FileExtension;
			Model3DAttachmentMeta meta = null;
			bool metaCached;
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				metaCached = cacheBundle.FileExists(metaPath);
				if (metaCached) {
					using (var assetStream = cacheBundle.OpenFile(metaPath)) {
						meta = TangerineYuzu.Instance.Value.ReadObject<Model3DAttachmentMeta>(metaPath, assetStream);
					}
				}
			}
			if (!metaCached) {
				meta = new Model3DAttachmentMeta();
				foreach (var a in source.Animations) {
					meta.SourceAnimationIds.Add(a.Id);
				}
				var submeshes = source.Descendants.OfType<Mesh3D>().SelectMany(sm => sm.Submeshes);
				foreach (var submesh3D in submeshes) {
					if (submesh3D.Material != null) {
						meta.SourceMaterials.Add(submesh3D.Material.Clone());
					}
				}
			}
			return meta;
		}

		private static Model3DAttachment ReadAttachment(Model3D source)
		{
			var path = source.ContentsPath + Model3DAttachment.FileExtension;
			Model3DAttachment attachment = null;
			bool attachmentCached;
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				attachmentCached = cacheBundle.FileExists(path);
				if (attachmentCached) {
					using (var assetStream = cacheBundle.OpenFile(path)){
						var attachmentFormat = TangerineYuzu.Instance.Value.ReadObject<Model3DAttachmentParser.ModelAttachmentFormat>(path, assetStream);
						attachment = Model3DAttachmentParser.GetModel3DAttachment(attachmentFormat, path);
					}
				}
			}
			if (!attachmentCached) {
				attachment = new Model3DAttachment { ScaleFactor = 1 };
				foreach (var a in source.Animations) {
					attachment.Animations.Add(new Model3DAttachment.Animation {
						Id = a.Id,
						SourceAnimationId = a.Id,
						StartFrame = 0,
						LastFrame = -1
					});
				}
			}
			return attachment;
		}

		private static void SaveAttachment(Model3DAttachment attachment, string contentPath)
		{
			Model3DAttachmentParser.Save(attachment, Path.Combine(Project.Current.AssetsDirectory, contentPath));
		}

		private static ThemedScrollView CreateComponentsPane(Model3DAttachment attachment)
		{
			var container = CreateContentWrapper();
			container.Content.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var list = new Widget {
					Layout = new VBoxLayout(),
				};
				container.Content.AddNode(list);
				var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.NodeComponentCollection>(
					w => new NodeComponentCollectionRow(w, attachment.NodeComponents), attachment.NodeComponents);
				widgetFactory.AddHeader(NodeComponentCollectionRow.CreateHeader());
				widgetFactory.AddFooter(DeletableRow<Model3DAttachment.NodeComponentCollection>.CreateFooter(() => {

					void CreateNodeComponent(bool isRoot = false)
					{
						history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
							attachment.NodeComponents,
							attachment.NodeComponents.Count,
							new Model3DAttachment.NodeComponentCollection { NodeId = isRoot ? "" : "Node id", Components = null, IsRoot = isRoot }));
					}

					if (!attachment.NodeComponents.Any(c => c.IsRoot)) {
						var menu = new Menu();
						menu.Add(new Command("Add For Node", () => CreateNodeComponent()));
						menu.Add(new Command("Add For Root", () => CreateNodeComponent(true)));
						menu.Popup();
					} else {
						CreateNodeComponent();
					}
				}));
				list.Components.Add(widgetFactory);
			}));
			return container;
		}

		private static void CheckErrors(Model3DAttachment attachment, Model3D source)
		{
			if (new HashSet<string>(attachment.Animations.Select(a => a.Id)).Count != attachment.Animations.Count) {
				throw new Lime.Exception("Animations shouldn't have the same names");
			}

			var errorAnim = attachment.Animations.FirstOrDefault(a =>
				new HashSet<string>(a.Markers.Select(m => m.Marker.Id)).Count() != a.Markers.Count);
			if (errorAnim != null) {
				throw new Lime.Exception($"Markers in '{ errorAnim.Id }' animation shouldn't have the same ids");
			}

			if (new HashSet<string>(attachment.MeshOptions.Select(a => a.Id)).Count != attachment.MeshOptions.Count) {
				throw new Lime.Exception("Mesh options shouldn't have the same node ids");
			}

			if (new HashSet<string>(attachment.NodeComponents.Select(a => a.NodeId)).Count != attachment.NodeComponents.Count) {
				throw new Lime.Exception("Node components shouldn't have the same node ids");
			}

			var mat = attachment.Materials.FirstOrDefault(m => m.Material == null);
			if (mat != null) {
				throw new Lime.Exception($"No material instance is set for the \'{ mat.SourceName }\' material remap");
			}
		}

		private static ThemedScrollView CreateNodeRemovalsPane(Model3DAttachment attachment)
		{
			var pane = CreateContentWrapper();
			pane.Content.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var list = new Widget {
					Layout = new VBoxLayout(),
				};
				pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
				pane.Content.AddNode(list);
				var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.NodeRemoval>(
					w => new NodeRemovalRow(w, attachment.NodeRemovals), attachment.NodeRemovals);
				widgetFactory.AddHeader(NodeRemovalRow.CreateHeader());
				widgetFactory.AddFooter(NodeRemovalRow.CreateFooter(() => {
					history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
						attachment.NodeRemovals,
						attachment.NodeRemovals.Count,
						new Model3DAttachment.NodeRemoval { NodeId = "NodeRemoval" }
					));
				}));
				list.Components.Add(widgetFactory);
			}));
			return pane;
		}

		private ThemedScrollView CreateAnimationsPane(Model3DAttachment attachment)
		{
			var container = CreateContentWrapper();
			container.Id = "AnimationsScrollView";
			container.Content.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var list = new Widget {
					Layout = new VBoxLayout(),
				};
				container.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
				container.Content.AddNode(CreateEntryAnimationEditor(attachment));
				container.Content.AddNode(list);
				var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.Animation>(
					w => {
						var animationRow = new AnimationRow(w, attachment, sourceAnimationIds);
						animationRow.WarningText = "Source animation not found";
						animationRow.Tasks.Add(ValidateRowTask(animationRow, () => {
							return sourceAnimationIds.Any(a => a == w.SourceAnimationId);
						}));
						return animationRow;
					}, attachment.Animations);
				widgetFactory.AddHeader(AnimationRow.CreateHeader());
				widgetFactory.AddFooter(AnimationRow.CreateFooter(() => {
					history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
						attachment.Animations,
						attachment.Animations.Count,
						new Model3DAttachment.Animation { Id = "Animation", }
					));
				}));
				list.Components.Add(widgetFactory);
				container.AddChangeWatcher(() => container.ScrollPosition, sp => panelState.AnimationsScrollPosition = sp);
			}));
			return container;
		}

		private ThemedScrollView CreateMeshOptionsPane()
		{
			var container = CreateContentWrapper();
			container.Content.Components.Add(new CreateContentOnVisibleBehaviour(() => {
				var list = new Widget {
					Layout = new VBoxLayout(),
				};
				container.Content.AddNode(list);
				var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.MeshOption>(w => new MeshRow(w), meshOptions);
				widgetFactory.AddHeader(MeshRow.CreateHeader());
				list.Components.Add(widgetFactory);
			}));
			return container;
		}

		private static ThemedScrollView CreateGeneralPane(Model3DAttachment attachment)
		{
			var container = CreateContentWrapper();
			container.Content.Padding = new Thickness(10, AttachmentMetrics.Spacing);
			new FloatPropertyEditor(Decorate(new PropertyEditorParams(
					container.Content,
					attachment,
					nameof(Model3DAttachment.ScaleFactor),
					nameof(Model3DAttachment.ScaleFactor)), displayLabel: true));
			return container;
		}

		private static ThemedScrollView CreateContentWrapper()
		{
			var sv = new ThemedScrollView();
			sv.Content.Padding = new Thickness { Right = 10 };
			sv.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			return sv;
		}

		private static void SetProperty(object obj, string propertyname, object value)
		{
			Core.Operations.SetProperty.Perform(obj, propertyname, value);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}

		public void Attach()
		{
			Instance = this;
			Panel.ContentWidget.PushNode(RootWidget);
		}

		public enum RowState
		{
			Default,
			Warning
		}

		public class CommonRow<T> : Widget
		{
			protected T Source { get; }
			protected Widget Header { get; }

			protected CommonRow(T source)
			{
				Source = source;
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header = new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					LayoutCell = new LayoutCell(),
				};
				Nodes.Add(Header);
				MinMaxHeight = AttachmentMetrics.RowHeight;
				Presenter = Presenters.StripePresenter;
			}
		}

		public class DeletableRow<T> : Widget
		{
			protected T Source { get; }
			public string WarningText;
			protected Widget Header { get; set; }
			protected Widget HeaderContainer { get; set; }
			private IList<T> SourceCollection { get; }
			protected readonly ThemedDeleteButton deleteButton;
			private readonly Image warningIcon;
			private readonly IPresenter warningPresenter;

			private RowState state;
			public RowState State
			{
				get => state;
				set {
					if (state != value) {
						state = value;
						OnStateChanged();
					}
				}
			}

			protected DeletableRow(T source, ObservableCollection<T> sourceCollection)
			{
				Source = source;
				SourceCollection = sourceCollection;
				warningPresenter = new WidgetFlatFillPresenter(Color4.Red.Transparentify(0.8f)) { IgnorePadding = true };
				warningIcon = new Image {
					Texture = IconPool.GetTexture("Timeline.NoEntry"),
					MinMaxSize = Vector2.One * 16,
					HitTestTarget = true,
					LayoutCell = new LayoutCell(Alignment.LeftCenter)
				};
				warningIcon.Tasks.Add(Tip.ShowTipOnMouseOverTask(warningIcon, () => WarningText));
				Header = new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					LayoutCell = new LayoutCell(),
				};
				deleteButton = new ThemedDeleteButton();
				deleteButton.Clicked += () =>
					history.DoTransaction(() => Core.Operations.RemoveFromList.Perform(sourceCollection, sourceCollection.IndexOf(Source)));
				var deleteButtonWrapper = new Widget {
					Layout = new HBoxLayout(),
					Nodes = {
						Spacer.HFill(),
						deleteButton,
					}
				};
				Nodes.Add(HeaderContainer = new Widget {
					Layout = new HBoxLayout(),
					Nodes = {
						Header,
						deleteButtonWrapper
					}
				});
				MinMaxHeight = AttachmentMetrics.RowHeight;
				Presenter = Presenters.StripePresenter;
			}

			private void OnStateChanged()
			{
				switch (state) {
					case RowState.Default:
						HeaderContainer.CompoundPresenter.Remove(warningPresenter);
						warningIcon.Unlink();
						break;
					case RowState.Warning:
						HeaderContainer.CompoundPresenter.Insert(0, warningPresenter);
						HeaderContainer.Nodes.Insert(0, warningIcon);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public static Widget CreateFooter(Action action)
			{
				return new Widget {
					Padding = new Thickness(AttachmentMetrics.Spacing),
					Anchors = Anchors.LeftRight,
					Layout = new HBoxLayout(),
					Nodes = {
						new ThemedAddButton {
							Clicked = action,
						},
						new ThemedSimpleText {
							Text = "Add new",
						}
					}
				};
			}
		}

		private class BlendingCell : Widget
		{
			private readonly ThemedAddButton AddButton;
			private readonly ThemedTabCloseButton RemoveButton;
			private readonly Property<BlendingOption> property;

			public BlendingCell(object obj, string propName)
			{
				Layout = new HBoxLayout();
				MinMaxHeight = 20;
				Anchors = Anchors.LeftRightTopBottom;
				property = new Property<BlendingOption>(obj, propName);
				AddButton = new ThemedAddButton {
					Anchors = Anchors.Center,
					Clicked = () =>
						history.DoTransaction(
							() => Core.Operations.SetProperty.Perform(obj, propName, new BlendingOption())),
					LayoutCell = new LayoutCell { Alignment = Alignment.Center }
				};
				RemoveButton = new ThemedTabCloseButton {
					Clicked = () =>
						history.DoTransaction(() => Core.Operations.SetProperty.Perform(obj, propName, null))
				};
				Nodes.Add(AddButton);
				AddChangeWatcher(() => property.Value, (v) => {
					Nodes.Clear();
					if (v == null) {
						Nodes.Add(Spacer.HStretch());
						Nodes.Add(AddButton);
					} else {
						new BlendingPropertyEditor(new PropertyEditorParams(this, obj, propName) {
							ShowLabel = false,
							History = history,
							PropertySetter = SetProperty
						});
						Nodes.Add(RemoveButton);
					}
				});
			}

			private void AddChangeWatcher(Func<BlendingOption> getter, Action<BlendingOption> action)
			{
				Tasks.Add(new Property<BlendingOption>(getter).DistinctUntilChanged().Consume(action));
			}
		}

		private class NodeRemovalRow : DeletableRow<Model3DAttachment.NodeRemoval>
		{
			public NodeRemovalRow(Model3DAttachment.NodeRemoval removal, ObservableCollection<Model3DAttachment.NodeRemoval> options) : base(removal, options)
			{
				Layout = new VBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var nodeIdPropertyEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						removal,
						nameof(Model3DAttachment.NodeRemoval.NodeId))));
				nodeIdPropertyEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;
				CompoundPresenter.Add(Presenters.StripePresenter);
			}

			internal static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						new ThemedSimpleText {
							Text = "Node Id",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new Widget(),
					}
				};
			}
		}

		private class MeshRow : CommonRow<Model3DAttachment.MeshOption>
		{
			public MeshRow(Model3DAttachment.MeshOption mesh) : base(mesh)
			{
				Layout = new HBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var label = new ThemedSimpleText(mesh.Id);
				Header.AddNode( new Widget {
					Nodes = { label },
					MinWidth = 0f,
					MaxWidth = float.PositiveInfinity
				});
				label.ExpandToContainerWithAnchors();
				var cullModePropEditor = new EnumPropertyEditor<CullMode>(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.CullMode))));
				cullModePropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0f;
				cullModePropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;
				var opaquePropEditor = new BooleanPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.Opaque))));
				opaquePropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0f;
				opaquePropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;
				var hitPropEditor = new BooleanPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.HitTestTarget))));
				hitPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0f;
				hitPropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;
				var disableMergingPropEditor = new BooleanPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.DisableMerging))));
				disableMergingPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0f;
				disableMergingPropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;
				CompoundPresenter.Add(Presenters.StripePresenter);
				Header.LayoutCell.StretchX = Header.Nodes.Count * 2.0f;
			}

			internal static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						CreateLabel("Node Id"),
						CreateLabel("Cull Mode"),
						CreateLabel("Opaque"),
						CreateLabel("Hit Test Target"),
						CreateLabel("Disable Merging"),
					}
				};
			}
		}

		private class AnimationRow : DeletableRow<Model3DAttachment.Animation>
		{
			private readonly ThemedExpandButton expandedButton;
			private readonly ThemedDropDownList sourceAnimationSelector;
			private readonly ObservableCollection<string> sourceAnimationIds;

			public AnimationRow(Model3DAttachment.Animation animation, Model3DAttachment attachment, ObservableCollection<string> sourceAnimationIds)
				: base(animation, attachment.Animations)
			{
				this.sourceAnimationIds = sourceAnimationIds;
				Layout = new VBoxLayout();
				expandedButton = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					Anchors = Anchors.Left,
					Id = "AnimationRowExpandedButton"
				};
				HeaderContainer.Padding = new Thickness(AttachmentMetrics.Spacing);
				Header.Nodes.Add(expandedButton);

				var animationNamePropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.Id))));
				animationNamePropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;

				sourceAnimationSelector = new ThemedDropDownList { LayoutCell = new LayoutCell(Alignment.Center) };

				foreach (var sourceAnimationId in sourceAnimationIds) {
					sourceAnimationSelector.Items.Add(new CommonDropDownList.Item(sourceAnimationId));
				}
				if (animation.SourceAnimationId == null) {
					animation.SourceAnimationId = sourceAnimationIds.FirstOrDefault();
				}

				sourceAnimationIds.CollectionChanged +=  SourceAnimationIdsOnCollectionChanged;
				sourceAnimationSelector.AsWidget.MinWidth = 0.0f;
				sourceAnimationSelector.Text = animation.SourceAnimationId;
				Header.AddNode(sourceAnimationSelector);
				sourceAnimationSelector.Changed += args => {
					history.DoTransaction(() => Core.Operations.SetProperty.Perform(animation, nameof(Model3DAttachment.Animation.SourceAnimationId), args.Value));
				};
				sourceAnimationSelector.AddChangeWatcher(() => animation.SourceAnimationId, v => sourceAnimationSelector.Text = v);
				var startFramePropEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.StartFrame))));
				startFramePropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;

				var lastFramePropEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.LastFrame))));
				lastFramePropEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;

				Header.AddNode(new BlendingCell(Source, nameof(Model3DAttachment.Animation.Blending)));

				var expandableContentWrapper = new Widget {
					Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing },
					LayoutCell = new LayoutCell { StretchY = 0 },
					Padding = new Thickness {
						Left = AttachmentMetrics.ExpandContentPadding,
						Top = AttachmentMetrics.Spacing,
						Bottom = AttachmentMetrics.Spacing
					},
					Visible = false,
				};
				expandableContentWrapper.Components.Add(new CreateContentOnVisibleBehaviour(() => {
					BuildList<Model3DAttachment.MarkerData, MarkerRow>(
					animation.Markers,
					expandableContentWrapper,
					"Markers",
					() => new Model3DAttachment.MarkerData {
						Marker = new Marker {
							Id = "Marker",
							Frame = 0,
						}
					},
					MarkerRow.CreateHeader());

					BuildList<Model3DAttachment.MarkerBlendingData, MarkerBlendingRow>(
						animation.MarkersBlendings,
						expandableContentWrapper,
						"Marker Blendings",
						() => new Model3DAttachment.MarkerBlendingData {
							SourceMarkerId = "Marker2",
							DestMarkerId = "Marker1"
						},
						MarkerBlendingRow.CreateHeader());
					BuildList<Model3DAttachment.NodeData, NodeRow>(
						animation.Nodes,
						expandableContentWrapper,
						"Nodes",
						() => new Model3DAttachment.NodeData { Id = "NodeId" },
						NodeRow.CreateHeader());

					BuildList<Model3DAttachment.NodeData, NodeRow>(
						animation.IgnoredNodes,
						expandableContentWrapper,
						"Ignored Nodes",
						() => new Model3DAttachment.NodeData { Id = "NodeId" },
						NodeRow.CreateHeader());
				}));

				Nodes.Add(expandableContentWrapper);
				expandableContentWrapper.AddChangeWatcher(
					() => expandedButton.Expanded,
					(v) => expandableContentWrapper.Visible = v);
				CompoundPresenter.Add(Presenters.StripePresenter);
				Header.LayoutCell.StretchX = Header.Nodes.Count * 2.0f;
			}

			private void SourceAnimationIdsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				sourceAnimationSelector.Items.Clear();
				foreach (var animationId in sourceAnimationIds) {
					sourceAnimationSelector.Items.Add(new CommonDropDownList.Item(animationId));
				}
			}

			public override void Dispose()
			{
				sourceAnimationIds.CollectionChanged -= SourceAnimationIdsOnCollectionChanged;
				base.Dispose();
			}

			public void Expand() => expandedButton.Expanded = true;

			internal static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = 2 * AttachmentMetrics.Spacing + AttachmentMetrics.ExpandButtonSize },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						CreateLabel("Animation name"),
						CreateLabel("Source Animation"),
						CreateLabel("Start Frame"),
						CreateLabel("Last Frame"),
						CreateLabel("Blending"),
						new Widget(),
					}
				};
			}
		}

		private static ThemedSimpleText CreateLabel(string text)
		{
			return new ThemedSimpleText {
				Text = text,
				VAlignment = VAlignment.Center,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, 2.0f),
				ForceUncutText = false,
			};
		}

		private Widget CreateEntryAnimationEditor(Model3DAttachment attachment)
		{
			var content = new Widget {
				Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing },
				Padding = new Thickness(top: AttachmentMetrics.Spacing)
			};
			var defaultAnimation = attachment.Animations.FirstOrDefault(a => a.Id == "Default")?.SourceAnimationId;
			string trigger = attachment.EntryTrigger;
			if (trigger != null && defaultAnimation != null) {
				trigger = trigger.Replace($"@{defaultAnimation}", "@Default");
			}
			var node = new Node3D {
				Trigger = trigger
			};
			var propEditorParams = new PropertyEditorParams(
				content, node, nameof(Node.Trigger), displayName: "Entry Animation");
			var editor = new TriggerPropertyEditor(propEditorParams);
			void Sync()
			{
				node.Animations.Clear();
				foreach (var a in attachment.Animations) {
					var newAnimation = new Animation { Id = a.Id };
					if (a.Id == "Default") {
						defaultAnimation = a.SourceAnimationId;
					}
					var markers = a.Markers.Select(m => new Marker { Id = m.Marker.Id });
					node.Animations.Add(newAnimation);
					foreach (var m in markers) {
						newAnimation.Markers.Add(m);
					}
				}
			}

			content.AddChangeWatcher(() => attachment.GetHashCodeForTrigger(), v => {
				Sync();
				editor.Invalidate();
			});
			content.AddChangeWatcher(() => node.Trigger, v => {
				attachment.EntryTrigger = v;
				if (attachment.EntryTrigger != null && defaultAnimation != null) {
					attachment.EntryTrigger = attachment.EntryTrigger.Replace("@Default", $"@{defaultAnimation}");
				}
			});
			return content;
		}

		private class NodeRow : DeletableRow<Model3DAttachment.NodeData>
		{
			public NodeRow(Model3DAttachment.NodeData source, ObservableCollection<Model3DAttachment.NodeData> sourceCollection) : base(source, sourceCollection)
			{
				Layout = new HBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var nodeIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						source,
						nameof(Model3DAttachment.NodeData.Id))));
				nodeIdPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;
				Presenter = Presenters.StripePresenter;
				Header.LayoutCell.StretchX = Header.Nodes.Count * 2.0f;
			}

			internal static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						CreateLabel("Node Id"),
						new Widget(),
					}
				};
			}
		}

		private class MarkerBlendingRow : DeletableRow<Model3DAttachment.MarkerBlendingData>
		{
			public MarkerBlendingRow(Model3DAttachment.MarkerBlendingData source, ObservableCollection<Model3DAttachment.MarkerBlendingData> sourceCollection) : base(source, sourceCollection)
			{
				Layout = new HBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var destMarkerPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.DestMarkerId))));
				destMarkerPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;

				var sourceMarkerPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.SourceMarkerId))));
				sourceMarkerPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;

				var blendingOptionEditBox = new BlendingPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.Blending))));
				blendingOptionEditBox.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;
				Header.LayoutCell.StretchX = Header.Nodes.Count * 2.0f;
			}

			public static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout() { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						CreateLabel("Marker Id"),
						CreateLabel("Source Marker Id"),
						CreateLabel("Blending Option"),
						new Widget(),
					}
				};
			}
		}

		private class MarkerRow : DeletableRow<Model3DAttachment.MarkerData>
		{
			public MarkerRow(
				Model3DAttachment.MarkerData marker,
				ObservableCollection<Model3DAttachment.MarkerData> markers) : base(marker, markers)
			{
				Layout = new HBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var markerIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Id))));
				markerIdPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;

				var frameEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Frame))));
				frameEditor.ContainerWidget.Nodes[0].AsWidget.MaxWidth = float.PositiveInfinity;
				var actionPropEditor = new EnumPropertyEditor<MarkerAction>(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Action))));
				actionPropEditor.ContainerWidget.Nodes[0].AsWidget.MinWidth = 0.0f;
				var jumpToPropEditor = new ThemedComboBox { LayoutCell = new LayoutCell(Alignment.Center) };
				jumpToPropEditor.MinSize = Vector2.Zero;
				jumpToPropEditor.MaxSize = Vector2.PositiveInfinity;
				jumpToPropEditor.Nodes[0].AsWidget.MinWidth = 0.0f;
				var previousMarkerId = Source.Marker.Id;
				jumpToPropEditor.Changed += args => {
					if ((string)args.Value != Source.Marker.JumpTo) {
						history.DoTransaction(() => Core.Operations.SetProperty.Perform(Source.Marker, nameof(Marker.JumpTo), args.Value));
					}
				};
				Header.AddNode(jumpToPropEditor);
				jumpToPropEditor.Clicked += () => {
					jumpToPropEditor.Items.Clear();
					foreach (var item in markers) {
						jumpToPropEditor.Items.Add(new CommonDropDownList.Item(item.Marker.Id));
					}
				};
				jumpToPropEditor.AddChangeWatcher(() => Source.Marker.JumpTo, v => jumpToPropEditor.Text = v);
				jumpToPropEditor.AddChangeWatcher(() => Source.Marker.Id, v => {
					foreach (var m in markers.Where(md => md.Marker.JumpTo == previousMarkerId).Select(md => md.Marker)) {
						m.JumpTo = v;
					}
					previousMarkerId = v;
				});
				jumpToPropEditor.Value = Source.Marker.JumpTo;
				Header.AddNode(new BlendingCell(Source, nameof(Model3DAttachment.MarkerData.Blending)));
				Header.LayoutCell.StretchX = Header.Nodes.Count * 2.0f;
			}

			public static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						CreateLabel("Marker Id"),
						CreateLabel("Frame"),
						CreateLabel("Action"),
						CreateLabel("JumpTo"),
						CreateLabel("Blending"),
						new Widget(),
					}
				};
			}
		}

		private class NodeComponentCollectionRow : DeletableRow<Model3DAttachment.NodeComponentCollection>
		{
			public NodeComponentCollectionRow(
				Model3DAttachment.NodeComponentCollection source,
				ObservableCollection<Model3DAttachment.NodeComponentCollection> sourceCollection) : base(source, sourceCollection)
			{
				Layout = new VBoxLayout();
				var expandedButton = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					Anchors = Anchors.Left
				};
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header.Nodes.Add(expandedButton);
				if (source.IsRoot) {
					Header.AddNode(new ThemedSimpleText("<Root>"));
				} else {
					var nodeIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						source,
						nameof(Model3DAttachment.NodeComponentCollection.NodeId))));
					nodeIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;
				}

				var expandableContentWrapper = new Widget {
					Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing },
					LayoutCell = new LayoutCell { StretchY = 0 },
					Padding = new Thickness {
						Left = AttachmentMetrics.ExpandContentPadding,
						Top = AttachmentMetrics.Spacing,
						Bottom = AttachmentMetrics.Spacing
					},
					Visible = false,
				};

				if (source.Components == null) {
					source.Components = new ObservableCollection<NodeComponent>();
				}

				BuildList(source.Components, expandableContentWrapper);
				Nodes.Add(expandableContentWrapper);
				expandableContentWrapper.AddChangeWatcher(
					() => expandedButton.Expanded,
					(v) => expandableContentWrapper.Visible = v);
				CompoundPresenter.Add(Presenters.StripePresenter);
			}

			private static string CamelCaseToLabel(string text)
			{
				return Regex.Replace(Regex.Replace(text, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
			}

			private static void BuildList(ObservableCollection<Lime.NodeComponent> source, Widget container)
			{
				var list = new Widget {
					Layout = new VBoxLayout {Spacing = 5},
					Padding = new Thickness {
						Top = AttachmentMetrics.Spacing
					},
				};
				container.AddNode(list);
				var validComponents = Project.Current.RegisteredComponentTypes
					.Where(t => NodeCompositionValidator.ValidateComponentType(typeof(Node3D), t)).ToList();
				var widgetFactory = new AttachmentWidgetFactory<NodeComponent>(w => new NodeComponentRow(w, source), source);
				var footer = DeletableRow<NodeComponentRow>.CreateFooter(() => {
					var menu = new Menu();
					foreach (var type in validComponents.Except(GetExceptedTypes(source))) {
						ICommand command = new Command(CamelCaseToLabel(type.Name), () => {
							var constructor = type.GetConstructor(Type.EmptyTypes);
							history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
								source, source.Count, constructor.Invoke(new object[] { })));
						});
						menu.Add(command);
					}
					menu.Popup();
				});
				footer.AddChangeWatcher(() => validComponents.Except(GetExceptedTypes(source)).Any(), any => footer.Visible = any);
				widgetFactory.AddFooter(footer);
				list.Components.Add(widgetFactory);
			}

			private static IEnumerable<Type> GetExceptedTypes(IEnumerable<NodeComponent> components)
			{
				foreach (var component in components) {
					yield return component.GetType();
				}
				// Animation blending is accessed from "Animation" section, so just ignore AnimationBlenderComponent.
				yield return typeof(AnimationBlender);
			}

			public static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout {Spacing = AttachmentMetrics.Spacing},
					Padding = new Thickness { Left = 2 * AttachmentMetrics.Spacing + AttachmentMetrics.ExpandButtonSize },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						new ThemedSimpleText {
							Text = nameof(Model3DAttachment.NodeComponentCollection.NodeId),
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new Widget(),
					}
				};
			}
		}

		private class NodeComponentRow : Widget
		{
			public NodeComponentRow(NodeComponent source, ObservableCollection<NodeComponent> sourceCollection)
			{
				Presenter = null;
				Layout = new VBoxLayout();
				Nodes.Clear();
				var container = new Widget {
					Anchors = Anchors.LeftRight,
					Layout = new VBoxLayout(),
				};
				PostPresenter = new WidgetBoundsPresenter(ColorTheme.Current.Inspector.CategoryLabelBackground) {
					IgnorePadding = true
				};
				var content = new InspectorContent(container) {
					History = history
				};
				content.OnComponentRemove += c => {
					history.DoTransaction(() => Core.Operations.RemoveFromList.Perform(sourceCollection, sourceCollection.IndexOf(c)));
				};
				Nodes.Add(container);
				content.BuildForObjects(new List<object> { source });
				Padding = new Thickness { Bottom = 4f};
			}
		}

		public class BlendingPropertyEditor : CommonPropertyEditor<BlendingOption>
		{
			private readonly NumericEditBox editor;

			public BlendingPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
			{
				editor = editorParams.NumericEditBoxFactory();
				editor.Step = 1f;
				editor.MinWidth = 0.0f;
				editor.LayoutCell = new LayoutCell(Alignment.Center);
				EditorContainer.AddNode(editor);
				var current = CoalescedPropertyValue();
				editor.Submitted += text => {
					if (int.TryParse(text, out var newValue)) {
						SetProperty(new BlendingOption(newValue));
					} else {
						editor.Text = current.GetValue().Value.Frames.ToString();
					}
				};
				editor.MaxWidth = float.PositiveInfinity;
				editor.AddChangeWatcher(current, v => editor.Text = v.Value.Frames.ToString() ?? "0");
			}
		}

		public class AttachmentWidgetFactory<T> : WidgetFactoryComponent<T>
		{
			private readonly Widget wrapper;

			public AttachmentWidgetFactory(Func<T, Widget> rowBuilder, ObservableCollection<T> source) : base(rowBuilder, source)
			{
				wrapper = new Widget {
					Layout = new VBoxLayout(),
					Nodes = { Container }
				};
			}

			protected override void OnOwnerChanged(Node oldOwner)
			{
				Owner?.AddNode(wrapper);
			}

			public void AddHeader(Widget header)
			{
				wrapper.PushNode(header);
			}

			public void AddFooter(Widget footer)
			{
				wrapper.AddNode(footer);
			}
		}

		public class CreateContentOnVisibleBehaviour : NodeBehavior
		{
			private bool isBuilt;
			private readonly Action builder;

			public CreateContentOnVisibleBehaviour(Action builder)
			{
				this.builder = builder;
			}

			public override void Update(float delta)
			{
				base.Update(delta);
				if (!isBuilt && Owner.AsWidget.Visible) {
					isBuilt = true;
					builder();
					Owner.Components.Remove(this);
				}
			}
		}
	}
}

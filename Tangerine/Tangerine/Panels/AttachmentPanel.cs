using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
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

	public class AttachmentDialog
	{
		private static DocumentHistory history;

		private static IPropertyEditorParams Decorate(PropertyEditorParams @params, bool displayLabel = false)
		{
			@params.ShowLabel = displayLabel;
			@params.History = history;
			@params.PropertySetter = SetProperty;
			@params.NumericEditBoxFactory = () => new InspectorContent.TransactionalNumericEditBox(history);
			return @params;
		}

		public static void ShowFor(Model3D source)
		{
			// Do not show others attachment dialogs if one is already present
			if (history != null) return;
			history = new DocumentHistory();
			Button cancelButton;
			var attachment = ReadAttachment(source);
			var window = new Window(new WindowOptions {
				ClientSize = new Vector2(700, 400),
				FixedSize = false,
				Title = "Attachment3D",
				MinimumDecoratedSize = new Vector2(500, 300),
			});
			var content = new ThemedTabbedWidget();
			content.AddTab("General", CreateGeneralPane(attachment), true);
			content.AddTab("Components", CreateComponentsPane(attachment));
			content.AddTab("Mesh Options", CreateMeshOptionsPane(attachment));
			content.AddTab("Animations", CreateAnimationsPane(attachment));
			content.AddTab("Node Removals", CreateNodeRemovalsPane(attachment));
			Button okButton;
			WindowWidget rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					content,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Padding = new Thickness { Top = 5 },
						Nodes = {
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			window.Closed += () => {
				history = null;
				UserPreferences.Instance.Load();
			};
			cancelButton.Clicked += () => {
				window.Close();
				history = null;
				UserPreferences.Instance.Load();
			};
			okButton.Clicked += () => {
				try {
					CheckErrors(attachment, source);
					window.Close();
					history = null;
					// Since attachment dialog not present as modal window, document can be rolled back with "undo"
					// operation to the state when source isn't presented or source content path isn't set.
					// So check it out before saving.
					if (source.DescendantOf(Document.Current.RootNode) && source.ContentsPath != null) {
						SaveAttachment(attachment, source.ContentsPath);
					}
				} catch (Lime.Exception e) {
					new AlertDialog(e.Message).Show();
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					UserPreferences.Instance.Load();
				}
			});
			rootWidget.Tasks.AddLoop(() => {
				if (!Command.Undo.IsConsumed()) {
					Command.Undo.Enabled = history.CanUndo();
					if (Command.Undo.WasIssued()) {
						Command.Undo.Consume();
						history.Undo();
					}
				}
				if (!Command.Redo.IsConsumed()) {
					Command.Redo.Enabled = history.CanRedo();
					if (Command.Redo.WasIssued()) {
						Command.Redo.Consume();
						history.Redo();
					}
				}
			});
		}

		private static Model3DAttachment ReadAttachment(Model3D source)
		{
			var path = source.ContentsPath + Model3DAttachment.FileExtension;
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				if (cacheBundle.FileExists(path)) {
					using (var assetStream = cacheBundle.OpenFile(path)){
						var attachmentFormat = TangerineYuzu.Instance.Value.ReadObject<Model3DAttachmentParser.ModelAttachmentFormat>(path, assetStream);
						return Model3DAttachmentParser.GetModel3DAttachment(attachmentFormat, path);
					}
				}
			}
			var attachment = new Model3DAttachment { ScaleFactor = 1 };
			foreach (var a in source.Animations) {
				attachment.Animations.Add(new Model3DAttachment.Animation {
					Name = a.Id,
					SourceAnimationId = a.Id,
					StartFrame = 0,
					LastFrame = -1
				});
				attachment.SourceAnimationIds.Add(a.Id);
			}
			return attachment;
		}

		private static void SaveAttachment(Model3DAttachment attachment, string contentPath)
		{
			Model3DAttachmentParser.Save(attachment, Path.Combine(Project.Current.AssetsDirectory, contentPath));
		}

		private static Widget CreateComponentsPane(Model3DAttachment attachment)
		{
			var pane = new ThemedScrollView();
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Padding = new Thickness { Right = 10 };
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.NodeComponentCollection>(
				w => new NodeComponentCollectionRow(w, attachment.NodeComponents), attachment.NodeComponents);
			widgetFactory.AddHeader(NodeComponentCollectionRow.CreateHeader());
			widgetFactory.AddFooter(DeletableRow<Model3DAttachment.NodeComponentCollection>.CreateFooter(() => {
				history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
					attachment.NodeComponents,
					attachment.NodeComponents.Count,
					new Model3DAttachment.NodeComponentCollection { NodeId = "Node id", Components = null }));
			}));
			list.Components.Add(widgetFactory);
			return pane;
		}

		private static void CheckErrors(Model3DAttachment attachment, Model3D source)
		{
			if (new HashSet<string>(attachment.Animations.Select(a => a.Name)).Count != attachment.Animations.Count) {
				throw new Lime.Exception("Animations shouldn't have the same names");
			}

			var errorAnim = attachment.Animations.FirstOrDefault(a =>
				new HashSet<string>(a.Markers.Select(m => m.Marker.Id)).Count() != a.Markers.Count);
			if (errorAnim != null) {
				throw new Lime.Exception($"Markers in '{ errorAnim.Name }' animation shouldn't have the same ids");
			}

			if (new HashSet<string>(attachment.MeshOptions.Select(a => a.Id)).Count != attachment.MeshOptions.Count) {
				throw new Lime.Exception("Mesh options shouldn't have the same node ids");
			}

			if (new HashSet<string>(attachment.NodeComponents.Select(a => a.NodeId)).Count != attachment.NodeComponents.Count) {
				throw new Lime.Exception("Node components shouldn't have the same node ids");
			}
		}

		private static Widget CreateNodeRemovalsPane(Model3DAttachment attachment)
		{
			var pane = new ThemedScrollView();
			pane.Content.Padding = new Thickness { Right = 10 };
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
			return pane;
		}

		private static Widget CreateAnimationsPane(Model3DAttachment attachment)
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness { Right = 10 },
			};
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.Animation>(
					w => new AnimationRow(w, attachment), attachment.Animations);
			widgetFactory.AddHeader(AnimationRow.CreateHeader());
			widgetFactory.AddFooter(AnimationRow.CreateFooter(() => {
				history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
					attachment.Animations,
					attachment.Animations.Count,
					new Model3DAttachment.Animation { Name = "Animation", }
				));
			}));
			list.Components.Add(widgetFactory);
			return pane;
		}

		private static Widget CreateMeshOptionsPane(Model3DAttachment attachment)
		{
			var pane = new ThemedScrollView();
			pane.Content.Padding = new Thickness { Right = 10 };
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.MeshOption>(
				w => new MeshRow(w, attachment.MeshOptions), attachment.MeshOptions);
			widgetFactory.AddHeader(MeshRow.CreateHeader());
			widgetFactory.AddFooter(MeshRow.CreateFooter(() => {
				history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(
					attachment.MeshOptions,
					attachment.MeshOptions.Count,
					new Model3DAttachment.MeshOption { Id = "MeshOption" }
				));
			}));
			list.Components.Add(widgetFactory);
			return pane;
		}

		private static Widget CreateGeneralPane(Model3DAttachment attachment)
		{
			var pane = new ThemedScrollView();
			pane.Content.Padding = new Thickness(10, AttachmentMetrics.Spacing);
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			new FloatPropertyEditor(Decorate(new PropertyEditorParams(
					pane.Content,
					attachment,
					nameof(Model3DAttachment.ScaleFactor),
					nameof(Model3DAttachment.ScaleFactor)), displayLabel: true));
			return pane;
		}

		private static void SetProperty(object obj, string propertyname, object value)
		{
			Core.Operations.SetProperty.Perform(obj, propertyname, value);
		}

		private class DeletableRow<T> : Widget
		{
			protected T Source { get; }
			protected Widget Header { get; }
			private IList<T> SourceCollection { get; }
			protected readonly ThemedDeleteButton deleteButton;

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

			protected DeletableRow(T source, ObservableCollection<T> sourceCollection)
			{
				Source = source;
				SourceCollection = sourceCollection;
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header = new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
				};
				var headerWrapper = new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
				};
				deleteButton = new ThemedDeleteButton {
					Anchors = Anchors.Right,
					LayoutCell = new LayoutCell(Alignment.LeftTop),
				};
				deleteButton.Clicked += () =>
					history.DoTransaction(() => Core.Operations.RemoveFromList.Perform(sourceCollection, sourceCollection.IndexOf(Source)));
				headerWrapper.Nodes.Add(Header);
				headerWrapper.Nodes.Add(new Widget());
				headerWrapper.Nodes.Add(deleteButton);
				MinMaxHeight = AttachmentMetrics.RowHeight;
				Nodes.Add(headerWrapper);
				Presenter = Presenters.StripePresenter;
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

		private class MeshRow : DeletableRow<Model3DAttachment.MeshOption>
		{
			public MeshRow(Model3DAttachment.MeshOption mesh, ObservableCollection<Model3DAttachment.MeshOption> options) : base(mesh, options)
			{
				Layout = new VBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var meshIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.Id))));
				meshIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var cullModePropEditor = new EnumPropertyEditor<CullMode>(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.CullMode))));
				cullModePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var opaquePropEditor = new BooleanPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.Opaque))));
				opaquePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

				var hitPropEditor = new BooleanPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.HitTestTarget))));
				hitPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;
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
						new ThemedSimpleText {
							Text = "Cull Mode",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Opaque",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Hit Test Target",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new Widget(),
					}
				};
			}
		}

		private class AnimationRow : DeletableRow<Model3DAttachment.Animation>
		{
			public AnimationRow(Model3DAttachment.Animation animation, Model3DAttachment attachment)
				: base(animation, attachment.Animations)
			{
				Layout = new VBoxLayout();
				var expandedButton = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					Anchors = Anchors.Left
				};
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header.Nodes.Add(expandedButton);

				var animationNamePropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.Name))));
				animationNamePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var sourceAnimationSelector = new ThemedDropDownList { LayoutCell = new LayoutCell(Alignment.Center) };
				foreach (var sourceAnimationId in attachment.SourceAnimationIds) {
					sourceAnimationSelector.Items.Add(new CommonDropDownList.Item(sourceAnimationId));
				}
				if (animation.SourceAnimationId == null) {
					animation.SourceAnimationId = attachment.SourceAnimationIds.FirstOrDefault();
				}
				sourceAnimationSelector.MinMaxWidth = AttachmentMetrics.ControlWidth;
				sourceAnimationSelector.Text = animation.SourceAnimationId;
				Header.AddNode(sourceAnimationSelector);
				sourceAnimationSelector.Changed += args => { animation.SourceAnimationId = (string)args.Value; };

				var startFramePropEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.StartFrame))));
				startFramePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

				var lastFramePropEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.LastFrame))));
				lastFramePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

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

				Nodes.Add(expandableContentWrapper);
				expandableContentWrapper.AddChangeWatcher(
					() => expandedButton.Expanded,
					(v) => expandableContentWrapper.Visible = v);
				CompoundPresenter.Add(Presenters.StripePresenter);
			}

			private void BuildList<TData, TRow>(ObservableCollection<TData> source, Widget container, string title, Func<TData> activator, Widget header) where TRow : DeletableRow<TData>
			{
				ThemedExpandButton markersExpandButton;
				container.AddNode(new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Nodes = {
						(markersExpandButton = new ThemedExpandButton {
							MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize)
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
				container.AddNode(list);
				var widgetFactory = new AttachmentWidgetFactory<TData>(
					w => (TRow)Activator.CreateInstance(typeof(TRow), new object[] { w, source }), source);
				widgetFactory.AddHeader(header);
				widgetFactory.AddFooter(DeletableRow<TData>.CreateFooter(() => {
					history.DoTransaction(() => Core.Operations.InsertIntoList.Perform(source, source.Count, activator()));
				}));
				list.Components.Add(widgetFactory);
				this.AddChangeWatcher(() => markersExpandButton.Expanded, (e) => list.Visible = e);
			}

			internal static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = 2 * AttachmentMetrics.Spacing + AttachmentMetrics.ExpandButtonSize },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						new ThemedSimpleText {
							Text = "Animation name",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Source Animation",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Start Frame",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Last Frame",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new ThemedSimpleText {
							Text = "Blending",
							MinMaxWidth = 80,
							VAlignment = VAlignment.Center,
							ForceUncutText = false
						},
						new Widget(),
					}
				};
			}
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
				nodeIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;
				Presenter = Presenters.StripePresenter;
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
						},
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
				var destMarkerPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.DestMarkerId))));
				destMarkerPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var sourceMarkerPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.SourceMarkerId))));
				sourceMarkerPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var blendingOptionEditBox = new BlendingPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.Blending))));
				blendingOptionEditBox.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;
			}

			public static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout() { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						new ThemedSimpleText {
							Text = "Marker Id",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
						},
						new ThemedSimpleText {
							Text = "Source Marker Id",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
						},
						new ThemedSimpleText {
							Text = "Blending Option",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
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
				var markerIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Id))));
				markerIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var frameEditor = new IntPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Frame))));
				frameEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;
				var actionPropEditor = new EnumPropertyEditor<MarkerAction>(
					Decorate(new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Action))));
				actionPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;
				var jumpToPropEditor = new ThemedComboBox { LayoutCell = new LayoutCell(Alignment.Center) };
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
				jumpToPropEditor.MinMaxWidth = AttachmentMetrics.ControlWidth;
				Header.AddNode(new BlendingCell(Source, nameof(Model3DAttachment.MarkerData.Blending)));
			}

			public static Widget CreateHeader()
			{
				return new Widget {
					Layout = new HBoxLayout { Spacing = AttachmentMetrics.Spacing },
					Padding = new Thickness { Left = AttachmentMetrics.Spacing },
					MinMaxHeight = 20,
					Presenter = Presenters.HeaderPresenter,
					Nodes = {
						new ThemedSimpleText {
							Text = "Marker Id",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
						},
						new ThemedSimpleText {
							Text = "Frame",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new ThemedSimpleText {
							Text = "Action",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new ThemedSimpleText {
							Text = "JumpTo",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new ThemedSimpleText {
							Text = "Blending",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
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

				var nodeIdPropEditor = new StringPropertyEditor(
					Decorate(new PropertyEditorParams(
						Header,
						source,
						nameof(Model3DAttachment.NodeComponentCollection.NodeId))));
				nodeIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

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
				editor.MinMaxWidth = 80;
				editor.LayoutCell = new LayoutCell(Alignment.Center);
				EditorContainer.AddNode(editor);
				var current = CoalescedPropertyValue();
				editor.Submitted += text => {
					if (int.TryParse(text, out var newValue)) {
						SetProperty(new BlendingOption(newValue));
					} else {
						editor.Text = current.GetValue().Frames.ToString();
					}
				};
				editor.AddChangeWatcher(current, v => editor.Text = v?.Frames.ToString() ?? "0");
			}
		}

		private class AttachmentWidgetFactory<T> : WidgetFactoryComponent<T>
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
	}
}

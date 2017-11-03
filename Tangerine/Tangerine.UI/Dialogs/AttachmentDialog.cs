using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	internal static class Presenters
	{
		public static IPresenter StripePresenter = new DelegatePresenter<Widget>(
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
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Button okButton;
		private readonly Button cancelButton;
		private readonly Frame frame;
		private readonly TabbedWidget content;
		private readonly Model3D source;
		private readonly Model3DAttachment attachment;
		private readonly string Path;

		public AttachmentDialog(Model3D source)
		{
			Path = source.ContentsPath;
			this.source = source;
			attachment = new Model3DAttachmentParser().Parse(Path) ?? new Model3DAttachment { ScaleFactor = 1 };
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(700, 400),
				FixedSize = false,
				Title = "Attachment3D",
				MinimumDecoratedSize = new Vector2(500, 300)
			});
			frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			content = new TabbedWidget();
			content.AddTab("General", CreateGeneralPane(), true);
			content.AddTab("Material Effects", CreateMaterialEffectsPane());
			content.AddTab("Mesh Options", CreateMeshOptionsPane());
			content.AddTab("Animations", CreateAnimationsPane());

			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					content,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			cancelButton.Clicked += () => {
				window.Close();
				Core.UserPreferences.Instance.Load();
			};
			okButton.Clicked += () => {
				try {
					CheckErrors();
					window.Close();
					Model3DAttachmentParser.Save(attachment, System.IO.Path.Combine(Project.Current.AssetsDirectory, Path));
				} catch (Lime.Exception e) {
					new AlertDialog(e.Message).Show();
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					Core.UserPreferences.Instance.Load();
				}
			});
		}

		private void CheckErrors()
		{
			if (new HashSet<string>(attachment.Animations.Select(a => a.Name)).Count != attachment.Animations.Count ||
				attachment.Animations.Any(a => a.Name == source.Animations.DefaultAnimation.Id)
			) {
				throw new Lime.Exception("Animations have simmilar names");
			}
			var defaultAnimation = attachment.Animations.FirstOrDefault(a => a.Name == Model3DAttachment.DefaultAnimationName);
		}

		private Widget CreateAnimationsPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.Animation>(
					w => new AnimationRow(w, attachment.Animations), attachment.Animations);
			widgetFactory.AddHeader(AnimationRow.CreateHeader());
			widgetFactory.AddFooter(AnimationRow.CreateFooter(() => {
				attachment.Animations.Add(new Model3DAttachment.Animation {
					Name = "Animation",
				});
			}));
			if (!attachment.Animations.Any(a => a.Name == Model3DAttachment.DefaultAnimationName)) {
				attachment.Animations.Insert(0, new Model3DAttachment.Animation {
					Name = Model3DAttachment.DefaultAnimationName,
				});
			}
			list.Components.Add(widgetFactory);
			return pane;
		}

		private Widget CreateMeshOptionsPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.MeshOption>(
				w => new MeshRow(w, attachment.MeshOptions), attachment.MeshOptions);
			widgetFactory.AddHeader(MeshRow.CreateHeader());
			widgetFactory.AddFooter(MeshRow.CreateFooter(() => {
				attachment.MeshOptions.Add(new Model3DAttachment.MeshOption {
					Id = "MeshOption",
				});
			}));
			list.Components.Add(widgetFactory);
			return pane;
		}

		private Widget CreateMaterialEffectsPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			var list = new Widget {
				Layout = new VBoxLayout(),
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			pane.Content.AddNode(list);
			var widgetFactory = new AttachmentWidgetFactory<Model3DAttachment.MaterialEffect>(
					w => new MaterialEffectRow(w, attachment.MaterialEffects), attachment.MaterialEffects);
			widgetFactory.AddHeader(MaterialEffectRow.CreateHeader());
			widgetFactory.AddFooter(DeletableRow<Model3DAttachment.MaterialEffect>.CreateFooter(() => {
				attachment.MaterialEffects.Add(new Model3DAttachment.MaterialEffect {
					Name = "MaterialEffect",
					MaterialName = "MaterialName",
					Path = "MaterialPath",
				});
			}));
			list.Components.Add(widgetFactory);
			return pane;
		}

		private Widget CreateGeneralPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			pane.Content.Layout = new VBoxLayout { Spacing = AttachmentMetrics.Spacing };
			new FloatPropertyEditor(
				new PropertyEditorParams(pane.Content, attachment, nameof(Model3DAttachment.ScaleFactor), "Scale Factor"));
			return pane;
		}

		private static NotifyCollectionChangedEventHandler CreateCollectionChangedCallback(IList list)
		{
			return (s, e) => {
				if (e.OldItems != null) {
					foreach (var item in e.OldItems) {
						list.Remove(item);
					}
				}
				if (e.NewItems != null) {
					foreach (var item in e.NewItems) {
						list.Add(item);
					}
				}
			};
		}

		private class DeletableRow<T> : Widget
		{
			public T Source { get; set; }
			public Widget Header { get; private set; }
			public IList<T> SourceCollection { get; private set; }
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

			public DeletableRow(T source, ObservableCollection<T> sourceCollection)
			{
				Source = source;
				SourceCollection = sourceCollection;
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header = new Widget {
					Layout = new HBoxLayout() { Spacing = AttachmentMetrics.Spacing },
				};
				var headerWrapper = new Widget {
					Layout = new HBoxLayout() { Spacing = AttachmentMetrics.Spacing },
				};
				deleteButton = new ThemedDeleteButton {
					Anchors = Anchors.Right,
					LayoutCell = new LayoutCell(Alignment.LeftTop),
				};
				deleteButton.Clicked += () => sourceCollection.Remove(Source);
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
					Clicked = () => property.Value = new BlendingOption(),
					LayoutCell = new LayoutCell { Alignment = Alignment.Center }
				};
				RemoveButton = new ThemedTabCloseButton {
					Clicked = () => property.Value = null
				};
				Nodes.Add(AddButton);
				AddChangeWatcher(() => property.Value, (v) => {
					Nodes.Clear();
					if (v == null) {
						Nodes.Add(AddButton);
					} else {
						new BlendingPropertyEditor(new PropertyEditorParams(this, obj, propName) { ShowLabel = false });
						Nodes.Add(RemoveButton);
					}
				});
			}

			private void AddChangeWatcher(Func<BlendingOption> getter, Action<BlendingOption> action)
			{
				Tasks.Add(new Property<BlendingOption>(getter).DistinctUntilChanged().Consume(action));
			}
		}

		private class MeshRow : DeletableRow<Model3DAttachment.MeshOption>
		{
			public MeshRow(Model3DAttachment.MeshOption mesh, ObservableCollection<Model3DAttachment.MeshOption> options) : base(mesh, options)
			{
				Layout = new VBoxLayout();
				Padding = new Thickness(AttachmentMetrics.Spacing);
				var meshIdPropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.Id)) { ShowLabel = false });
				meshIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var cullModePropEditor = new EnumPropertyEditor<CullMode>(
					new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.CullMode)) { ShowLabel = false });
				cullModePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var hitPropEditor = new BooleanPropertyEditor(
					new PropertyEditorParams(
						Header,
						mesh,
						nameof(Model3DAttachment.MeshOption.HitTestTarget)) { ShowLabel = false });
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
						},
						new ThemedSimpleText {
							Text = "Cull Mode",
							MinMaxWidth = AttachmentMetrics.EditorWidth,
						},
						new ThemedSimpleText {
							Text = "Hit Test Target",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new Widget(),
					}
				};
			}
		}

		private class AnimationRow : DeletableRow<Model3DAttachment.Animation>
		{
			public AnimationRow(Model3DAttachment.Animation animation, ObservableCollection<Model3DAttachment.Animation> options)
				: base(animation, options)
			{
				var isDefault = animation.Name == Model3DAttachment.DefaultAnimationName;
				deleteButton.Visible = !isDefault;
				Layout = new VBoxLayout();
				var expandedButton = new ThemedExpandButton {
					MinMaxSize = new Vector2(AttachmentMetrics.ExpandButtonSize),
					Anchors = Anchors.Left
				};
				Padding = new Thickness(AttachmentMetrics.Spacing);
				Header.Nodes.Add(expandedButton);

				var animationNamePropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						animation,
						nameof(Model3DAttachment.Animation.Name)) { ShowLabel = false });
				animationNamePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

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
				if (!isDefault) {
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
				}


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
					source.Add(activator());
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
						},
						new ThemedSimpleText {
							Text = "Blending",
							MinMaxWidth = 80,
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
					new PropertyEditorParams(
						Header,
						source,
						nameof(Model3DAttachment.NodeData.Id)) { ShowLabel = false });
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
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.DestMarkerId)) { ShowLabel = false });
				destMarkerPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var sourceMarkerPropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.SourceMarkerId)) { ShowLabel = false });
				sourceMarkerPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var blendingOptionEditBox = new BlendingPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MarkerBlendingData.Blending)) { ShowLabel = false });
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
					new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Id)) { ShowLabel = false });
				markerIdPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.EditorWidth;

				var framePropEditor = new IntPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Frame)) { ShowLabel = false });

				var actionPropEditor = new EnumPropertyEditor<MarkerAction>(
					new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.Action)) { ShowLabel = false });
				actionPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

				var jumpToPropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source.Marker,
						nameof(Marker.JumpTo)) { ShowLabel = false });
				jumpToPropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;
				Header.AddNode(new BlendingCell(Source, nameof(Model3DAttachment.MarkerData.Blending)));
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
							Text = "Frame",
							MinMaxWidth = 80,
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

		private class MaterialEffectRow : DeletableRow<Model3DAttachment.MaterialEffect>
		{
			public MaterialEffectRow(
				Model3DAttachment.MaterialEffect source,
				ObservableCollection<Model3DAttachment.MaterialEffect> sourceCollection) : base(source, sourceCollection)
			{
				Layout = new HBoxLayout();
				var namePropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MaterialEffect.Name)) { ShowLabel = false });
				namePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

				var materialNamePropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MaterialEffect.MaterialName)) { ShowLabel = false });
				materialNamePropEditor.ContainerWidget.MinMaxWidth = AttachmentMetrics.ControlWidth;

				var pathPropEditor = new StringPropertyEditor(
					new PropertyEditorParams(
						Header,
						Source,
						nameof(Model3DAttachment.MaterialEffect.Path)) { ShowLabel = false });
				pathPropEditor.ContainerWidget.MinMaxWidth = 2 * AttachmentMetrics.EditorWidth;

				Header.AddNode(new BlendingCell(Source, nameof(Model3DAttachment.MaterialEffect.Blending)));
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
							Text = "Name",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new ThemedSimpleText {
							Text = "Material Name",
							MinMaxWidth = AttachmentMetrics.ControlWidth,
						},
						new ThemedSimpleText {
							Text = "Path",
							MinMaxWidth = 2 * AttachmentMetrics.EditorWidth,
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

		public class BlendingPropertyEditor : CommonPropertyEditor<BlendingOption>
		{
			private readonly NumericEditBox editor;

			public BlendingPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
			{
				editor = editorParams.NumericEditBoxFactory();
				editor.Step = 1f;
				editor.MinMaxWidth = 80;
				editor.LayoutCell = new LayoutCell(Alignment.Center);
				ContainerWidget.AddNode(editor);
				var current = CoalescedPropertyValue();
				editor.Submitted += text => {
					int newValue;
					if (int.TryParse(text, out newValue)) {
						SetProperty(new BlendingOption(newValue));
					} else {
						editor.Text = current.GetValue().DurationInFrames.ToString();
					}
				};
				editor.AddChangeWatcher(current, v => editor.Text = v?.DurationInFrames.ToString() ?? "0");
			}
		}

		private class AttachmentWidgetFactory<T> : WidgetFactoryComponent<T>
		{
			public Widget wrapper;

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

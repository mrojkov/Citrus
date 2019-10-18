using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI.Docking;
using Tangerine.UI.FilesDropHandler;
using Tangerine.UI.SceneView.Presenters;

namespace Tangerine.UI.SceneView
{
	public class SceneView : IDocumentView
	{
		private readonly FilesDropManager filesDropManager;
		private Vector2 mousePositionOnFilesDrop;

		/// <summary>
		/// A collection of IFilesDropHandler which bring functionality of
		/// files drag and drop. Can be extended via orange plugins. This collection will
		/// be cloned by Yuzu for each instance of Sceneview
		/// </summary>
		public static List<IFilesDropHandler> FilesDropHandlers { get; } = new List<IFilesDropHandler> {
			new AudiosDropHandler(), new ImagesDropHandler(), new ScenesDropHandler()
		};
		// Given panel.
		public readonly Widget Panel;
		// Widget which is a direct child of the panel.
		public readonly Widget Frame;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		public readonly Widget InputArea;
		public WidgetInput Input => InputArea.Input;
		// Container for the document root node.
		public readonly Widget Scene;
		public static readonly RulersWidget RulersWidget = new RulersWidget();
		public static readonly ZoomWidget ZoomWidget = new ZoomWidget();
		public static readonly ToolbarButton ShowNodeDecorationsPanelButton = new ToolbarButton {
			Tooltip = "Node decorations",
			Texture = IconPool.GetTexture("SceneView.ShowPanel"),
			MinMaxSize = new Vector2(24),
			LayoutCell = new LayoutCell(new Alignment { X = HAlignment.Left, Y = VAlignment.Bottom } )
		};

		/// <summary>
		/// Gets the mouse position in the scene coordinates.
		/// </summary>
		public Vector2 MousePosition => Scene.LocalMousePosition();

		public ComponentCollection<Component> Components = new ComponentCollection<Component>();

		public static SceneView Instance { get; private set; }

		public static void RegisterGlobalCommands()
		{
			ConnectCommand(SceneViewCommands.PreviewAnimation, new PreviewAnimationHandler(false));
			ConnectCommand(SceneViewCommands.PreviewAnimationWithTriggeringOfMarkers, new PreviewAnimationHandler(true));
			ConnectCommand(SceneViewCommands.ResolutionChanger, new ResolutionChangerHandler());
			ConnectCommand(SceneViewCommands.ResolutionReverceChanger, new ResolutionChangerHandler(isReverse: true));
			ConnectCommand(SceneViewCommands.ResolutionOrientation, new ResolutionOrientationHandler());
			ConnectCommand(SceneViewCommands.Duplicate, DuplicateNodes,
				() => Document.Current?.TopLevelSelectedRows().Any(row => row.IsCopyPasteAllowed()) ?? false);
			ConnectCommand(SceneViewCommands.TieWidgetsWithBones, TieWidgetsWithBones);
			ConnectCommand(SceneViewCommands.UntieWidgetsFromBones, UntieWidgetsFromBones);
		}

		private static void ConnectCommand(ICommand command, DocumentCommandHandler handler)
		{
			CommandHandlerList.Global.Connect(command, handler);
		}

		private static void ConnectCommand(ICommand command, Action action, Func<bool> enableChecker = null)
		{
			CommandHandlerList.Global.Connect(command, new DocumentDelegateCommandHandler(action, enableChecker));
		}

		private static void DuplicateNodes()
		{
			var text = Clipboard.Text;
			try {
				Copy.CopyToClipboard();
				Paste.Perform();
			} finally {
				Clipboard.Text = text;
			}
		}

		private static void TieWidgetsWithBones()
		{
			try {
				var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>();
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				Core.Operations.TieWidgetsWithBones.Perform(bones, widgets);
			} catch (TieWidgetsWithBonesException e) {
				Document.Current.History.RollbackTransaction();
				AlertDialog.Show($"Unable to tie bones with {e.Node.Id} node. There are no empty skinning slots.");
			}
		}

		private static void UntieWidgetsFromBones()
		{
			var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>();
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
			Core.Operations.UntieWidgetsFromBones.Perform(bones, widgets);
		}

		public SceneView(Widget panelWidget)
		{
			this.Panel = panelWidget;
			InputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			InputArea.FocusScope = new KeyboardFocusScope(InputArea);
			Scene = new Widget {
				Nodes = { Document.Current.RootNode }
			};
			Frame = new Widget {
				Id = "SceneView",
				Nodes = { InputArea, Scene }
			};
			CreateComponents();
			CreateProcessors();
			CreatePresenters();
			filesDropManager = new FilesDropManager(InputArea);
			filesDropManager.AddFilesDropHandlers(FilesDropHandlers.Select(fdh =>
				(IFilesDropHandler)Lime.Yuzu.Instance.Value.Clone(fdh)));
			filesDropManager.Handling += FilesDropOnHandling;
			filesDropManager.NodeCreated += FilesDropOnNodeCreated;
			Scene.AddChangeWatcher(() => Document.Current.SlowMotion, v => AdjustSceneAnimationSpeed());
			Scene.AddChangeWatcher(() => Document.Current.PreviewAnimation, v => AdjustSceneAnimationSpeed());
			Frame.Awoke += CenterDocumentRoot;
		}

		private void AdjustSceneAnimationSpeed()
		{
			Scene.AnimationSpeed = GetRequiredSceneAnimationSpeed();
		}

		private float GetRequiredSceneAnimationSpeed()
		{
			if (Document.Current.PreviewAnimation) {
				if (Document.Current.SlowMotion) {
					return 0.1f;
				} else {
					return 1.0f;
				}
			}
			return 0.0f;
		}

		private void CenterDocumentRoot(Node node)
		{
			// Before Frame awakens something is being changed on this frame enough to change Frame Size on LayoutManager.Layout()
			// which will come at the end of the frame. Force it now to get accurate Frame.Size;
			WidgetContext.Current.Root.LayoutManager.Layout();
			var rulerSize = RulersWidget.RulerHeight * (RulersWidget.Visible ? 1 : 0);
			var widget = Document.Current.RootNode.AsWidget;
			var frameWidth = Frame.Width - rulerSize;
			var frameHeight = Frame.Height - ZoomWidget.FrameHeight - rulerSize;
			var wnatedZoom = Mathf.Clamp(Mathf.Min(frameWidth / (widget.Width * widget.Scale.X), frameHeight / (widget.Height * widget.Scale.Y)), 0.0f, 1.0f);
			var zoomIndex = ZoomWidget.FindNearest(wnatedZoom, 0, ZoomWidget.zoomTable.Count);
			Scene.Scale = new Vector2(ZoomWidget.zoomTable[zoomIndex]);
			Scene.Position = -(widget.Position + widget.Size * widget.Scale * 0.5f) * Scene.Scale + new Vector2(frameWidth * 0.5f, frameHeight * 0.5f) + Vector2.One * rulerSize;
		}

		private void FilesDropOnHandling()
		{
			if (!Window.Current.Active) {
				Window.Current.Activate();
				InputArea.SetFocus();
			}
			mousePositionOnFilesDrop = MousePosition * Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
		}

		private void FilesDropOnNodeCreated(Node node)
		{
			if (node is Widget) {
				SetProperty.Perform(node, nameof(Widget.Position), mousePositionOnFilesDrop);
			}
		}

		public void Attach()
		{
			Instance = this;
			Panel.AddNode(ShowNodeDecorationsPanelButton);
			Panel.AddNode(ZoomWidget);
			Panel.AddNode(RulersWidget);
			Panel.AddNode(Frame);
			DockManager.Instance.AddFilesDropManager(filesDropManager);
		}

		public void Detach()
		{
			DockManager.Instance.RemoveFilesDropManager(filesDropManager);
			Instance = null;
			Frame.Unlink();
			ShowNodeDecorationsPanelButton.Unlink();
			RulersWidget.Unlink();
			ZoomWidget.Unlink();
		}

		/// <summary>
		/// Checks whether the mouse is over a control point within a given radius.
		/// </summary>
		public bool HitTestControlPoint(Vector2 controlPoint, float radius = 10)
		{
			return (controlPoint - MousePosition).Length < radius / Scene.Scale.X;
		}

		/// <summary>
		/// Checks whether the mouse is over a control point within a specific for resize radius.
		/// </summary>
		public bool HitTestResizeControlPoint(Vector2 controlPoint)
		{
			return HitTestControlPoint(controlPoint, 6);
		}

		void CreateComponents() { }

		void CreateProcessors()
		{
			Frame.Tasks.Add(
				new ActivateOnMouseOverProcessor(),
				new CreateWidgetProcessor(),
				new CreateSplinePointProcessor(),
				new CreatePointObjectProcessor(),
				new CreateSplinePoint3DProcessor(),
				new CreateBoneProcessor(),
				new CreateNodeProcessor(),
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new DragPivotProcessor(),
				new DragBoneProcessor(),
				new ChangeBoneRadiusProcessor(),
				new RotateBoneProcessor(),
				new DragPointObjectsProcessor(),
				new DragSplineTangentsProcessor(),
				new DragSplinePoint3DProcessor(),
				new DragAnimationPathPointProcessor(),
				new DragWidgetsProcessor(),
				new ResizeWidgetsProcessor(),
				new RescalePointObjectSelectionProcessor(),
				new RotatePointObjectSelectionProcessor(),
				new RotateWidgetsProcessor(),
				new RulerProcessor(),
				new DragNineGridLineProcessor(),
				new ShiftTimelineProcessor(),
				new MouseSelectionProcessor(),
				new ShiftClickProcessor(),
				new PreviewAnimationProcessor(),
				new ResolutionPreviewProcessor(),
				new FrameProgressionProcessor()
			);
		}

		void CreatePresenters()
		{
			new Bone3DPresenter(this);
			new ContainerAreaPresenter(this);
			new SelectedWidgetsPresenter(this);
			new WidgetsPivotMarkPresenter(this);
			new PointObjectsPresenter(this);
			new SplinePointPresenter(this);
			new TranslationGizmoPresenter(this);
			new BonePresenter(this);
			new BoneAsistantPresenter(this);
			new DistortionMeshPresenter(this);
			new FrameBorderPresenter(this);
			new InspectRootNodePresenter(this);
			new NineGridLinePresenter(this);
			new Animation2DPathPresenter(this);
			new WavePivotPresenter(this);
		}

		public void CreateNode(Type nodeType, ICommand command)
		{
			Components.Add(new CreateNodeRequestComponent { NodeType = nodeType, Command = command });
		}

		public void DuplicateSelectedNodes()
		{
			DuplicateNodes();
		}
	}

	public class CreateNodeRequestComponent : Component
	{
		public Type NodeType { get; set; }
		public ICommand Command { get; set; }

		public static bool Consume<T>(ComponentCollection<Component> components, out Type nodeType, out ICommand command) where T : Node
		{
			var c = components.Get<CreateNodeRequestComponent>();
			if (c != null && (c.NodeType.IsSubclassOf(typeof(T)) || c.NodeType == typeof(T))) {
				components.Remove<CreateNodeRequestComponent>();
				nodeType = c.NodeType;
				command = c.Command;
				return true;
			}
			nodeType = null;
			command = null;
			return false;
		}

		public static bool Consume<T>(ComponentCollection<Component> components) where T : Node
		{
			Type type;
			ICommand command;
			return Consume<T>(components, out type, out command);
		}

		public static bool Consume<T>(ComponentCollection<Component> components, out ICommand command) where T : Node
		{
			Type type;
			return Consume<T>(components, out type, out command);
		}
	}
}

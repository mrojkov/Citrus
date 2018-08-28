using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI.Docking;

namespace Tangerine.UI.SceneView
{
	public class SceneView : IDocumentView
	{
		private readonly FilesDropHandler filesDropHandler;
		private Vector2 mousePositionOnFilesDrop;

		// Given panel.
		public readonly Widget Panel;
		// Widget which is a direct child of the panel.
		public readonly Widget Frame;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		public readonly Widget InputArea;
		public WidgetInput Input => InputArea.Input;
		// Container for the document root node.
		public readonly SceneWidget Scene;
		public static readonly RulersWidget RulersWidget = new RulersWidget();
		public static readonly ZoomWidget ZoomWidget = new ZoomWidget();
		public static readonly ToolbarButton ShowNodeDecorationsPanelButton = new ToolbarButton {
			Tip = "Node decorations",
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
			ConnectCommand(SceneViewCommands.DragUp, () => DragNodes(new Vector2(0, -1)));
			ConnectCommand(SceneViewCommands.DragDown, () => DragNodes(new Vector2(0, 1)));
			ConnectCommand(SceneViewCommands.DragLeft, () => DragNodes(new Vector2(-1, 0)));
			ConnectCommand(SceneViewCommands.DragRight, () => DragNodes(new Vector2(1, 0)));
			ConnectCommand(SceneViewCommands.DragUpFast, () => DragNodes(new Vector2(0, -5)));
			ConnectCommand(SceneViewCommands.DragDownFast, () => DragNodes(new Vector2(0, 5)));
			ConnectCommand(SceneViewCommands.DragLeftFast, () => DragNodes(new Vector2(-5, 0)));
			ConnectCommand(SceneViewCommands.DragRightFast, () => DragNodes(new Vector2(5, 0)));
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

		static void DragNodes(Vector2 delta)
		{
			DragWidgets(delta);
			DragNodes3D(delta);
			DragSplinePoints3D(delta);
		}

		static void DragWidgets(Vector2 delta)
		{
			var containerWidget = Document.Current.Container as Widget;
			if (containerWidget != null) {
				var transform = containerWidget.CalcTransitionToSpaceOf(Instance.Scene).CalcInversed();
				var dragDelta = transform * delta - transform * Vector2.Zero;
				foreach (var widget in Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + dragDelta, CoreUserPreferences.Instance.AutoKeyframes);
				}
			}
		}

		static void DragNodes3D(Vector2 delta)
		{
			foreach (var node3D in Document.Current.SelectedNodes().Editable().OfType<Node3D>()) {
				Core.Operations.SetAnimableProperty.Perform(node3D, nameof(Widget.Position), node3D.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		static void DragSplinePoints3D(Vector2 delta)
		{
			foreach (var point in Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>()) {
				Core.Operations.SetAnimableProperty.Perform(point, nameof(Widget.Position), point.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		public SceneView(Widget panelWidget)
		{
			this.Panel = panelWidget;
			InputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			InputArea.FocusScope = new KeyboardFocusScope(InputArea);
			Scene = new SceneWidget {
				Nodes = { Document.Current.RootNode }
			};
			Frame = new Widget {
				Id = "SceneView",
				Nodes = { InputArea, Scene }
			};
			CreateComponents();
			CreateProcessors();
			CreatePresenters();
			filesDropHandler = new FilesDropHandler(InputArea);
			filesDropHandler.Handling += FilesDropOnHandling;
			filesDropHandler.NodeCreated += FilesDropOnNodeCreated;
			Scene.AddChangeWatcher(() => Document.Current.SlowMotion, v => Scene.AnimationSpeed = v ? 0.1f : 1);
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
			DockManager.Instance.AddFilesDropHandler(filesDropHandler);
		}

		public void Detach()
		{
			DockManager.Instance.RemoveFilesDropHandler(filesDropHandler);
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
				new DragWidgetsProcessor(),
				new DragPointObjectsProcessor(),
				new DragSplineTangentsProcessor(),
				new DragSplinePoint3DProcessor(),
				new DragAnimationPathPointProcessor(),
				new ResizeWidgetsProcessor(),
				new RescalePointObjectSelectionProcessor(),
				new RotatePointObjectSelectionProcessor(),
				new RotateWidgetsProcessor(),
				new RulerProcessor(),
				new DragNineGridLineProcessor(),
				new MouseSelectionProcessor(),
				new ShiftClickProcessor(),
				new PreviewAnimationProcessor(),
				new ResolutionPreviewProcessor()
			);
		}

		void CreatePresenters()
		{
			new Bone3DPresenter(this);
			new ContainerAreaPresenter(this);
			new WidgetsPivotMarkPresenter(this);
			new SelectedWidgetsPresenter(this);
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
		}

		public void CreateNode(Type nodeType, ICommand command)
		{
			Components.Add(new CreateNodeRequestComponent { NodeType = nodeType, Command = command });
		}

		public void DuplicateSelectedNodes()
		{
			DuplicateNodes();
		}

		public class SceneWidget : Widget
		{
			public override void Update(float delta)
			{
				if (Document.Current.PreviewAnimation) {
					base.Update(delta);
				}
			}
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

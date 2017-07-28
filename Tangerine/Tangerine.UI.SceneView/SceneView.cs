using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SceneView : IDocumentView
	{
		// Given panel.
		public readonly Widget Panel;
		// Widget which is a direct child of the panel.
		public readonly Widget Frame;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		public readonly Widget InputArea;
		public WidgetInput Input => InputArea.Input;
		// Container for the document root node.
		public readonly SceneWidget Scene;
		/// <summary>
		/// Gets the mouse position in the scene coordinates.
		/// </summary>
		public Vector2 MousePosition => Scene.Input.LocalMousePosition;

		public ComponentCollection<Component> Components = new ComponentCollection<Component>();

		public static SceneView Instance { get; private set; }

		public static void RegisterGlobalCommands()
		{
			var h = CommandHandlerList.Global;
			h.Connect(SceneViewCommands.PreviewAnimation, new PreviewAnimationHandler());
			h.Connect(SceneViewCommands.DragUp, () => DragNodes(new Vector2(0, -1)));
			h.Connect(SceneViewCommands.DragDown, () => DragNodes(new Vector2(0, 1)));
			h.Connect(SceneViewCommands.DragLeft, () => DragNodes(new Vector2(-1, 0)));
			h.Connect(SceneViewCommands.DragRight, () => DragNodes(new Vector2(1, 0)));
			h.Connect(SceneViewCommands.DragUpFast, () => DragNodes(new Vector2(0, -5)));
			h.Connect(SceneViewCommands.DragDownFast, () => DragNodes(new Vector2(0, 5)));
			h.Connect(SceneViewCommands.DragLeftFast, () => DragNodes(new Vector2(-5, 0)));
			h.Connect(SceneViewCommands.DragRightFast, () => DragNodes(new Vector2(5, 0)));
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
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + dragDelta);
				}
			}
		}

		static void DragNodes3D(Vector2 delta)
		{
			foreach (var node3D in Document.Current.SelectedNodes().Editable().OfType<Node3D>()) {
				Core.Operations.SetAnimableProperty.Perform(node3D, nameof(Widget.Position), node3D.Position + (Vector3)delta / 100);
			}
		}

		static void DragSplinePoints3D(Vector2 delta)
		{
			foreach (var point in Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>()) {
				Core.Operations.SetAnimableProperty.Perform(point, nameof(Widget.Position), point.Position + (Vector3)delta / 100);
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
		}

		public void Attach()
		{
			Instance = this;
			Panel.AddNode(Frame);
			DockManager.Instance.FilesDropped += DropFiles;
		}

		public void Detach()
		{
			DockManager.Instance.FilesDropped -= DropFiles;
			Instance = null;
			Frame.Unlink();
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

		void DropFiles(IEnumerable<string> files)
		{
			if (!InputArea.IsMouseOverThisOrDescendant()) {
				return;
			}
			var widgetPos = MousePosition * Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			Document.Current.History.BeginTransaction();
			try {
				foreach (var file in files) {
					try {
						string assetPath, assetType;
						if (Utils.ExtractAssetPathOrShowAlert(file, out assetPath, out assetType)) {
							if (assetType == ".png") {
								var node = Core.Operations.CreateNode.Perform(typeof(Image));
								var texture = new SerializableTexture(assetPath);
								Core.Operations.SetProperty.Perform(node, nameof(Image.Texture), texture);
								Core.Operations.SetProperty.Perform(node, nameof(Widget.Position), widgetPos);
								Core.Operations.SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
								Core.Operations.SetProperty.Perform(node, nameof(Widget.Size), (Vector2)texture.ImageSize);
							} else if (assetType == ".ogg") {
								var node = Core.Operations.CreateNode.Perform(typeof(Audio));
								var sample = new SerializableSample(assetPath);
								Core.Operations.SetProperty.Perform(node, nameof(Audio.Sample), sample);
								Core.Operations.SetProperty.Perform(node, nameof(Node.Id), Path.GetFileNameWithoutExtension(assetPath));
								Core.Operations.SetProperty.Perform(node, nameof(Audio.Volume), 1);
								var key = new Keyframe<AudioAction> {
									Frame = Document.Current.AnimationFrame,
									Value = AudioAction.Play
								};
								Core.Operations.SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
							} else if (assetType == ".tan" || assetType == ".model" || assetType == ".scene") {
								var scene = Node.CreateFromAssetBundle(assetPath);
								var node = Core.Operations.CreateNode.Perform(scene.GetType());
								Core.Operations.SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
								if (scene is Widget) {
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Position), widgetPos);
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Size), ((Widget)scene).Size);
								}
								node.LoadExternalScenes();
							}
						}
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
					}
				}
			} finally {
				Document.Current.History.EndTransaction();
			}
		}

		void CreateComponents() { }

		void CreateProcessors()
		{
			Frame.Tasks.Add(
				new CreateWidgetProcessor(),
				new CreatePointObjectProcessor(),
				new CreateSplinePoint3DProcessor(),
				new CreateNodeProcessor(),
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new DragPivotProcessor(),
				new DragWidgetsProcessor(),
				new DragPointObjectsProcessor(),
				new DragSplineTangentsProcessor(),
				new DragSplinePoint3DProcessor(),
				new ResizeWidgetsProcessor(),
				new RescalePointObjectSelectionProcessor(),
				new RotatePointObjectSelectionProcessor(),
				new RotateWidgetsProcessor(),
				new MouseSelectionProcessor(),
				new ShiftClickProcessor(),
				new PreviewAnimationProcessor()
			);
		}

		void CreatePresenters()
		{
			new ContainerAreaPresenter(this);
			new SelectedWidgetsPresenter(this);
			new PointObjectsSelectionPresenter(this);
			new TranslationGizmoPresenter(this);
		}

		public void CreateNode(Type nodeType)
		{
			Components.Add(new CreateNodeRequestComponent { NodeType = nodeType });
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

		public static bool Consume<T>(ComponentCollection<Component> components, out Type nodeType) where T: Node
		{
			var c = components.Get<CreateNodeRequestComponent>();
			if (c != null && (c.NodeType.IsSubclassOf(typeof(T)) || c.NodeType == typeof(T))) {
				components.Remove<CreateNodeRequestComponent>();
				nodeType = c.NodeType;
				return true;
			}
			nodeType = null;
			return false;
		}

		public static bool Consume<T>(ComponentCollection<Component> components) where T: Node
		{
			Type type;
			return Consume<T>(components, out type);
		}
	}
}
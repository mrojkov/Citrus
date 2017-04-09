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

		public ComponentCollection<IComponent> Components = new ComponentCollection<IComponent>();

		public static SceneView Instance { get; private set; }

		public static void RegisterGlobalCommands()
		{
			var h = CommandHandlerList.Global;
			h.Connect(SceneViewCommands.PreviewAnimation, new PreviewAnimationHandler());
			h.Connect(SceneViewCommands.DragUp, () => DragWidgets(new Vector2(0, -1)));
			h.Connect(SceneViewCommands.DragDown, () => DragWidgets(new Vector2(0, 1)));
			h.Connect(SceneViewCommands.DragLeft, () => DragWidgets(new Vector2(-1, 0)));
			h.Connect(SceneViewCommands.DragRight, () => DragWidgets(new Vector2(1, 0)));
			h.Connect(SceneViewCommands.DragUpFast, () => DragWidgets(new Vector2(0, -5)));
			h.Connect(SceneViewCommands.DragDownFast, () => DragWidgets(new Vector2(0, 5)));
			h.Connect(SceneViewCommands.DragLeftFast, () => DragWidgets(new Vector2(-5, 0)));
			h.Connect(SceneViewCommands.DragRightFast, () => DragWidgets(new Vector2(5, 0)));
		}

		static void DragWidgets(Vector2 delta)
		{
			var transform = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(Instance.Scene).CalcInversed();
			var dragDelta = transform * delta - transform * Vector2.Zero;
			foreach (var widget in Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + dragDelta);
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

		void DropFiles(IEnumerable<string> files)
		{
			if (!InputArea.IsMouseOverThisOrDescendant()) {
				return;
			}
			var widgetPos = MousePosition * Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			Document.Current.History.BeginTransaction();
			try {
				foreach (var file in files) {
					string assetPath, assetType;
					if (Utils.ExtractAssetPathOrShowAlert(file, out assetPath, out assetType)) {
						if (assetType == ".png") {
							var node = Core.Operations.CreateNode.Perform(typeof(Image));
							var texture = new SerializableTexture(assetPath);
							Core.Operations.SetProperty.Perform(node, nameof(Image.Texture), texture);
							Core.Operations.SetProperty.Perform(node, nameof(Widget.Position), widgetPos);
							Core.Operations.SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
							Core.Operations.SetProperty.Perform(node, nameof(Widget.Size), (Vector2)texture.ImageSize);
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
				}
			} finally {
				Document.Current.History.EndTransaction();
			}
		}

		void CreateComponents()
		{
			Components.Add(new ExpositionComponent());
		}

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
				new ResizeWidgetsProcessor(),
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

	public class CreateNodeRequestComponent : IComponent
	{
		public Type NodeType { get; set; }

		public static bool Consume<T>(ComponentCollection<IComponent> components, out Type nodeType) where T: Node
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

		public static bool Consume<T>(ComponentCollection<IComponent> components) where T: Node
		{
			Type type;
			return Consume<T>(components, out type);
		}
	}
}
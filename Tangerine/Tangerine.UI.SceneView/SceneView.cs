using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SceneView : Entity, IDocumentView
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

		public static SceneView Instance { get; private set; }

		static SceneView()
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
			var transform = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(SceneView.Instance.Scene).CalcInversed();
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
		}

		public void Detach()
		{
			Instance = null;
			Frame.Unlink();
		}

		void CreateComponents()
		{
			Components.Add(new ExpositionComponent());
		}

		void CreateProcessors()
		{
			Frame.Tasks.Add(
				new CreateWidgetProcessor(),
				new CreateNodeProcessor(),
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new DragPivotProcessor(),
				new DragWidgetsProcessor(),
				new DragPointObjectsProcessor(),
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
			new PointObjectsPresenter(this);
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
	}
}
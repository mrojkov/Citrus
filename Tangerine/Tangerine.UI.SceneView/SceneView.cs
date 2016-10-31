using System;
using System.Collections.Generic;
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
				new SceneViewDecorator(),
				new CreateWidgetProcessor(),
				new CreateNodeProcessor(),
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new SelectedWidgetsPresenter(),
				new DragPivotProcessor(),
				new DragWidgetsProcessor(),
				new ResizeWidgetsProcessor(),
				new RotateWidgetsProcessor(),
				new MouseSelectionProcessor(),
				new ShiftClickProcessor(),
				new WASDProcessor()
			);
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
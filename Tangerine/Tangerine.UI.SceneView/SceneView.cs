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
		private readonly Widget inputArea;
		public WidgetInput Input => inputArea.Input;
		// Container for the document root node.
		public readonly Widget Scene;
		/// <summary>
		/// Gets the mouse position in the scene coordinates.
		/// </summary>
		public Vector2 MousePosition => Scene.Input.LocalMousePosition;

		public static SceneView Instance { get; private set; }

		public SceneView(Widget panelWidget)
		{
			this.Panel = panelWidget;
			inputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			inputArea.FocusScope = new KeyboardFocusScope(inputArea);
			Scene = new Widget {
				Nodes = { Document.Current.RootNode }
			};
			Frame = new Widget {
				Id = "SceneView",
				Nodes = { inputArea, Scene }
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
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new SelectedWidgetsPresenter(),
				new DragWidgetsProcessor(),
				new ResizeWidgetsProcessor(),
				new MouseSelectionProcessor(),
				new WASDProcessor()
			);
		}
	}
}
using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SceneView : Entity, IDocumentView
	{
		// Given panel.
		private readonly Widget panelWidget;
		// Widget which is a direct child of the panel.
		public readonly Widget Frame;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		private readonly Widget inputArea;
		public WidgetInput Input => inputArea.Input;
		// Root node of the document.
		public readonly Widget Scene;
		/// <summary>
		/// Gets the mouse position in the scene coordinates.
		/// </summary>
		public Vector2 MousePosition => Scene.Input.LocalMousePosition;

		public static SceneView Instance { get; private set; }

		public SceneView(Widget panelWidget)
		{
			this.panelWidget = panelWidget;
			inputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
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
			panelWidget.AddNode(Frame);
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
				new MouseScrollProcessor(),
				new SelectedWidgetsPresenter(),
				new ResizeProcessor(),
				new MouseSelectionProcessor()
			);
		}
	}
}
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
		public readonly Widget Root;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		private readonly Widget inputArea;
		public WidgetInput Input => inputArea.Input;
		// Canvas which is meant to be scrollable and zoomable widget.
		public readonly Widget Canvas;
		/// <summary>
		/// Gets the mouse position in the canvas space.
		/// </summary>
		public Vector2 CanvasMousePosition => Canvas.Input.LocalMousePosition;

		public static SceneView Instance { get; private set; }

		public SceneView(Widget panelWidget)
		{
			this.panelWidget = panelWidget;
			inputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			Canvas = new Widget {
				Nodes = { Document.Current.RootNode }
			};
			Root = new Widget {
				Id = "SceneView",
				Nodes = { inputArea, Canvas }
			};
			CreateComponents();
			CreateProcessors();
		}

		public void Attach()
		{
			Instance = this;
			panelWidget.AddNode(Root);
		}

		public void Detach()
		{
			Instance = null;
			Root.Unlink();
		}

		void CreateComponents()
		{
			Components.Add(new ExpositionComponent());
		}

		void CreateProcessors()
		{
			Root.Tasks.Add(
				new MouseScrollProcessor(),
				new SelectedWidgetsPresenter(),
				new ResizeProcessor(),
				new MouseSelectionProcessor()
			);
		}
	}
}
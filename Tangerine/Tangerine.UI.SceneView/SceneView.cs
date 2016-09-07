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
		public readonly Widget RootWidget;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		public readonly Widget InputArea;
		// Canvas which is meant to be scrollable and zoomable widget.
		public readonly Widget CanvasWidget;

		public static SceneView Instance { get; private set; }

		public SceneView(Widget panelWidget)
		{
			this.panelWidget = panelWidget;
			InputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			CanvasWidget = new Widget {
				Nodes = { Document.Current.RootNode }
			};
			RootWidget = new Widget {
				Id = "SceneView",
				Nodes = { InputArea, CanvasWidget }
			};
			CreateComponents();
			CreateProcessors();
		}

		public void Attach()
		{
			Instance = this;
			panelWidget.AddNode(RootWidget);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}

		void CreateComponents()
		{
			Components.Add(new ExpositionComponent());
		}

		void CreateProcessors()
		{
			RootWidget.Tasks.Add(
				new MouseScrollProcessor(),
				new SelectedWidgetsPresenter()
			);
		}
	}
}
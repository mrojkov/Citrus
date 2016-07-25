using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class SceneView : IDocumentView
	{
		private readonly Widget panelWidget; // Given panel.
		private readonly Widget rootWidget; // Widget which is a direct child of the panel.
		public readonly Widget InputScreen; // Widget having the same size as panel, used for catching mouse input above the canvas.
		public readonly Widget CanvasWidget; // Canvas which is meant to be scrollable and zoomable widget.

		public static SceneView Instance { get; private set; }

		public SceneView(Widget panelWidget)
		{
			this.panelWidget = panelWidget;
			InputScreen = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			CanvasWidget = new Widget {
				Nodes = { Document.Current.RootNode }
			};
			rootWidget = new Widget {
				Id = "SceneView",
				Nodes = { InputScreen, CanvasWidget }
			};
			// Document.Current.RootNode.PostPresenter = new WidgetBoundsPresenter(Color4.Green);
			CreateProcessors();
		}

		public void Attach()
		{
			Instance = this;
			panelWidget.AddNode(rootWidget);
		}

		public void Detach()
		{
			Instance = null;
			rootWidget.Unlink();
		}

		void CreateProcessors()
		{
			rootWidget.Tasks.Add(new IProcessor[] {
				new MouseScrollProcessor()
			});
		}
	}
}

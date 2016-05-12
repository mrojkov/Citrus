using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RollPane
	{
		public readonly Widget ContentWidget;
		public readonly Widget RootWidget;
		public readonly Widget OverlayWidget;
		public event Action<Widget> OnRenderOverlay;

		public RollPane()
		{
			RootWidget = new Frame {
				MinWidth = Metrics.ToolbarMinWidth,
				ClipChildren = ClipMethod.ScissorTest,
			};
			ContentWidget = new Widget {
				Padding = new Thickness { Top = 1, Bottom = 1 },
				Width = RootWidget.Width,
				Height = 0,
				Anchors = Anchors.LeftRight,
				Layout = new VBoxLayout { Spacing = 1 },
				Presenter = new DelegatePresenter<Node>(RenderBackground)
			};
			OverlayWidget = new Widget { Presenter = new DelegatePresenter<Widget>(w => OnRenderOverlay?.Invoke(w)) };
			RootWidget.AddNode(OverlayWidget);
			RootWidget.AddNode(ContentWidget);
			ContentWidget.Updating += delta => ContentWidget.Y = -Timeline.Instance.ScrollOrigin.Y;
		}
				
		private void RenderBackground(Node node)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, Colors.GridLines);
		}
	}
}

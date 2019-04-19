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
				Id = nameof(RollPane),
				Layout = new StackLayout { VerticallySizeable = true },
				ClipChildren = ClipMethod.ScissorTest,
				HitTestTarget = true,
			};
			ContentWidget = new Widget {
				Padding = new Thickness { Top = 1, Bottom = 1 },
				Width = RootWidget.Width,
				Height = 0,
				Anchors = Anchors.LeftRight,
				Layout = new VBoxLayout { Spacing = TimelineMetrics.RowSpacing },
				Presenter = new SyncDelegatePresenter<Node>(RenderBackground)
			};
			OverlayWidget = new Widget { Presenter = new SyncDelegatePresenter<Widget>(w => OnRenderOverlay?.Invoke(w)) };
			RootWidget.AddNode(OverlayWidget);
			RootWidget.AddNode(ContentWidget);
			ContentWidget.Updating += _ => ContentWidget.Y = -Timeline.Instance.Offset.Y;
			RootWidget.Gestures.Add(new ClickGesture(1, ShowContextMenu));
		}

		private void ShowContextMenu()
		{
			if (Document.Current.Animation.IsCompound) {
				new Menu {
					new Command("Add", () => Components.RollAnimationTrackView.AddAnimationTrack())
				}.Popup();
			}
		}

		private void RenderBackground(Node node)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ColorTheme.Current.TimelineRoll.Lines);
		}
	}
}

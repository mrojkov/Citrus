#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedDefaultWindowWidget : DefaultWindowWidget
	{
		public override bool IsNotDecorated() => false;

		public ThemedDefaultWindowWidget(IWindow window) : base(window)
		{
			CompoundPresenter.Push(new WindowWidgetPresenter());
		}
	}

	[YuzuDontGenerateDeserializer]
	public class ThemedInvalidableWindowWidget : InvalidableWindowWidget
	{
		public override bool IsNotDecorated() => false;

		public ThemedInvalidableWindowWidget(IWindow window) : base(window)
		{
			CompoundPresenter.Push(new WindowWidgetPresenter());
		}
	}

	[YuzuDontGenerateDeserializer]
	internal class WindowWidgetPresenter : IPresenter
	{
		public IPresenter Clone() => this;

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = (Widget)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Size = widget.Size;
			ro.Color = Theme.Colors.GrayBackground;
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Size;
			public Color4 Color;

			public override void Render()
			{
				PrepareRenderState();
				Renderer.DrawRect(Vector2.Zero, Size, Color);
			}
		}
	}
}
#endif

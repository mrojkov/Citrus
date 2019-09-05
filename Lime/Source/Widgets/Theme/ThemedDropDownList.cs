#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedDropDownList : DropDownList
	{
		public override bool IsNotDecorated() => false;

		public ThemedDropDownList()
		{
			MinSize = Theme.Metrics.DefaultButtonSize;
			MaxHeight = Theme.Metrics.DefaultButtonSize.Y;
			TabTravesable = new TabTraversable();
			var text = new ThemedSimpleText {
				Id = "TextWidget",
				VAlignment = VAlignment.Center,
			};
			CompoundPresenter.Add(new DropDownListPresenter());
			PostPresenter = new Theme.MouseHoverBorderPresenter();
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
			Padding = Theme.Metrics.ControlsPadding;
			AddNode(text);
			text.ExpandToContainerWithAnchors();
			text.X += 4;
			text.Width -= DropDownListPresenter.IconWidth + 4;
		}

		internal class DropDownListPresenter : IPresenter
		{
			public const float IconWidth = 20;

			static readonly VectorShape separator = new VectorShape {
				new VectorShape.Line(0, 0.1f, 0, 0.9f, Theme.Colors.ControlBorder, 0.05f),
			};
			static readonly VectorShape icon = new VectorShape {
				new VectorShape.Line(0.5f, 0.6f, 0.7f, 0.4f, Color4.White.Transparentify(0.2f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.6f, 0.3f, 0.4f, Color4.White.Transparentify(0.2f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.55f, 0.7f, 0.35f, Color4.White.Transparentify(0.5f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.55f, 0.3f, 0.35f, Color4.White.Transparentify(0.5f), 0.04f, false),
			};

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var dd = (CommonDropDownList)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(dd);
				ro.Size = dd.Size;
				ro.Gradient = dd.GloballyEnabled ? Theme.Colors.ButtonDefault : Theme.Colors.ButtonDisable;
				ro.BorderColor = Theme.Colors.ControlBorder;
				ro.IconColor = dd.Items.Count > 0 ? Theme.Colors.BlackText : Theme.Colors.GrayText;
				return ro;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public ColorGradient Gradient;
				public Color4 BorderColor;
				public Color4 IconColor;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawVerticalGradientRect(Vector2.Zero, Size, Gradient);
					Renderer.DrawRectOutline(Vector2.Zero, Size, BorderColor);
					var transform = Matrix32.Scaling(IconWidth, Size.Y) * Matrix32.Translation(Size.X - IconWidth, 0);
					separator.Draw(transform);
					icon.Draw(transform, IconColor);
				}
			}
		}
	}
}
#endif

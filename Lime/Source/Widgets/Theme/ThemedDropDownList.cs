#if !ANDROID && !iOS
using System;

namespace Lime
{
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
			CompoundPresenter.Add(new DropDownListPresenter(this));
			PostPresenter = new Theme.MouseHoverBorderPresenter();
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
			Padding = Theme.Metrics.ControlsPadding;
			AddNode(text);
			text.ExpandToContainerWithAnchors();
			text.X += 4;
			text.Width -= DropDownListPresenter.IconWidth + 4;
		}

		internal class DropDownListPresenter : CustomPresenter
		{
			public const float IconWidth = 20;

			readonly CommonDropDownList list;
			readonly VectorShape separator = new VectorShape {
				new VectorShape.Line(0, 0.1f, 0, 0.9f, Theme.Colors.ControlBorder, 0.05f),
			};
			readonly VectorShape icon = new VectorShape {
				new VectorShape.Line(0.5f, 0.6f, 0.7f, 0.4f, Color4.White.Transparentify(0.2f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.6f, 0.3f, 0.4f, Color4.White.Transparentify(0.2f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.55f, 0.7f, 0.35f, Color4.White.Transparentify(0.5f), 0.04f, false),
				new VectorShape.Line(0.5f, 0.55f, 0.3f, 0.35f, Color4.White.Transparentify(0.5f), 0.04f, false),
			};

			public DropDownListPresenter(CommonDropDownList list)
			{
				this.list = list;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Theme.Colors.ButtonDefault);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Theme.Colors.ControlBorder);
				var transform = Matrix32.Scaling(IconWidth, widget.Height) * Matrix32.Translation(widget.Width - IconWidth, 0);
				separator.Draw(transform);
				icon.Draw(transform, list.Items.Count > 0 ? Theme.Colors.BlackText : Theme.Colors.GrayText);
			}
		}
	}
}
#endif

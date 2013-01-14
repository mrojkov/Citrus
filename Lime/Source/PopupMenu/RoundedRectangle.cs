using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class RoundedRectangle : Widget
	{
		public float CornerRadius = 25;
		public Color4 InnerColor = Color4.White;
		public Color4 OuterColor = Color4.Black;

		private Polyline outerFrame = new Polyline();
		private Polyline innerFrame = new Polyline();
		private Image innerRect = new Image();

		public RoundedRectangle()
		{
			this.AddNode(innerRect);
			this.AddNode(innerFrame);
			this.AddNode(outerFrame);
		}

		public override void Update(int delta)
		{
			SetupInnerRect();
			SetupBorder(outerFrame, Width, Height, CornerRadius, -2);
			SetupBorder(innerFrame, Width, Height, CornerRadius, 0);
			innerFrame.Color = InnerColor;
			outerFrame.Color = OuterColor;
			base.Update(delta);
		}

		private void SetupInnerRect()
		{
			innerRect.Blending = Lime.Blending.Silhuette;
			innerRect.Position = CornerRadius * Vector2.One;
			innerRect.Size = Size - 2 * CornerRadius * Vector2.One;
			innerRect.Color = InnerColor;
		}

		private static void SetupBorder(Polyline border, float width, float height,
			float cornerRadius, float padding)
		{
			padding += cornerRadius;
			border.Thickness = cornerRadius * 2;
			border.Points = new List<Vector2> {
				new Vector2(padding, padding),
				new Vector2(width - padding, padding),
				new Vector2(width - padding, height - padding),
				new Vector2(padding, height - padding),
				new Vector2(padding, padding)
			};
		}
	}
}

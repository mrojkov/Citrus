using System;
using System.Collections.Generic;

namespace Lime
{
	internal interface IVectorShapeElement
	{
		void Draw();
	}

	// This class should be removed after NanoVG integration.
	internal class VectorShape : List<IVectorShapeElement>
	{
		public void Draw()
		{
			Draw(Matrix32.Identity);
		}

		public void Draw(Matrix32 transform)
		{
			var t = Renderer.Transform1;
			Renderer.Transform1 = transform * t;
			foreach (var i in this) {
				i.Draw();
			}
			Renderer.Transform1 = t;
		}

		public class Line : IVectorShapeElement
		{
			public Vector2 A;
			public Vector2 B;
			public float Thickness;
			public Color4 Color;

			public Line() { }
			public Line(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1)
			{
				A = new Vector2(x0, y0);
				B = new Vector2(x1, y1);
				Color = color;
				Thickness = thickness;
			}

			public void Draw()
			{
				Renderer.DrawLine(A, B, Color, Thickness);
			}
		}
	}
}


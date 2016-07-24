using System;
using System.Collections.Generic;

namespace Lime
{
	internal interface IVectorShapeElement
	{
		void Draw(Color4 tint);
	}

	// This class should be removed after NanoVG integration.
	internal class VectorShape : List<IVectorShapeElement>
	{
		public void Draw()
		{
			Draw(Matrix32.Identity, Color4.White);
		}

		public void Draw(Matrix32 transform)
		{
			Draw(transform, Color4.White);
		}

		public void Draw(Matrix32 transform, Color4 tint)
		{
			var t = Renderer.Transform1;
			Renderer.Transform1 = transform * t;
			foreach (var i in this) {
				i.Draw(tint);
			}
			Renderer.Transform1 = t;
		}

		public class Line : IVectorShapeElement
		{
			public Vector2 A;
			public Vector2 B;
			public float Thickness;
			public Color4 Color;
			public bool Antialiased;

			public Line() { }
			public Line(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1, bool antialiased = true)
			{
				A = new Vector2(x0, y0);
				B = new Vector2(x1, y1);
				Color = color;
				Thickness = thickness;
				Antialiased = antialiased;
			}

			public void Draw(Color4 tint)
			{
				var c = Color * tint;
				if (Antialiased) {
					Renderer.DrawLine(A, B, c.Transparentify(0.75f), Thickness * 1.5f);
					Renderer.DrawLine(A, B, c.Transparentify(0.5f), Thickness * 1.25f);
				}
				Renderer.DrawLine(A, B, c, Thickness);
			}
		}
	}
}


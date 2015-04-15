using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class SpriteList
	{
		private class Sprite
		{
			public int Tag;
			public ITexture Texture;
			public Color4 Color;
			public Vector2 UV0;
			public Vector2 UV1;
			public Vector2 Position;
			public Vector2 Size;

			public void Draw(Color4 color)
			{
				Renderer.DrawSprite(Texture, Color * color, Position, Size, UV0, UV1);
			}
		}

		private List<Sprite> items = new List<Sprite>();

		public void Reserve(int count)
		{
			items.Capacity += count;
		}

		public void Add(
			ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1, int tag)
		{
			items.Add(new Sprite() {
				Texture = texture,
				Color = color,
				Position = position,
				Size = size,
				UV0 = UV0,
				UV1 = UV1,
				Tag = tag
			});
		}

		public void Render()
		{
			Render(Color4.White);
		}

		public void Render(Color4 color)
		{
			foreach (var s in items) {
				s.Draw(color);
			}
		}

		public bool HitTest(Matrix32 worldTransform, Vector2 point, out int tag)
		{
			point = worldTransform.CalcInversed().TransformVector(point);
			foreach (var s in items) {
				var a = s.Position;
				var b = s.Position + s.Size;
				if (point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y) {
					tag = s.Tag;
					return true;
				}
			}
			tag = -1;
			return false;
		}
	}
}

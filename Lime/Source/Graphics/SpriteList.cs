using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Sprite
	{
		public int Tag;
		public ITexture Texture;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public Vector2 Position;
		public Vector2 Size;
	}

	public class SpriteList : List<Sprite>
	{
		public void Reserve(int count)
		{
			Capacity += count;
		}

		public void Add(
			ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1, int tag)
		{
			Add(new Sprite() {
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
			Renderer.DrawSpriteList(this, color);
		}

		public bool HitTest(Matrix32 worldTransform, Vector2 point, out int tag)
		{
			point = worldTransform.CalcInversed().TransformVector(point);
			foreach (var s in this) {
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

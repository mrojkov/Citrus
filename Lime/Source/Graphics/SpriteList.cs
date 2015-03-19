using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class SpriteList
	{
		public class Item
		{
			public int Tag;
			public ITexture Texture;
			public Color4 Color;
			public Vector2 UV0;
			public Vector2 UV1;
			public Vector2 Position;
			public Vector2 Size;
			internal Item Next;

			public void Draw(Color4 color)
			{
				Renderer.DrawSprite(Texture, Color * color, Position, Size, UV0, UV1);
			}
		}

		private Item head;
		private Item tail;

		public void Add(Item item)
		{
			if (head == null) {
				head = item;
				tail = item;
			} else {
				tail.Next = item;
				tail = item;
			}
		}

		public void Add(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1, int tag)
		{
			Add(new Item() {
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
			for (var item = head; item != null; item = item.Next) {
				item.Draw(color);
			}
		}

		public bool HitTest(Matrix32 worldTransform, Vector2 point, out int tag)
		{
			point = worldTransform.CalcInversed().TransformVector(point);
			for (var item = head; item != null; item = item.Next) {
				var a = item.Position;
				var b = item.Position + item.Size;
				if (point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y) {
					tag = item.Tag;
					return true;
				}
			}
			tag = -1;
			return false;
		}
	}
}

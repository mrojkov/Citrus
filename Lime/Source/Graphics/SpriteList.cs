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

		public void Add(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1)
		{
			Add(new Item() {
				Texture = texture,
				Color = color,
				Position = position,
				Size = size,
				UV0 = UV0,
				UV1 = UV1
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
	}
}

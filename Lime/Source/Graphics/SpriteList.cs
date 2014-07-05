using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class SpriteList : IDisposable
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

		static Item pool;
		Item head;
		Item tail;

		~SpriteList()
		{
			Dispose();
		}

		public Item Add()
		{
			var item = AllocateItem();
			if (head == null) {
				head = item;
				tail = item;
			} else {
				tail.Next = item;
				tail = item;
			}
			return item;
		}

		public Item Add(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1)
		{
			var item = Add();
			item.Texture = texture;
			item.Color = color;
			item.Position = position;
			item.Size = size;
			item.UV0 = UV0;
			item.UV1 = UV1;
			return item;
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

		public void Dispose()
		{
			if (head != null) {
				tail.Next = pool;
				pool = head;
				head = null;
				tail = null;
			}
		}

		private static Item AllocateItem()
		{
			if (pool == null) {
				return new Item();
			} else {
				var item = pool;
				pool = pool.Next;
				item.Next = null;
				return item;
			}
		}
	}
}

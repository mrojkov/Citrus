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

	public class SpriteList
	{
		private interface ISpriteWrapper
		{
			IEnumerable<Sprite> GetSprites();
			bool HitTest(Vector2 point, out int tag);
		}


		private class NormalSprite : Sprite, ISpriteWrapper
		{
			public IEnumerable<Sprite> GetSprites()
			{
				yield return this;
			}

			public bool HitTest(Vector2 point, out int tag)
			{
				var a = Position;
				var b = Position + Size;
				tag = Tag;
				return point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y;
			}
		}

		public struct CharDef
		{
			public FontChar FontChar;
			public Vector2 Position;

			public Vector2 Size(float fontHeight)
			{
				return new Vector2(fontHeight / FontChar.Height * FontChar.Width, fontHeight);
			}
		}

		private class TextSprite : ISpriteWrapper
		{
			public int Tag;
			public Color4 Color;
			public Font Font;
			public float FontHeight;
			public CharDef[] CharDefs;

			public IEnumerable<Sprite> GetSprites()
			{
				foreach (var cd in CharDefs) {
					var ch = cd.FontChar;
					yield return new Sprite {
						Tag = Tag,
						Texture = Font.Textures[ch.TextureIndex],
						Color = this.Color,
						Position = cd.Position,
						Size = cd.Size(FontHeight),
						UV0 = ch.UV0,
						UV1 = ch.UV1,
					};
				}
			}

			public bool HitTest(Vector2 point, out int tag)
			{
				foreach (var cd in CharDefs) {
					var a = cd.Position;
					var b = cd.Position + cd.Size(FontHeight);
					if (point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y) {
						tag = Tag;
						return true;
					}
				}
				tag = -1;
				return false;
			}
		}

		private List<ISpriteWrapper> items = new List<ISpriteWrapper>();

		public void Add(
			ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1, int tag)
		{
			items.Add(new NormalSprite() {
				Texture = texture,
				Color = color,
				Position = position,
				Size = size,
				UV0 = UV0,
				UV1 = UV1,
				Tag = tag,
			});
		}

		public void Add(Font font, Color4 color, float fontHeight, CharDef[] charDefs, int tag)
		{
			items.Add(new TextSprite {
				Font = font,
				Color = color,
				FontHeight = fontHeight,
				CharDefs = charDefs,
				Tag = tag,
			});
		}

		public void Render()
		{
			Render(Color4.White);
		}

		private static Sprite sentinel = new Sprite();

		private IEnumerable<Sprite> GetSprites()
		{
			foreach (var w in items)
				foreach (var s in w.GetSprites())
					yield return s;
			yield return sentinel;
		}

		public void Render(Color4 color)
		{
			Renderer.DrawSpriteList(GetSprites(), color);
		}

		public bool HitTest(Matrix32 worldTransform, Vector2 point, out int tag)
		{
			point = worldTransform.CalcInversed().TransformVector(point);
			foreach (var s in items) {
				if (s.HitTest(point, out tag)) {
					return true;
				}
			}
			tag = -1;
			return false;
		}
	}
}

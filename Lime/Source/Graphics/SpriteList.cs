using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Sprite
	{
		public int Tag;
		public ITexture Texture1;
		public ITexture Texture2;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public Vector2 Position;
		public Vector2 Size;
		public ShaderProgram ShaderProgram;
		/// <summary>
		/// Sets specific blending mode for that sprite, it is used to batch two pass lcd text rendering,
		/// but not like with Darken
		/// </summary>
		public Blending? Blending;
		public bool ForceOverrideColor;
	}

	public class SpriteList
	{
		private interface ISpriteWrapper
		{
			void AddToList(List<Sprite> sprites);
			bool HitTest(Vector2 point, out int tag);
		}

		private class NormalSprite : Sprite, ISpriteWrapper
		{
			public void AddToList(List<Sprite> sprites)
			{
				sprites.Add(this);
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
			public IFont Font;
			public float FontHeight;
			public CharDef[] CharDefs;
			private static Sprite[] buffer = new Sprite[50];
			public static int Index = 0;

			public void AddToList(List<Sprite> sprites)
			{
				bool hasRgbIntensity = false;
				foreach (CharDef cd in CharDefs) {
					FontChar ch = cd.FontChar;
					if (Index >= buffer.Length) {
						Array.Resize(ref buffer, (int)(buffer.Length * 1.5));
					}

					if (buffer[Index] == null) buffer[Index] = new Sprite();

					Sprite s = buffer[Index];
					Index++;
					s.Tag = Tag;
					s.Texture1 = ch.Texture;
					s.Texture2 = null;
					s.Position = cd.Position;
					s.Size = cd.Size(FontHeight);
					s.UV0 = ch.UV0;
					s.UV1 = ch.UV1;
					s.ShaderProgram = null;
					if (cd.FontChar.RgbIntensity) {
						hasRgbIntensity = true;
						s.Color = Color4.White;
						s.ForceOverrideColor = true;
						s.Blending = Blending.LcdTextFirstPass;
					} else {
						s.Color = Color;
						s.ForceOverrideColor = false;
						s.Blending = null;
					}
					sprites.Add(s);
				}

				if (hasRgbIntensity) {
					foreach (CharDef cd in CharDefs) {
						if (!cd.FontChar.RgbIntensity) continue;

						FontChar ch = cd.FontChar;
						if (Index >= buffer.Length) {
							Array.Resize(ref buffer, (int) (buffer.Length * 1.5));
						}

						if (buffer[Index] == null) buffer[Index] = new Sprite();

						Sprite s = buffer[Index];
						Index++;
						s.Tag = Tag;
						s.Texture1 = ch.Texture;
						s.Texture2 = null;
						s.Color = Color;
						s.ForceOverrideColor = false;
						s.Position = cd.Position;
						s.Size = cd.Size(FontHeight);
						s.UV0 = ch.UV0;
						s.UV1 = ch.UV1;
						s.ShaderProgram = null;
						s.Blending = Blending.LcdTextSecondPass;
						sprites.Add(s);
					}
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
				Texture1 = texture,
				Color = color,
				Position = position,
				Size = size,
				UV0 = UV0,
				UV1 = UV1,
				Tag = tag,
			});
		}

		public void Add(IFont font, Color4 color, float fontHeight, CharDef[] charDefs, int tag)
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

		private static List<Sprite> sprites = new List<Sprite>();

		public void Render(Color4 color)
		{
			TextSprite.Index = 0;
			foreach (var w in items) {
				w.AddToList(sprites);
			}
			Renderer.DrawSpriteList(sprites, color);
			sprites.Clear();
		}

		public void Render(Color4 color, Action<Sprite> spritePostProcessor)
		{
			TextSprite.Index = 0;
			foreach (var w in items) {
				w.AddToList(sprites);
			}
			foreach (var s in sprites) {
				spritePostProcessor(s);
			}
			Renderer.DrawSpriteList(sprites, color);
			sprites.Clear();
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

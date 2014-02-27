using System.Runtime.InteropServices;
using Lime;
using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	[ProtoContract]
	public enum HAlignment
	{
		[ProtoEnum]
		Left,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Right,
	}

	[ProtoContract]
	public enum VAlignment
	{
		[ProtoEnum]
		Top,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Bottom,
	}

	[ProtoContract]
	public enum TextOverflowMode
	{
		[ProtoEnum]
		Default,
		[ProtoEnum]
		Minify,
		[ProtoEnum]
		Ellipsis,
	}

	[ProtoContract]
	public sealed class SimpleText : Widget
	{
		private Renderer.SpriteList spriteList;
		private SerializableFont font;
		private string text;
		private float fontHeight;
		private float spacing;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private Vector2 prevSize;

		[ProtoMember(1)]
		public SerializableFont Font {
			get { return font; }
			set { SetFont(value); }
		}

		[ProtoMember(2)]
		public override string Text {
			get { return text; }
			set { SetText(value); }
		}

		[ProtoMember(3)]
		public float FontHeight {
			get { return fontHeight; }
			set { SetFontHeight(value); }
		}

		[ProtoMember(4)]
		public float Spacing {
			get { return spacing; }
			set { SetSpacing(value); }
		}

		[ProtoMember(5)]
		public HAlignment HAlignment {
			get { return hAlignment; }
			set { SetHAlignment(value); }
		}

		[ProtoMember(6)]
		public VAlignment VAlignment {
			get { return vAlignment; }
			set { SetVAlignment(value); }
		}

		[ProtoMember(8)]
		public TextOverflowMode OverflowMode { get; set; }

		public SimpleText()
		{
			Text = "";
			FontHeight = 15;
			Font = new SerializableFont();
		}

		public override Vector2 CalcContentSize()
		{
			return Renderer.MeasureTextLine(Font.Instance, Text, FontHeight);
		}

		public override void Render()
		{
			if (prevSize != Size) {
				DisposeSpriteList();
				prevSize = Size;
			}
			if (spriteList == null) {
				if (OverflowMode == TextOverflowMode.Minify) {
					FitTextInsideWidgetArea();
				}
				spriteList = new Renderer.SpriteList();
				Vector2 extent;
				RenderHelper(spriteList, out extent);
			}
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			spriteList.Render(GlobalColor);
		}

		public Vector2 MeasureText()
		{
			Vector2 extent;
			RenderHelper(null, out extent);
			return extent;
		}

		public void FitTextInsideWidgetArea(float minFontHeight = 10)
		{
			var minH = minFontHeight;
			var maxH = FontHeight;
			if (maxH <= minH) {
				return;
			}
			var spacingKoeff = Spacing / FontHeight;
			while (maxH - minH > 1) {
				FontHeight = (minH + maxH) / 2;
				Spacing = FontHeight * spacingKoeff;
				var extent = MeasureText();
				var fit = (extent.X < Width && extent.Y < Height);
				if (fit) {
					minH = FontHeight;
				} else {
					maxH = FontHeight;
				}
			}
		}

		private void RenderHelper(Renderer.SpriteList spriteList, out Vector2 extent)
		{
			extent = Vector2.Zero;
			var localizedText = Localization.GetString(Text);
			if (string.IsNullOrEmpty(localizedText)) {
				return;
			}
			var lines = SplitText(localizedText);
			var pos = Vector2.Down * CalcVerticalTextPosition(lines);
			foreach (var line in lines) {
				RenderSingleTextLine(spriteList, ref extent, ref pos, line);
			}
			extent.Y = lines.Count * (FontHeight + Spacing);
			if (extent.Y > 0) {
				extent.Y -= Spacing;
			}
		}

		private float CalcVerticalTextPosition(List<string> lines)
		{
			var totalHeight = CalcTotalHeight(lines.Count);
			if (VAlignment == VAlignment.Bottom) {
				return Size.Y - totalHeight;
			} else if (VAlignment == VAlignment.Center) {
				return (Size.Y - totalHeight) * 0.5f;
			}
			return 0;
		}

		private float CalcTotalHeight(int numLines)
		{
			var totalHeight = FontHeight * numLines + Spacing * (numLines - 1);
			return totalHeight;
		}

		private void RenderSingleTextLine(Renderer.SpriteList spriteList, ref Vector2 extent, ref Vector2 pos, string line)
		{
			float lineWidth = MeasureTextLine(line).X;
			switch (HAlignment) {
				case HAlignment.Right:
					pos.X = Size.X - lineWidth;
					break;
				case HAlignment.Center:
					pos.X = (Size.X - lineWidth) * 0.5f;
					break;
			}
			if (spriteList != null) {
				Renderer.DrawTextLine(spriteList, Font.Instance, pos, line, Color4.White, FontHeight, 0, line.Length);
			}
			extent.X = Mathf.Max(extent.X, pos.X + lineWidth);
			pos.Y += Spacing + FontHeight;
		}

		private List<string> SplitText(string text)
		{
			var strings = new List<string>(text.Split('\n'));
			for (var i = 0; i < strings.Count; i++) {
				if (OverflowMode == TextOverflowMode.Ellipsis) {
					// Clipping the last line of the text
					if (CalcTotalHeight(i + 2) > Height) {
						strings[i] = ClipLineWithEllipsis(strings[i]);
						while (strings.Count > i + 1) {
							strings.RemoveAt(strings.Count - 1);
						}
						break;
					}
				}
				// Trying to split long lines. If a line can't be split it gets clipped.
				while (MeasureTextLine(strings[i]).X > Width) {
					if (!CarryLastWordToNextLine(strings, i)) {
						if (OverflowMode == TextOverflowMode.Ellipsis) {
							strings[i] = ClipLineWithEllipsis(strings[i]);
						}
						break;
					}
				}
			}
			return strings;
		}

		private Vector2 MeasureTextLine(string line)
		{
			return Renderer.MeasureTextLine(Font.Instance, line, FontHeight);
		}

		private static bool CarryLastWordToNextLine(List<string> strings, int line)
		{
			var lastSpaceAt = strings[line].LastIndexOf(' ');
			if (lastSpaceAt >= 0) {
				if (line + 1 >= strings.Count) {
					strings.Add("");
				}
				if (strings[line + 1] != "") {
					strings[line + 1] = ' ' + strings[line + 1];
				}
				strings[line + 1] = strings[line].Substring(lastSpaceAt + 1) + strings[line + 1];
				strings[line] = strings[line].Substring(0, lastSpaceAt);
			} else {
				return false;
			}
			return true;
		}

		private string ClipLineWithEllipsis(string line)
		{
			var lineWidth = MeasureTextLine(line).X;
			if (lineWidth <= Width) {
				return line;
			}
			while (line.Length > 0 && lineWidth > Width) {
				lineWidth = MeasureTextLine(line + "...").X;
				line = line.Substring(0, line.Length - 1);
			}
			line += "...";
			return line;
		}

		private void SetFont(SerializableFont value)
		{
			if (value != font) {
				font = value;
				DisposeSpriteList();
			}
		}

		private void SetText(string value)
		{
			if (value != text) {
				text = value;
				DisposeSpriteList();
			}
		}

		private void SetFontHeight(float value)
		{
			if (value != fontHeight) {
				fontHeight = value;
				DisposeSpriteList();
			}
		}

		private void SetHAlignment(Lime.HAlignment value)
		{
			if (value != hAlignment) {
				hAlignment = value;
				DisposeSpriteList();
			}
		}

		private void SetVAlignment(Lime.VAlignment value)
		{
			if (value != vAlignment) {
				vAlignment = value;
				DisposeSpriteList();
			}
		}

		private void SetSpacing(float value)
		{
			if (value != spacing) {
				spacing = value;
				DisposeSpriteList();
			}
		}

		private void DisposeSpriteList()
		{
			if (spriteList != null) {
				spriteList.Dispose();
				spriteList = null;
			}
		}
	}
}

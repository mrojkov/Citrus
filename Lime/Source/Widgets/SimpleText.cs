using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public sealed class SimpleText : Widget, IText
	{
		private SpriteList spriteList;
		private SerializableFont font;
		private string text;
		private float fontHeight;
		private float spacing;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private Color4 textColor;

		public Func<string, string> LocalizationHandler;

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

		[ProtoMember(9)]
		public bool WordSplitAllowed { get; set; }

		[ProtoMember(10)]
		public Color4 TextColor
		{
			get { return textColor; }
			set { textColor = value; }
		}

		public SimpleText()
		{
			// CachedRendering = true;
			Text = "";
			FontHeight = 15;
			Font = new SerializableFont();
			TextColor = Color4.White;
		}

		public override Vector2 CalcContentSize()
		{
			return Renderer.MeasureTextLine(Font.Instance, Text, FontHeight);
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			DisposeSpriteList();
		}

		public override void Render()
		{
			if (spriteList == null) {
				if (OverflowMode == TextOverflowMode.Minify) {
					FitTextInsideWidgetArea();
				}
				spriteList = new SpriteList();
				Rectangle extent;
				RenderHelper(spriteList, out extent);
			}

			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			spriteList.Render(GlobalColor * textColor);
		}

		public Rectangle MeasureText()
		{
			Rectangle rect;
			RenderHelper(null, out rect);
			return rect;
		}

		public override void StaticScale(float ratio, bool roundCoordinates)
		{
			fontHeight *= ratio;
			base.StaticScale(ratio, roundCoordinates);
		}

		public void FitTextInsideWidgetArea(float minFontHeight = 10)
		{
			var minH = minFontHeight;
			var maxH = FontHeight;
			if (maxH <= minH) {
				return;
			}
			var bestHeight = minH;
			var spacingKoeff = Spacing / FontHeight;
			while (maxH - minH > 1) {
				var rect = MeasureText();
				var fit = (rect.Width <= Width && rect.Height <= Height);
				if (fit) {
					minH = FontHeight;
					bestHeight = Mathf.Max(bestHeight, FontHeight);
				} else {
					maxH = FontHeight;
				}
				FontHeight = (minH + maxH) / 2;
				Spacing = FontHeight * spacingKoeff;
			}
			FontHeight = bestHeight;
			Spacing = bestHeight * spacingKoeff;
		}

		private void RenderHelper(SpriteList spriteList, out Rectangle rect)
		{
			rect = Rectangle.Empty;
			var localizedText = LocalizationHandler != null ? LocalizationHandler(Text) : Localization.GetString(Text);
			if (string.IsNullOrEmpty(localizedText)) {
				return;
			}
			var lines = SplitText(localizedText);
			var pos = Vector2.Down * CalcVerticalTextPosition(lines);
			bool firstLine = true;
			foreach (var line in lines) {
				Rectangle lineRect;
				RenderSingleTextLine(spriteList, out lineRect, pos, line);
				pos.Y += Spacing + FontHeight;
				if (firstLine) {
					rect = lineRect;
					firstLine = false;
				} else {
					rect = Rectangle.Bounds(rect, lineRect);
				}
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

		private void RenderSingleTextLine(SpriteList spriteList, out Rectangle extent, Vector2 pos, string line)
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
				Renderer.DrawTextLine(Font.Instance, pos, line, Color4.White, FontHeight, 0, line.Length, spriteList);
			}
			extent = new Rectangle(pos.X, pos.Y, pos.X + lineWidth, pos.Y + FontHeight);
		}

		private List<string> SplitText(string text)
		{
			var strings = new List<string>(text.Split('\n'));
			if (OverflowMode == TextOverflowMode.Ignore) {
				return strings;
			}
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
					if (!TextLineSplitter.CarryLastWordToNextLine(strings, i, WordSplitAllowed, IsTextLinePartFitToWidth)) {
						if (OverflowMode == TextOverflowMode.Ellipsis) {
							strings[i] = ClipLineWithEllipsis(strings[i]);
						}
						break;
					}
				}
			}
			return strings;
		}

		private bool IsTextLinePartFitToWidth(string line, int start, int count)
		{
			return Renderer.MeasureTextLine(Font.Instance, line, FontHeight, start, count).X <= Width;
		}

		public Vector2 MeasureTextLine(string line)
		{
			return Renderer.MeasureTextLine(Font.Instance, line, FontHeight);
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
			InvalidateRenderCache();
			spriteList = null;
		}

		private static class TextLineSplitter
		{
			public delegate bool MeasureTextLineWidthDelegate(string line, int start, int count);

			public static bool CarryLastWordToNextLine(List<string> strings, int line, bool isWordSplitAllowed, MeasureTextLineWidthDelegate measureHandler)
			{
				string lastWord;
				string lineWithoutLastWord;
				if (TrySplitLine(strings[line], isWordSplitAllowed, measureHandler, out lineWithoutLastWord, out lastWord)) {
					PushWordToLine(lastWord, strings, line + 1);
					strings[line] = lineWithoutLastWord;
					return true;
				} else {
					return false;
				}
			}

			private static bool TrySplitLine(string line, bool isWordSplitAllowed, MeasureTextLineWidthDelegate measureHandler, out string lineWithoutLastWord, out string lastWord)
			{
				return
					TryCutLastWord(line, out lineWithoutLastWord, out lastWord)
					|| (
						(isWordSplitAllowed || line.HasJapaneseSymbols())
						&& TryCutWordTail(line, measureHandler, out lineWithoutLastWord, out lastWord)
					);
			}

			private static bool TryCutLastWord(string text, out string lineWithoutLastWord, out string lastWord)
			{
				lineWithoutLastWord = null;
				lastWord = null;
				var lastSpaceAt = text.LastIndexOf(' ');
				if (lastSpaceAt >= 0) {
					lineWithoutLastWord = text.Substring(0, lastSpaceAt);
					lastWord = text.Substring(lastSpaceAt + 1);
					return true;
				} else {
					return false;
				}
			}

			private static bool TryCutWordTail(string textLine, MeasureTextLineWidthDelegate measureHandler, out string currentLinePart, out string nextLinePart)
			{
				currentLinePart = null;
				nextLinePart = null;
				var cutFrom = CalcFittedCharactersCount(textLine, measureHandler);
				if (cutFrom > 0) {
					nextLinePart = textLine.Substring(cutFrom);
					currentLinePart = textLine.Substring(0, cutFrom);
					return true;
				} else {
					return false;
				}
			}

			private static int CalcFittedCharactersCount(string textLine, MeasureTextLineWidthDelegate measureHandler)
			{
				int min = 0;
				int max = textLine.Length;
				int mid = 0;
				bool isLineLonger = false;

				do {
					mid = min + ((max - min) / 2);
					isLineLonger = !measureHandler(textLine, 0, mid);
					if (isLineLonger) {
						max = mid;
					} else {
						min = mid;
					}
				}
				while (min < max && !(!isLineLonger && ((max - min) / 2) == 0));

				return mid;
			}

			private static void PushWordToLine(string word, List<string> strings, int line)
			{
				if (line >= strings.Count) {
					strings.Add("");
				}
				if (strings[line] != "") {
					strings[line] = ' ' + strings[line];
				}
				strings[line] = word + strings[line];
			}

		}

	}
}

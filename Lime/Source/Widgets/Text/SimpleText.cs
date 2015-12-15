using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Виджет, выводящий текст с упрощенным форматированием
	/// </summary>
	[ProtoContract]
	public class SimpleText : Widget, IText, IKeyboardInputProcessor
	{
		private SpriteList spriteList;
		private SerializableFont font;
		private string text;
		private Rectangle extent;
		private float fontHeight;
		private float spacing;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private Color4 textColor;
		private string displayText;

		private TextProcessorDelegate textProcessor;
		public event TextProcessorDelegate TextProcessor {
			add { textProcessor += value; Invalidate(); }
			remove { textProcessor -= value; Invalidate(); }
		}

		[ProtoMember(1)]
		public SerializableFont Font {
			get { return font; }
			set {
				if (value != font) {
					font = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Текст, заданный в HotStudio. Этот текст не выводится на экран (см DisplayText)
		/// </summary>
		[ProtoMember(2)]
		public override string Text {
			get { return text ?? ""; }
			set {
				if (value != text) {
					text = value;
					Invalidate();
				}
			}
		}

		public string DisplayText {
			get
			{
				if (displayText == null) {
					displayText = text;
					if (textProcessor != null)
						textProcessor(ref displayText);
				}
				return displayText;
			}
		}

		/// <summary>
		/// Размер шрифта
		/// </summary>
		[ProtoMember(3)]
		public float FontHeight {
			get { return fontHeight; }
			set {
				if (value != fontHeight) {
					fontHeight = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Расстояние между строками
		/// </summary>
		[ProtoMember(4)]
		public float Spacing {
			get { return spacing; }
			set {
				if (value != spacing) {
					spacing = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Горизонтальное выравнивание текста
		/// </summary>
		[ProtoMember(5)]
		public HAlignment HAlignment {
			get { return hAlignment; }
			set {
				if (value != hAlignment) {
					hAlignment = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Вертикальное выравнивание текста
		/// </summary>
		[ProtoMember(6)]
		public VAlignment VAlignment {
			get { return vAlignment; }
			set {
				if (value != vAlignment) {
					vAlignment = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Способ обрезания текста, выходящего за пределы контейнера виджета
		/// </summary>
		[ProtoMember(8)]
		public TextOverflowMode OverflowMode { get; set; }

		/// <summary>
		/// Если текст не помещается в одну строку, то он может разбиваться в любом месте
		/// (если false, то текст может разбиваться только на месте пробелов)
		/// </summary>
		[ProtoMember(9)]
		public bool WordSplitAllowed { get; set; }

		/// <summary>
		/// Цвет текста
		/// </summary>
		[ProtoMember(10)]
		public Color4 TextColor
		{
			get { return textColor; }
			set { textColor = value; }
		}

		/// <summary>
		/// Удалять лишние пробелы между словами
		/// </summary>
		public bool TrimWhitespaces { get; set; }

		private CaretPosition caret = new CaretPosition();
		
		/// <summary>
		/// Если текст находится в состоянии редактирования, возвращает интерфейс каретки
		/// </summary>
		public ICaretPosition Caret { get { return caret; } }

		public bool Localizable { get; set; }

		public SimpleText()
		{
			TrimWhitespaces = true;
			// CachedRendering = true;
			Text = "";
			FontHeight = 15;
			Font = new SerializableFont();
			TextColor = Color4.White;
			TextProcessor += LocalizeProcessor;
			Localizable = true;
		}

		public void LocalizeProcessor(ref string text)
		{
			if (Localizable) {
				text = text.Localize();
			}
		}

		/// <summary>
		/// Возвращает размер текста
		/// </summary>
		public override Vector2 CalcContentSize()
		{
			return Renderer.MeasureTextLine(Font.Instance, DisplayText, FontHeight);
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			Invalidate();
		}

		public override void Render()
		{
			if (caret.Valid != CaretPosition.ValidState.All)
				spriteList = null;
			PrepareSpriteListAndExtent();
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			spriteList.Render(GlobalColor * textColor);
		}

		private void PrepareSpriteListAndExtent()
		{
			if (spriteList != null) {
				return;
			}
			if (OverflowMode == TextOverflowMode.Minify) {
				var savedSpacing = spacing;
				var savedHeight = fontHeight;
				FitTextInsideWidgetArea();
				spriteList = new SpriteList();
				extent = RenderHelper(spriteList);
				spacing = savedSpacing;
				fontHeight = savedHeight;
			} else {
				spriteList = new SpriteList();
				extent = RenderHelper(spriteList);
			}
		}

		/// <summary>
		/// Gets the text's bounding box.
		/// </summary>
		public Rectangle MeasureText()
		{
			PrepareSpriteListAndExtent();
			return extent;
		}

		public override void StaticScale(float ratio, bool roundCoordinates)
		{
			fontHeight *= ratio;
			base.StaticScale(ratio, roundCoordinates);
		}

		/// <summary>
		/// Changes FontHeight and Spacing to make the text inside widget's area.
		/// </summary>
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
				Rectangle rect = RenderHelper(null);
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
			FontHeight = bestHeight.Floor();
			Spacing = bestHeight * spacingKoeff;
		}

		private static CaretPosition dummyCaret = new CaretPosition();

		private Rectangle RenderHelper(SpriteList spriteList)
		{
			Rectangle rect = Rectangle.Empty;
			var savedCaret = caret;
			if (spriteList == null) {
				caret = dummyCaret;
			}
			try {
				var lines = SplitText(DisplayText);
				if (TrimWhitespaces) {
					TrimLinesWhitespaces(lines);
				}
				var pos = Vector2.Down * CalcVerticalTextPosition(lines);
				caret.RenderingLineNumber = 0;
				caret.RenderingTextPos = 0;
				caret.NearestCharPos = Vector2.Zero;
				if (String.IsNullOrEmpty(DisplayText)) {
					caret.WorldPos = pos;
					caret.Line = caret.Pos = caret.TextPos = 0;
					caret.Valid = CaretPosition.ValidState.All;
					return rect;
				}
				bool firstLine = true;
				if (caret.Valid == CaretPosition.ValidState.TextPos)
					Caret.TextPos = Caret.TextPos.Clamp(0, Text.Length);
				if (caret.Valid == CaretPosition.ValidState.LinePos)
					Caret.Line = Caret.Line.Clamp(0, lines.Count - 1);
				int i = 0;
				foreach (var line in lines) {
					bool lastLine = ++i == lines.Count;
					if (caret.Valid == CaretPosition.ValidState.LinePos && caret.Line == caret.RenderingLineNumber) {
						Caret.Pos = Caret.Pos.Clamp(0, line.Length - (lastLine ? 0 : 1));
					}
					Rectangle lineRect = RenderSingleTextLine(spriteList, pos, line);
					if (lastLine) {
						// There is no end-of-text character, so simulate it.
						caret.Sync(line.Length, new Vector2(lineRect.Right, lineRect.Top), Vector2.Down * fontHeight);
					}
					pos.Y += Spacing + FontHeight;
					++caret.RenderingLineNumber;
					if (firstLine) {
						rect = lineRect;
						firstLine = false;
					} else {
						rect = Rectangle.Bounds(rect, lineRect);
					}
				}
				if (caret.Valid == CaretPosition.ValidState.WorldPos) {
					caret.WorldPos = caret.NearestCharPos;
				}
				caret.Valid = CaretPosition.ValidState.All;
			} finally {
				caret = savedCaret;
			}
			return rect;
		}

		private static void TrimLinesWhitespaces(List<string> lines)
		{
			for (int i = 0; i < lines.Count; i++) {
				lines[i] = lines[i].Trim();
			}
		}

		private float CalcVerticalTextPosition(List<string> lines)
		{
			var totalHeight = CalcTotalHeight(lines.Count);
			if (VAlignment == VAlignment.Bottom) {
				return Size.Y - totalHeight;
			} else if (VAlignment == VAlignment.Center) {
				return ((Size.Y - totalHeight) * 0.5f).Round();
			}
			return 0;
		}

		private float CalcTotalHeight(int numLines)
		{
			return Math.Max(FontHeight * numLines + Spacing * (numLines - 1), FontHeight);
		}

		Rectangle RenderSingleTextLine(SpriteList spriteList, Vector2 pos, string line)
		{
			float lineWidth = MeasureTextLine(line).X;
			switch (HAlignment) {
				case HAlignment.Right:
					pos.X = Size.X - lineWidth;
					break;
				case HAlignment.Center:
					pos.X = ((Size.X - lineWidth) * 0.5f).Round();
					break;
			}
			if (spriteList != null) {
				Renderer.DrawTextLine(
					Font.Instance, pos, line, Color4.White, FontHeight, 0, line.Length, spriteList, caret.Sync);
			}
			return new Rectangle(pos.X, pos.Y, pos.X + lineWidth, pos.Y + FontHeight);
		}

		private List<string> SplitText(string text)
		{
			var strings = new List<string>(text.Split('\n'));
			// Add linebreaks to make editor happy.
			for (int i = 0; i < strings.Count - 1; i++) {
				strings[i] += '\n';
			}
			if (OverflowMode == TextOverflowMode.Ignore) {
				return strings;
			}
			for (var i = 0; i < strings.Count; i++) {
				if (OverflowMode == TextOverflowMode.Ellipsis) {
					// Clipping the last line of the text.
					if (CalcTotalHeight(i + 2) > Height) {
						strings[i] = ClipLineWithEllipsis(strings[i]);
						while (strings.Count > i + 1) {
							strings.RemoveAt(strings.Count - 1);
						}
						break;
					}
				}
				// Trying to split long lines. If a line can't be split it gets clipped.
				while (MeasureTextLine(strings[i]).X > Math.Abs(Width)) {
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

		public void Invalidate()
		{
			displayText = null;
			caret.Valid = CaretPosition.ValidState.TextPos;
			spriteList = null;
		}
	}
}

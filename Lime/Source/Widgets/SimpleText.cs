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
	public sealed class SimpleText : Widget
	{
		private Renderer.SpriteList spriteList;
		private SerializableFont font;
		private string text;
		private float fontHeight;
		private float spacing;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private bool autoFit;
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

		[ProtoMember(7)]
		public bool AutoFit {
			get { return autoFit; }
			set { SetAutoFit(value); }
		}

		public SimpleText()
		{
			AutoFit = true;
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

		private void RenderHelper(Renderer.SpriteList spriteList, out Vector2 extent)
		{
			extent = Vector2.Zero;
			var localizedText = Localization.GetString(Text);
			if (string.IsNullOrEmpty(localizedText)) {
				return;
			}
			var lines = SplitText(localizedText);
			var pos = new Vector2();
			var totalHeight = FontHeight * lines.Count + Spacing * (lines.Count - 1);
			switch (VAlignment) {
				case VAlignment.Bottom:
					pos.Y = Size.Y - totalHeight;
					break;
				case VAlignment.Center:
					pos.Y = (Size.Y - totalHeight) * 0.5f;
					break;
			}
			foreach (var line in lines) {
				var lineExtent = Renderer.MeasureTextLine(Font.Instance, line, FontHeight);
				switch (HAlignment) {
					case HAlignment.Right:
						pos.X = Size.X - lineExtent.X;
						break;
					case HAlignment.Center:
						pos.X = (Size.X - lineExtent.X) * 0.5f;
						break;
				}
				if (spriteList != null) {
					Renderer.DrawTextLine(spriteList, Font.Instance, pos, line, Color4.White, FontHeight, 0, line.Length);
				}
				extent.X = Mathf.Max(extent.X, pos.X + lineExtent.X);
				pos.Y += Spacing + FontHeight;
			}
			extent.Y = lines.Count * (FontHeight + Spacing);
			if (extent.Y > 0) {
				extent.Y -= Spacing;
			}
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

		private List<string> SplitText(string text)
		{
			var strings = new List<string>(text.Split('\n'));
			for (var i = 0; i < strings.Count; i++) {
				while (Renderer.MeasureTextLine(Font.Instance, strings[i], FontHeight).X > Width) {
					var lastSpacePosition = strings[i].LastIndexOf(' ');
					if (lastSpacePosition >= 0) {
						if (i + 1 >= strings.Count) {
							strings.Add("");
						}
						if (strings[i + 1] != "") {
							strings[i + 1] = ' ' + strings[i + 1];
						}
						strings[i + 1] = strings[i].Substring(lastSpacePosition + 1) + strings[i + 1];
						strings[i] = strings[i].Substring(0, lastSpacePosition);
					} else {
						break;
					}
				}
			}
			return strings;
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

		private void SetAutoFit(bool value)
		{
			if (value != autoFit) {
				autoFit = value;
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

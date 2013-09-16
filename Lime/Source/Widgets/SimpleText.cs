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
		[ProtoMember(1)]
		public SerializableFont Font { get; set; }

		[ProtoMember(2)]
		public override string Text { get; set; }

		[ProtoMember(3)]
		public float FontHeight { get; set; }

		[ProtoMember(4)]
		public float Spacing { get; set; }

		[ProtoMember(5)]
		public HAlignment HAlignment { get; set; }

		[ProtoMember(6)]
		public VAlignment VAlignment { get; set; }

		[ProtoMember(7)]
		public bool AutoFit { get; set; }

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
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Vector2 extent;
			RenderHelper(measureOnly: false, extent: out extent);
		}

		public Vector2 MeasureText()
		{
			Vector2 extent;
			RenderHelper(measureOnly: true, extent: out extent);
			return extent;
		}

		private void RenderHelper(bool measureOnly, out Vector2 extent)
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
				if (!measureOnly) {
					Renderer.DrawTextLine(Font.Instance, pos, line, FontHeight, GlobalColor);
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
	}
}

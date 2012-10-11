using Lime;
using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	[ProtoContract]
	public class SimpleText : Widget
	{
		[ProtoMember(1)]
		public SerializableFont Font { get; set; }

		[ProtoMember(2)]
		public string Text { get; set; }

		[ProtoMember(3)]
		public float FontHeight { get; set; }

		[ProtoMember(4)]
		public float Spacing { get; set; }

		[ProtoMember(5)]
		public HAlignment HAlignment { get; set; }

		[ProtoMember(6)]
		public VAlignment VAlignment { get; set; }

		public SimpleText()
		{
			Text = "";
			FontHeight = 15;
			Font = new SerializableFont();
		}

		public override void Render(float extrapolation)
		{
			Renderer.Transform1 = globalMatrix;
			Renderer.Blending = globalBlending;
			var localizedText = Localization.GetString(Text);
			if (!string.IsNullOrEmpty(localizedText)) {
				var strings = SplitText(localizedText);
				var pos = new Vector2();
				float totalHeight = FontHeight * strings.Count + Spacing * (strings.Count - 1);
				if (VAlignment == VAlignment.Bottom)
					pos.Y = Size.Y - totalHeight;
				else if (VAlignment == VAlignment.Center)
					pos.Y = (Size.Y - totalHeight) * 0.5f;
				foreach (var str in strings) {
					var extent = Renderer.MeasureTextLine(Font.Instance, str, FontHeight);
					if (HAlignment == HAlignment.Right)
						pos.X = Size.X - extent.X;
					else if (HAlignment == HAlignment.Center)
						pos.X = (Size.X - extent.X) * 0.5f;
					Renderer.DrawTextLine(Font.Instance, pos, str, FontHeight, globalColor);
					pos.Y += Spacing + FontHeight;
				}
			}
		}

		private List<string> SplitText(string text)
		{
			var strings = new List<string>(text.Split('\n'));
			for (int i = 0; i < strings.Count; i++) {
				while (Renderer.MeasureTextLine(Font.Instance, strings[i], FontHeight).X > Width) {
					int lastSpacePosition = strings[i].LastIndexOf(' ');
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

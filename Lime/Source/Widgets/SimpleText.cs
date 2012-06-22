using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SimpleText : Widget
	{
		[ProtoMember(1)]
		public SerializableFont Font = new SerializableFont();

		[ProtoMember(2)]
		public string Text = "";

		[ProtoMember(3)]
		public float FontHeight = 15;

		[ProtoMember(4)]
		public float Spacing = 0;

		[ProtoMember(5)]
		public HAlignment HAlignment;

		[ProtoMember(6)]
		public VAlignment VAlignment;

		public override void Render()
		{
			Renderer.Transform1 = globalMatrix;
			Renderer.Blending = globalBlending;
			var localizedText = Localization.GetString(Text);
			if (!string.IsNullOrEmpty(localizedText)) {
				var strings = localizedText.Split('\n');
				var pos = new Vector2();
				float totalHeight = FontHeight * strings.Length + Spacing * (strings.Length - 1);
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
	}
}

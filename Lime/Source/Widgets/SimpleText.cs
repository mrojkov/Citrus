using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SimpleText : Widget
	{
		[ProtoMember (1)]
		public SerializableFont Font = new SerializableFont ();

		[ProtoMember (2)]
		public string Text;

		[ProtoMember (3)]
		public float FontHeight = 15;

		[ProtoMember (4)]
		public float Spacing = 0;

		[ProtoMember (5)]
		public HAlignment HorizontalAlign;

		[ProtoMember (6)]
		public VAlignment VerticalAlign;

		public override void Render ()
		{
			if (worldShown) {
				Renderer.WorldMatrix = worldMatrix;
				Renderer.Blending = worldBlending;
				if (!string.IsNullOrEmpty (Text)) {
					Renderer.DrawTextLine (Font.Instance, Vector2.Zero, Text, worldColor, FontHeight);
				}
			}
		}
	}
}

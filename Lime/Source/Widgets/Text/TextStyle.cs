using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	public class TextStyle : Node
	{
		public static TextStyle Default = new TextStyle();

		[ProtoContract]
		public enum ImageUsageEnum
		{
			[ProtoEnum]
			Bullet,
			[ProtoEnum]
			Overlay
		}

		[ProtoMember(1)]
		[YuzuMember]
		public ITexture ImageTexture { get; set; }

		[ProtoMember(2)]
		[YuzuMember]
		public Vector2 ImageSize { get; set; }

		[ProtoMember(3)]
		[YuzuMember]
		public ImageUsageEnum ImageUsage { get; set; }

		[ProtoMember(4)]
		[YuzuMember]
		public SerializableFont Font { get; set; }

		[ProtoMember(5)]
		[YuzuMember]
		public float Size { get; set; }

		[ProtoMember(6)]
		[YuzuMember]
		public float SpaceAfter { get; set; }

		[ProtoMember(7)]
		[YuzuMember]
		public bool Bold { get; set; }

		[ProtoMember(8)]
		[YuzuMember]
		public bool CastShadow { get; set; }

		[ProtoMember(9)]
		[YuzuMember]
		public Vector2 ShadowOffset { get; set; }

		[ProtoMember(10)]
		[YuzuMember]
		public Color4 TextColor { get; set; }

		[ProtoMember(11)]
		[YuzuMember]
		public Color4 ShadowColor { get; set; }

		public TextStyle()

		{
			Size = 15;
			TextColor = Color4.White;
			ShadowColor = Color4.Black;
			ShadowOffset = Vector2.One;
			Font = new SerializableFont();
		}
	}
}

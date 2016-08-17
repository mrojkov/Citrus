using Yuzu;

namespace Lime
{
	public class TextStyle : Node
	{
		public static TextStyle Default = new TextStyle();

		public enum ImageUsageEnum
		{
			Bullet,
			Overlay
		}

		[YuzuMember]
		public ITexture ImageTexture { get; set; }

		[YuzuMember]
		public Vector2 ImageSize { get; set; }

		[YuzuMember]
		public ImageUsageEnum ImageUsage { get; set; }

		[YuzuMember]
		public SerializableFont Font { get; set; }

		[YuzuMember]
		public float Size { get; set; }

		[YuzuMember]
		public float SpaceAfter { get; set; }

		[YuzuMember]
		public bool Bold { get; set; }

		[YuzuMember]
		public bool CastShadow { get; set; }

		[YuzuMember]
		public Vector2 ShadowOffset { get; set; }

		[YuzuMember]
		public Color4 TextColor { get; set; }

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

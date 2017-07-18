using Yuzu;

namespace Lime
{
	[AllowedParentTypes(typeof(RichText))]
	public class TextStyle : Node
	{
		private ITexture imageTexture;
		private Vector2 imageSize;
		private ImageUsageEnum imageUsage;
		private SerializableFont font;
		private float size;
		private float spaceAfter;
		private bool bold;
		private bool castShadow;
		private Vector2 shadowOffset;
		private Color4 textColor;
		private Color4 shadowColor;
		public ShaderProgram ShaderProgram;
		internal int PalleteIndex = -1;

		public static TextStyle Default = new TextStyle();

		public enum ImageUsageEnum
		{
			Bullet,
			Overlay
		}

		[YuzuMember]
		public ITexture ImageTexture {
			get { return imageTexture; }
			set
			{
				if (imageTexture != value) {
					imageTexture = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public Vector2 ImageSize
		{
			get { return imageSize; }
			set
			{
				if (imageSize != value) {
					imageSize = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public ImageUsageEnum ImageUsage
		{
			get { return imageUsage; }
			set
			{
				if (imageUsage != value) {
					imageUsage = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public SerializableFont Font
		{
			get { return font; }
			set
			{
				if (font != value) {
					font = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public float Size
		{
			get { return size; }
			set
			{
				if (size != value) {
					size = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public float SpaceAfter
		{
			get { return spaceAfter; }
			set
			{
				if (spaceAfter != value) {
					spaceAfter = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public bool Bold
		{
			get { return bold; }
			set
			{
				if (bold != value) {
					bold = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public bool CastShadow
		{
			get { return castShadow; }
			set
			{
				if (castShadow != value) {
					castShadow = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public Vector2 ShadowOffset
		{
			get { return shadowOffset; }
			set
			{
				if (shadowOffset != value) {
					shadowOffset = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public Color4 TextColor
		{
			get { return textColor; }
			set
			{
				if (textColor != value) {
					textColor = value;
					InvalidateRichText();
				}
			}
		}

		[YuzuMember]
		public Color4 ShadowColor
		{
			get { return shadowColor; }
			set
			{
				if (shadowColor != value) {
					shadowColor = value;
					InvalidateRichText();
				}
			}
		}

		public TextStyle()
		{
			RenderChainBuilder = null;
			Size = 15;
			TextColor = Color4.White;
			ShadowColor = Color4.Black;
			ShadowOffset = Vector2.One;
			Font = new SerializableFont();
		}

		void InvalidateRichText() => (Parent as RichText)?.Invalidate();

		protected override void OnTagChanged()
		{
			ShaderProgram = null;
			if (!int.TryParse(Tag, out PalleteIndex)) {
				PalleteIndex = -1;
			}
		}
	}
}

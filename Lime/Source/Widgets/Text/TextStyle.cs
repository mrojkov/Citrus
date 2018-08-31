using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 12)]
	[TangerineAllowedParentTypes(typeof(RichText))]
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
		internal int gradientMapIndex = -1;
		private float letterSpacing;
		public static TextStyle Default = new TextStyle();

		public enum ImageUsageEnum
		{
			Bullet,
			Overlay
		}

		[YuzuMember]
		[TangerineKeyframeColor(28)]
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
		[TangerineKeyframeColor(29)]
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
		[TangerineKeyframeColor(30)]
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
		[TangerineKeyframeColor(31)]
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
		[TangerineKeyframeColor(1)]
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
		[TangerineKeyframeColor(2)]
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
		[TangerineKeyframeColor(3)]
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
		[TangerineKeyframeColor(4)]
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
		[TangerineKeyframeColor(5)]
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
		[TangerineKeyframeColor(6)]
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

		[YuzuMember]
		public int GradientMapIndex
		{
			get
			{
				return gradientMapIndex;
			}
			set
			{
				gradientMapIndex = value;
				InvalidateRichText();
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(7)]
		public float LetterSpacing
		{
			get { return letterSpacing; }
			set
			{
				if (letterSpacing != value) {
					letterSpacing = value;
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

		private void InvalidateRichText() => (Parent as RichText)?.Invalidate();

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}

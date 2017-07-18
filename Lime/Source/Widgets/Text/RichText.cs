using System;
using Lime.Text;
using Yuzu;

namespace Lime
{
	[TangerineNodeBuilder("BuildForTangerine")]
	[AllowedChildrenTypes(typeof(TextStyle))]
	public class RichText : Widget, IText
	{
		private TextParser parser = new TextParser();
		private string text;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private TextOverflowMode overflowMode;
		public SpriteList spriteList;
		internal TextRenderer Renderer;
		public Action<RichText, Sprite> SpriteListElementHandler = null;
		private string displayText;
		private TextProcessorDelegate textProcessor;

		[YuzuMember]
		public override string Text
		{
			get { return text; }
			set
			{
				if (text != value) {
					text = value;
					Invalidate();
				}
			}
		}

		public string DisplayText
		{
			get
			{
				if (displayText == null) {
					displayText = Localizable ? Text.Localize() : Text;
					textProcessor?.Invoke(ref displayText);
				}
				return displayText;
			}
		}

		public event TextProcessorDelegate TextProcessor
		{
			add
			{
				textProcessor += value;
				Invalidate();
			}
			remove
			{
				textProcessor -= value;
				Invalidate();
			}
		}

		[YuzuMember]
		public HAlignment HAlignment
		{
			get { return hAlignment; }
			set
			{
				if (hAlignment != value) {
					hAlignment = value;
					Invalidate();
				}
			}
		}

		[YuzuMember]
		public VAlignment VAlignment
		{
			get { return vAlignment; }
			set
			{
				if (vAlignment != value) {
					vAlignment = value;
					Invalidate();
				}
			}
		}

		[YuzuMember]
		public TextOverflowMode OverflowMode
		{
			get { return overflowMode; }
			set
			{
				if (overflowMode != value) {
					overflowMode = value;
					Invalidate();
				}
			}
		}

		[YuzuMember]
		public bool WordSplitAllowed { get; set; }

		// TODO
		public bool TrimWhitespaces { get; set; }

		public event Action<string> Submitted;

		public RichText()
		{
			Presenter = DefaultPresenter.Instance;
			Localizable = true;
			Localizable = true;
			TrimWhitespaces = true;
			Text = "";
			SpriteListElementHandler = Lime.ShaderPrograms.ColorfulTextShaderProgram.HandleRichTextSprite;
		}

		void IText.Submit()
		{
			if (Submitted != null) {
				Submitted(Text);
			}
		}

		public override void Dispose()
		{
			Invalidate();
			base.Dispose();
		}

		private string errorMessage;

		public string ErrorMessage
		{
			get
			{
				ParseText();
				return errorMessage;
			}
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			Invalidate();
		}

		private void InvokeSpriteHandler(Sprite sprite)
		{
			SpriteListElementHandler?.Invoke(this, sprite);
		}

		private bool InvertStylesPalleteIndex()
		{
			bool dirty = false;
			foreach (var s in Renderer.Styles) {
				if (s.PalleteIndex != -1) {
					dirty = true;
					s.PalleteIndex = ShaderPrograms.ColorfulTextShaderProgram.GradientMapTextureSize - s.PalleteIndex - 1;
					s.ShaderProgram = null;
				}
			}
			return dirty;
		}

		public override void Render()
		{
			EnsureSpriteList();
			Lime.Renderer.Transform1 = LocalToWorldTransform;
			Lime.Renderer.Blending = GlobalBlending;
			Lime.Renderer.Shader = GlobalShader;
			spriteList.Render(GlobalColor, InvokeSpriteHandler);
			if (InvertStylesPalleteIndex()) {
				spriteList.Render(GlobalColor, InvokeSpriteHandler);
				InvertStylesPalleteIndex();
			}
		}

		private void EnsureSpriteList()
		{
			if (spriteList == null) {
				spriteList = new SpriteList();
				PrepareRenderer().Render(spriteList, Size, HAlignment, VAlignment);
			}
		}

		// TODO: return effective AABB, not only extent
		public Rectangle MeasureText()
		{
			var extent = PrepareRenderer().MeasureText(Size.X, Size.Y);
			return new Rectangle(Vector2.Zero, extent);
		}

		// TODO
		public bool Localizable { get; set; }

		public override void StaticScale(float ratio, bool roundCoordinates)
		{
			foreach (var node in Nodes) {
				var style = node as TextStyle;
				if (style != null) {
					style.Size *= ratio;
					style.ImageSize *= ratio;
					style.SpaceAfter *= ratio;
					style.ShadowOffset *= ratio;
				}
			}
			base.StaticScale(ratio, roundCoordinates);
		}

		private TextRenderer PrepareRenderer()
		{
			if (Renderer != null)
				return Renderer;
			ParseText();
			Renderer = new TextRenderer(OverflowMode, WordSplitAllowed);
			// Setup default style(take first one from node list or TextStyle.Default).
			TextStyle defaultStyle = null;
			if (Nodes.Count > 0) {
				defaultStyle = Nodes[0] as TextStyle;
			}
			Renderer.AddStyle(defaultStyle ?? TextStyle.Default);
			// Fill up style list.
			foreach (var styleName in parser.Styles) {
				var style = Nodes.TryFind(styleName) as TextStyle;
				Renderer.AddStyle(style ?? TextStyle.Default);
			}
			// Add text fragments.
			foreach (var frag in parser.Fragments) {
				// Warning! Using style + 1, because -1 is a default style.
				Renderer.AddFragment(frag.Text, frag.Style + 1, frag.IsNbsp);
			}
			return Renderer;
		}

		public void RemoveUnusedStyles()
		{
			PrepareRenderer();
			for (int i = 1; i < Nodes.Count; ) {
				var node = Nodes[i];
				if (node is TextStyle && !Renderer.HasStyle(node as TextStyle))
					Nodes.RemoveAt(i);
				else
					++i;
			}
		}

		private void ParseText()
		{
			if (parser != null) {
				return;
			}
			parser = new TextParser(DisplayText);
			errorMessage = parser.ErrorMessage;
			if (errorMessage != null) {
				parser = new TextParser("Error: " + errorMessage);
			}
		}

		public void Invalidate()
		{
			spriteList = null;
			parser = null;
			Renderer = null;
			displayText = null;
			InvalidateParentConstraintsAndArrangement();
			Window.Current?.Invalidate();
		}

		/// Call on user-supplied parts of text.
		public static string Escape(string text)
		{
			return text.
				Replace("&amp;", "&amp;amp;").Replace("&lt;", "&amp;lt;").Replace("&gt;", "&amp;&gt").
				Replace("<", "&lt;").Replace(">", "&gt;");
		}

		/// <summary>
		/// Make a hit test. Returns style of a text chunk a user hit to.
		/// TODO: implement more advanced system with tag attributes like <link url="...">...</link>
		/// </summary>
		public bool HitTest(Vector2 point, out TextStyle style)
		{
			style = null;
			int tag;
			EnsureSpriteList();
			if (spriteList.HitTest(LocalToWorldTransform, point, out tag) && tag >= 0) {
				style = null;
				if (tag == 0) {
					style = Nodes[0] as TextStyle;
				} else {
					style = Nodes.TryFind(parser.Styles[tag - 1]) as TextStyle;
				}
				return true;
			}
			return false;
		}

		public ICaretPosition Caret { get; set; } = DummyCaretPosition.Instance;

		void IText.SyncCaretPosition() { }

		public bool CanDisplay(char ch) { return true; }

		private void BuildForTangerine()
		{
			var style = new TextStyle { Id = "TextStyle1" };
			Nodes.Add(style);
		}
	}
}

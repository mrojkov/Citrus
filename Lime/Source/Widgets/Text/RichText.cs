using System;
using Lime.Text;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Виджет, отображающий текст и использующий продвинутое форматирование
	/// </summary>
	public class RichText : Widget, IText
	{
		private TextParser parser = new TextParser();
		private string text;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private SpriteList spriteList;
		private TextRenderer renderer;

		[YuzuMember]
		public override string Text
		{
			get { return text; }
			set { SetText(value); }
		}

		// TODO
		public string DisplayText { get { return text; } }
		public event TextProcessorDelegate TextProcessor;

		[YuzuMember]
		public HAlignment HAlignment
		{
			get { return hAlignment; }
			set { SetHAlignment(value); }
		}

		[YuzuMember]
		public VAlignment VAlignment
		{
			get { return vAlignment; }
			set { SetVAlignment(value); }
		}

		[YuzuMember]
		public TextOverflowMode OverflowMode { get; set; }

		[YuzuMember]
		public bool WordSplitAllowed { get; set; }

		// TODO
		public bool TrimWhitespaces { get; set; }

		public event Action<string> Submitted;

		public RichText()
		{
			// CachedRendering = true;
			Localizable = true;
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

		public override void Render()
		{
			EnsureSpriteList();
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			spriteList.Render(GlobalColor);
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
			if (renderer != null)
				return renderer;
			ParseText();
			renderer = new TextRenderer(OverflowMode, WordSplitAllowed);
			// Setup default style(take first one from node list or TextStyle.Default).
			TextStyle defaultStyle = null;
			if (Nodes.Count > 0) {
				defaultStyle = Nodes[0] as TextStyle;
			}
			renderer.AddStyle(defaultStyle ?? TextStyle.Default);
			// Fill up style list.
			foreach (var styleName in parser.Styles) {
				var style = Nodes.TryFind(styleName) as TextStyle;
				renderer.AddStyle(style ?? TextStyle.Default);
			}
			// Add text fragments.
			foreach (var frag in parser.Fragments) {
				// Warning! Using style + 1, because -1 is a default style.
				renderer.AddFragment(frag.Text, frag.Style + 1, frag.IsNbsp);
			}
			return renderer;
		}

		public void RemoveUnusedStyles()
		{
			PrepareRenderer();
			for (int i = 1; i < Nodes.Count; ) {
				var node = Nodes[i];
				if (node is TextStyle && !renderer.HasStyle(node as TextStyle))
					Nodes.RemoveAt(i);
				else
					++i;
			}
		}

		private void SetHAlignment(Lime.HAlignment value)
		{
			if (value == hAlignment) {
				return;
			}
			hAlignment = value;
			Invalidate();
		}

		private void SetVAlignment(Lime.VAlignment value)
		{
			if (value == vAlignment) {
				return;
			}
			vAlignment = value;
			Invalidate();
		}

		private void SetText(string value)
		{
			if (value == text) {
				return;
			}
			Invalidate();
			text = value;
		}

		private void ParseText()
		{
			if (parser != null) {
				return;
			}
			var localizedText = Localization.GetString(text);
			parser = new TextParser(localizedText);
			errorMessage = parser.ErrorMessage;
			if (errorMessage != null) {
				parser = new TextParser("Error: " + errorMessage);
			}
		}

		public void Invalidate()
		{
			spriteList = null;
			parser = null;
			renderer = null;
			InvalidateParentConstraintsAndArrangement();
			if (Window.Current != null) {
				Window.Current.Invalidate();
			}
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

		void IText.SyncCaretPosition() { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Lime.Text;

namespace Lime
{
	[ProtoContract]
	public class RichText : Widget, IText
	{
		private TextParser parser = new TextParser();
		private string text;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private SpriteList spriteList;

		[ProtoMember(1)]
		public override string Text
		{
			get { return text; }
			set { SetText(value); }
		}

		public string DisplayText // TODO
		{
			get { return text; }
			set { SetText(value); }
		}

		[ProtoMember(2)]
		public HAlignment HAlignment 
		{ 
			get { return hAlignment; } 
			set { SetHAlignment(value); } 
		}
		
		[ProtoMember(3)]
		public VAlignment VAlignment 
		{ 
			get { return vAlignment; } 
			set { SetVAlignment(value); } 
		}

		[ProtoMember(4)]
		public TextOverflowMode OverflowMode { get; set; }

		[ProtoMember(5)]
		public bool WordSplitAllowed { get; set; }

		// TODO
		public bool TrimWhitespaces { get; set; }

		public RichText()
		{
			// CachedRendering = true;
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
				var renderer = PrepareRenderer();
				spriteList = new SpriteList();
				renderer.Render(spriteList, Size, HAlignment, VAlignment);
			}
		}

		// TODO: return effective AABB, not only extent
		public Rectangle MeasureText()
		{
			var renderer = PrepareRenderer();
			var extent = renderer.MeasureText(Size.X, Size.Y);
			return new Rectangle(Vector2.Zero, extent);
		}

		public override void StaticScale(float ratio, bool roundCoordinates)
		{
			foreach (var node in Nodes) {
				var style = node as TextStyle;
				if (style != null) {
					style.Size *= ratio;
				}
			}
			base.StaticScale(ratio, roundCoordinates);
		}


		private TextRenderer PrepareRenderer()
		{
			ParseText();
			var renderer = new TextRenderer(OverflowMode, WordSplitAllowed);
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
				renderer.AddFragment(frag.Text, frag.Style + 1);
			}
			return renderer;
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
			InvalidateRenderCache();
			spriteList = null;
			parser = null;
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
	}
}

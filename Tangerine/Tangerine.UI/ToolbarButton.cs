using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Yuzu;

namespace Tangerine.UI
{
	public class ToolbarButton : Button
	{
		enum State
		{
			Default,
			Highlight,
			Press
		}

		private State state;
		private ITexture texture;
		private bool @checked;
		private bool @highlightable;

		public bool Checked
		{
			get { return @checked; }
			set
			{
				if (@checked != value) {
					@checked = value;
					Window.Current.Invalidate();
				}
			}
		}

		public bool Highlightable
		{
			get { return @highlightable; }
			set
			{
				if (@highlightable != value) {
					@highlightable = value;
					Window.Current.Invalidate();
				}
			}
		}

		public override ITexture Texture
		{
			get { return texture; }
			set
			{
				if (texture != value) {
					texture = value;
					Window.Current.Invalidate();
				}
			}
		}

		public ToolbarButton()
		{
			highlightable = true;
			Nodes.Clear();
			Padding = new Thickness(3);
			Size = MinMaxSize = new Vector2(22, 22);
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					if (markerId == "Focus") {
						state = State.Highlight;
					} else if (markerId == "Press") {
						state = State.Press;
					} else {
						state = State.Default;
					}
					Window.Current.Invalidate();
					return true;
				}
			};
			CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Color4 bgColor, borderColor;
				GetColors(out bgColor, out borderColor);
				if (bgColor != Color4.Transparent) {
					Renderer.DrawRect(Vector2.One, Size - 2 * Vector2.One, bgColor);
				}
				Renderer.DrawSprite(Texture, GlobalColor, ContentPosition, ContentSize, Vector2.Zero, Vector2.One);
				if (borderColor != Color4.Transparent) {
					Renderer.DrawRectOutline(Vector2.One, Size - 2 * Vector2.One, borderColor, 1);
				}
			}));
		}

		private void GetColors(out Color4 bgColor, out Color4 borderColor)
		{
			if (Highlightable && state == State.Highlight) {
				bgColor = ToolbarColors.ButtonHighlightBackground;
				borderColor = ToolbarColors.ButtonHighlightBorder;
			} else if (Highlightable && state == State.Press) {
				bgColor = ToolbarColors.ButtonPressBackground;
				borderColor = ToolbarColors.ButtonPressBorder;
			} else if (Checked) {
				bgColor = ToolbarColors.ButtonCheckedBackground;
				borderColor = ToolbarColors.ButtonCheckedBorder;
			} else {
				bgColor = Color4.Transparent;
				borderColor = Color4.Transparent;
			}
		}

		public ToolbarButton(ITexture texture) : this()
		{
			Texture = texture;
		}
	}	
}
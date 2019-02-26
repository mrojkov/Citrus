using System;
using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ToolbarButton : Button
	{
		protected enum State
		{
			Default,
			Highlight,
			Press
		}

		private State state;
		private ITexture texture;
		private bool isChecked;
		private bool highlightable;
		private string text;

		public bool Checked
		{
			get { return isChecked; }
			set
			{
				if (isChecked != value) {
					isChecked = value;
					Window.Current.Invalidate();
				}
			}
		}

		public bool Highlightable
		{
			get { return highlightable; }
			set
			{
				if (highlightable != value) {
					highlightable = value;
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

		private ThemedSimpleText caption;
		public override string Text
		{
			get => text;
			set {
				if (value != text) {
					text = value;
					if (caption != null) {
						caption.Text = text;
						Window.Current.Invalidate();
					}
				}
			}
		}

		public string Tooltip { get; set; }

		public ToolbarButton()
		{
			highlightable = true;
			Nodes.Clear();
			Padding = new Thickness(2);
			Size = MinMaxSize = Theme.Metrics.DefaultToolbarButtonSize;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId, animationTimeCorrection) => {
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
			CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.Shader = Enabled ? ShaderId.Diffuse : ShaderId.Silhuette;
				Color4 bgColor, borderColor;
				GetColors(state, out bgColor, out borderColor);
				if (bgColor != Color4.Transparent) {
					Renderer.DrawRect(Vector2.Zero, Size, bgColor);
				}
				if (Texture != null) {
					var iconColor = Enabled ? GlobalColor : GlobalColor * ColorTheme.Current.Toolbar.ButtonDisabledColor;
					Renderer.DrawSprite(Texture, iconColor, ContentPosition, ContentSize, Vector2.Zero, Vector2.One);
				} else if (caption != null) {
					caption.Color = Enabled ? Theme.Colors.BlackText : Theme.Colors.GrayText;
				}
				if (borderColor != Color4.Transparent) {
					Renderer.DrawRectOutline(Vector2.Zero, Size, borderColor);
				}
			}));
			Awoke += Awake;
		}

		private static void Awake(Node owner)
		{
			var tb = (ToolbarButton)owner;
			tb.Tasks.Add(Lime.Tooltip.Instance.ShowOnMouseOverTask(tb, () => tb.Tooltip));
			tb.AddChangeWatcher(() => tb.Enabled, _ => Window.Current.Invalidate());
		}

		protected virtual void GetColors(State state, out Color4 bgColor, out Color4 borderColor)
		{
			if (Highlightable && state == State.Highlight) {
				bgColor = ColorTheme.Current.Toolbar.ButtonHighlightBackground;
				borderColor = ColorTheme.Current.Toolbar.ButtonHighlightBorder;
			} else if (Highlightable && state == State.Press) {
				bgColor = ColorTheme.Current.Toolbar.ButtonPressBackground;
				borderColor = ColorTheme.Current.Toolbar.ButtonPressBorder;
			} else if (Checked) {
				bgColor = ColorTheme.Current.Toolbar.ButtonCheckedBackground;
				borderColor = ColorTheme.Current.Toolbar.ButtonCheckedBorder;
			} else {
				bgColor = Color4.Transparent;
				borderColor = Color4.Transparent;
			}
		}

		public ToolbarButton(ITexture texture) : this()
		{
			Texture = texture;
		}

		public ToolbarButton(string text) : this()
		{
			caption = new ThemedSimpleText(text) {
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center
			};
			Text = text;
			MinMaxSize = new Vector2(caption.MeasureUncutText().X + 10, MaxSize.Y);
			AddNode(caption);
			caption.ExpandToContainerWithAnchors();
		}
	}
}

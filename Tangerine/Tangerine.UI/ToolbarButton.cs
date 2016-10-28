using System;
using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;

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

		public string Tip { get; set; }

		public ToolbarButton()
		{
			highlightable = true;
			Nodes.Clear();
			Padding = new Thickness(3);
			Size = MinMaxSize = DesktopTheme.Metrics.DefaultButtonSize.Y * Vector2.One;
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
				Renderer.Shader = Enabled ? ShaderId.Diffuse : ShaderId.Silhuette;
				Color4 bgColor, borderColor;
				GetColors(out bgColor, out borderColor);
				if (bgColor != Color4.Transparent) {
					Renderer.DrawRect(Vector2.Zero, Size, bgColor);
				}
				var iconColor = Enabled ? GlobalColor : GlobalColor * ToolbarColors.ButtonDisabledColor;
				Renderer.DrawSprite(Texture, iconColor, ContentPosition, ContentSize, Vector2.Zero, Vector2.One);
				if (borderColor != Color4.Transparent) {
					Renderer.DrawRectOutline(Vector2.Zero, Size, borderColor);
				}
			}));
		}

		protected override void Awake()
		{
			base.Awake();
			Tasks.Add(ShowTipTask());
			this.AddChangeWatcher(() => Enabled, _ => Window.Current.Invalidate());
		}

		private IEnumerator<object> ShowTipTask()
		{
			while (true) {
				yield return null;
				if (IsMouseOver() && Tip != null) {
					var showTip = true;
					for (float t = 0; t < 0.5f; t += Task.Current.Delta) {
						if (!IsMouseOver()) {
							showTip = false;
							break;
						}
						yield return null;
					}
					if (showTip) {
						var window = WidgetContext.Current.Root;
						var tip = new Widget {
							Position = CalcPositionInSpaceOf(window) + new Vector2(Width * 0.66f, Height),
							LayoutCell = new LayoutCell { Ignore = true },
							Layout = new StackLayout(),
							Nodes = {
								new SimpleText { Text = Tip, Padding = new Thickness(4) },
								new BorderedFrame()
							}
						};
						tip.Updated += _ => tip.Size = tip.EffectiveMinSize;
						window.PushNode(tip);
						try {
							while (IsMouseOver()) {
								yield return null;
							}
						} finally {
							tip.Unlink();
						}
					}
				}
			}
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
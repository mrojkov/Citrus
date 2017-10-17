using Lime;

namespace Tangerine.UI
{
	public class ThemedExpandButton : ThemedButton
	{
		public bool Expanded { get; set; }

		public ThemedExpandButton()
		{
			var presenter = new ExpandButtonPresenter();
			Presenter = presenter;
			LayoutCell = new LayoutCell() { Alignment = Alignment.Center };
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
		}

		protected override void HandleClick()
		{
			Expanded = !Expanded;
			base.HandleClick();
		}

		private class ExpandButtonPresenter : CustomPresenter<ThemedExpandButton>, IButtonPresenter
		{
			private ColorGradient innerGradient;
			public void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				switch (state) {
					case "Press":
						innerGradient = Theme.Colors.ExpandButtonPress;
						break;
					case "Focus":
						innerGradient = Theme.Colors.ExpandButtonHover;
						break;
					case "Disable":
						innerGradient = Theme.Colors.ExpandButtonDisable;
						break;
					default:
						innerGradient = Theme.Colors.ExpandButtonDefault;
						break;
				}
			}

			static Vertex[] vertices = new Vertex[3];

			protected override void InternalRender(ThemedExpandButton widget)
			{
				widget.PrepareRendererState();
				var pos = widget.ContentPosition;
				var size = widget.ContentSize;
				if (widget.Expanded) {
					vertices[0] = new Vertex { Pos = pos, Color = innerGradient.A };
					vertices[1] = new Vertex { Pos = new Vector2(pos.X + size.X, pos.Y), Color = innerGradient.A };
					vertices[2] = new Vertex { Pos = new Vector2(pos.X + size.X / 2, pos.Y + size.Y), Color = innerGradient.B };
				} else {
					vertices[0] = new Vertex { Pos = pos, Color = innerGradient.A };
					vertices[1] = new Vertex { Pos = new Vector2(pos.X, pos.Y + size.Y), Color = innerGradient.A };
					vertices[2] = new Vertex { Pos = new Vector2(pos.X + size.X, pos.Y + size.Y / 2), Color = innerGradient.B };
				}
				Renderer.DrawTriangleFan(null, null, vertices, vertices.Length);
			}

			protected override bool InternalPartialHitTest(ThemedExpandButton node, ref HitTestArgs args)
			{
				return node.PartialHitTest(ref args);
			}
		}
	}
}
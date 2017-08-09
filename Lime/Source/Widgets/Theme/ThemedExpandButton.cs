namespace Lime
{
    public class ThemedExpandButton : ThemedButton
    {
	    public bool Expanded { get; set; }
		public ThemedExpandButton()
		{
			var presenter =  new EpandButtonPresenter();
			Presenter = presenter;
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

		private class EpandButtonPresenter : CustomPresenter<ThemedExpandButton>, IButtonPresenter
		{
			private ColorGradient innerGradient;
			public void SetState(string state)
			{
				CommonWindow.Current.Invalidate();
				switch (state)
				{
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
			
			protected override void InternalRender(ThemedExpandButton widget)
			{
				widget.PrepareRendererState();
				var vertices = new Vertex[3];
				if (widget.Expanded) {
					vertices[0] = new Vertex { Pos = Vector2.Zero, Color = innerGradient.A};
					vertices[1] = new Vertex { Pos = new Vector2(widget.Size.X, 0), Color = innerGradient.A};
					vertices[2] = new Vertex { Pos = new Vector2(widget.Size.X / 2, widget.Size.Y), Color = innerGradient.B};
				} else {
					vertices[0] = new Vertex { Pos = Vector2.Zero, Color = innerGradient.A};
					vertices[1] = new Vertex { Pos = new Vector2(0, widget.Size.Y), Color = innerGradient.A};
					vertices[2] = new Vertex { Pos = new Vector2(widget.Size.X, widget.Size.Y / 2), Color = innerGradient.B};
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
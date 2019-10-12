namespace Lime
{
	public class ColorCheckBox : CheckBox
	{
		public override bool IsNotDecorated() => false;

		public ColorCheckBox(Color4 color)
		{
			Layout = new StackLayout();
			AddNode(new Button {
				Id = "Button",
				Presenter = new CheckBoxPresenter(this, color),
				LayoutCell = new LayoutCell(Alignment.Center),
				MinMaxSize = new Vector2(16, 16),
				TabTravesable = null
			});
			TabTravesable = new TabTraversable();
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
			State = CheckBoxState.Checked;
		}

		private class CheckBoxPresenter : IPresenter
		{
			private readonly CheckBox checkBox;
			private readonly Color4 color;

			public CheckBoxPresenter(CheckBox checkBox, Color4 color)
			{
				this.checkBox = checkBox;
				this.color = color;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(widget);
				ro.Size = widget.Size;
				ro.BackgroundColor = color;
				ro.CheckBoxSize = new Vector2(16, 16);
				ro.State = checkBox.State;
				return ro;
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public Color4 BackgroundColor;
				public Vector2 CheckBoxSize;
				public CheckBoxState State;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawRect(Vector2.Zero, Size, State == CheckBoxState.Checked ? BackgroundColor : Color4.Black);
				}
			}
		}
	}
}

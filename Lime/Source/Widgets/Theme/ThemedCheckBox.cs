#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedCheckBox : CheckBox
	{
		public override bool IsNotDecorated() => false;

		public ThemedCheckBox()
		{
			Layout = new StackLayout();
			AddNode(new Button {
				Id = "Button",
				Presenter = new CheckBoxPresenter(this),
				LayoutCell = new LayoutCell(Alignment.Center),
				MinMaxSize = Theme.Metrics.CheckBoxSize,
				TabTravesable = null
			});
			TabTravesable = new TabTraversable();
			CompoundPostPresenter.Add(new Theme.MouseHoverBorderPresenter());
			LateTasks.Add(Theme.MouseHoverInvalidationTask(this));
		}

		class CheckBoxPresenter : IPresenter
		{
			readonly CheckBox checkBox;

			private static readonly VectorShape checkedIcon = new VectorShape {
				new VectorShape.Line(0.2f, 0.5f, 0.4f, 0.8f, Theme.Colors.BlackText, 0.1f),
				new VectorShape.Line(0.4f, 0.8f, 0.75f, 0.25f, Theme.Colors.BlackText, 0.1f),
			};

			private static readonly VectorShape indeterminateIcon = new VectorShape {
				new VectorShape.Line(0.2f, 0.5f, 0.8f, 0.5f, Theme.Colors.BlackText, 0.6f, antialiased: false)
			};

			public CheckBoxPresenter(CheckBox checkBox)
			{
				this.checkBox = checkBox;
			}

			public bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var widget = (Widget)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(widget);
				ro.Size = widget.Size;
				ro.BackgroundColor = Theme.Colors.WhiteBackground;
				ro.BorderColor = Theme.Colors.ControlBorder;
				ro.CheckBoxSize = Theme.Metrics.CheckBoxSize;
				ro.State = checkBox.State;
				return ro;
			}

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			private class RenderObject : WidgetRenderObject
			{
				public Vector2 Size;
				public Color4 BackgroundColor;
				public Color4 BorderColor;
				public Vector2 CheckBoxSize;
				public CheckBoxState State;

				public override void Render()
				{
					PrepareRenderState();
					Renderer.DrawRect(Vector2.Zero, Size, BackgroundColor);
					Renderer.DrawRectOutline(Vector2.Zero, Size, BorderColor);
					if (State == CheckBoxState.Checked) {
						var transform = Matrix32.Scaling(Theme.Metrics.CheckBoxSize);
						checkedIcon.Draw(transform);
					} else if (State == CheckBoxState.Indeterminate) {
						var transform = Matrix32.Scaling(Theme.Metrics.CheckBoxSize);
						indeterminateIcon.Draw(transform);
					}
				}
			}
		}
	}
}
#endif

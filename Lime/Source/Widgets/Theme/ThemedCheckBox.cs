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

		class CheckBoxPresenter : CustomPresenter
		{
			readonly CheckBox checkBox;

			private VectorShape icon = new VectorShape {
				new VectorShape.Line(0.2f, 0.5f, 0.4f, 0.8f, Theme.Colors.BlackText, 0.1f),
				new VectorShape.Line(0.4f, 0.8f, 0.75f, 0.25f, Theme.Colors.BlackText, 0.1f),
			};

			public CheckBoxPresenter(CheckBox checkBox)
			{
				this.checkBox = checkBox;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Theme.Colors.WhiteBackground);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Theme.Colors.ControlBorder);
				if (checkBox.Checked) {
					var transform = Matrix32.Scaling(Theme.Metrics.CheckBoxSize);
					icon.Draw(transform);
				}
			}

			public override bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);
		}
	}
}
#endif

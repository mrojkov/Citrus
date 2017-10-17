using Lime;

namespace Tangerine.UI
{
	public class ThemedDeleteButton : Button
	{
		public override bool IsNotDecorated() => false;

		public ThemedDeleteButton()
		{
			var presenter = new VectorShapeButtonPresenter(new VectorShape {
				new VectorShape.Line(0.3f, 0.5f, 0.7f, 0.5f, Color4.White, 0.075f * 1.5f),
			});
			LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0);
			PostPresenter = presenter;
			MinMaxSize = Theme.Metrics.CloseButtonSize;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
		}
	}

	public class ThemedAddButton : Button
	{
		public override bool IsNotDecorated() => false;

		public ThemedAddButton()
		{
			var presenter = new VectorShapeButtonPresenter(new VectorShape {
				new VectorShape.Line(0.45f, 0.15f, 0.45f, 0.75f, Color4.White, 0.075f * 1.5f),
				new VectorShape.Line(0.15f, 0.45f, 0.75f, 0.45f, Color4.White, 0.075f * 1.5f),
			});
			LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0);
			Presenter = presenter;
			MinMaxSize = Theme.Metrics.CloseButtonSize;
			DefaultAnimation.AnimationEngine = new AnimationEngineDelegate {
				OnRunAnimation = (animation, markerId) => {
					presenter.SetState(markerId);
					return true;
				}
			};
		}
	}
}

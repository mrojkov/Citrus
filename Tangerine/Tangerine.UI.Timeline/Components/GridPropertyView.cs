using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridPropertyView : IGridRowView
	{
		private readonly Node node;
		private readonly IAnimator animator;

		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }
		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.Get<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.Get<AwakeBehavior>();

		public GridPropertyView(Node node, IAnimator animator)
		{
			this.node = node;
			this.animator = animator;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new DelegatePresenter<Widget>(Render)
			};
			GridWidget.Components.Add(new AwakeBehavior());
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new DelegatePresenter<Widget>(Render)
			};
			OverviewWidget.Components.Add(new AwakeBehavior());
		}

		private void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, ColorTheme.Current.TimelineGrid.PropertyRowBackground);
			var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(animator.Animable.GetType(), animator.TargetPropertyPath)?.ColorIndex ?? 0;
			var color = KeyframePalette.Colors[colorIndex];
			var baseTransform = Renderer.Transform1;
			for (int i = 0; i < animator.ReadonlyKeys.Count; i++) {
				var key = animator.ReadonlyKeys[i];
				Renderer.Transform1 =
					Matrix32.RotationRough(Mathf.Pi / 4) *
					Matrix32.Translation((key.Frame + 0.5f) * TimelineMetrics.ColWidth + 0.5f, widget.Height / 2 + 0.5f) *
					baseTransform;
				var v = TimelineMetrics.ColWidth / 3 * Vector2.One;
				Renderer.DrawRect(-v, v, color);
			}
		}
	}
}

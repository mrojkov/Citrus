using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public partial class ListView : ScrollViewWithSlider, IList<Widget>
	{
		public void SetRefreshGesture(Widget animationTemplate, IEnumerable<object> refreshTask)
		{
			new RefreshGesture(this, animationTemplate, refreshTask);
		}

		private class RefreshGesture
		{
			private ListView listView;
			private Widget animation;
			private IEnumerable<object> refreshTask;

			public RefreshGesture(ListView listView, Widget animationTemplate, IEnumerable<object> refreshTask)
			{
				this.refreshTask = refreshTask;
				this.listView = listView;
				this.animation = animationTemplate.Clone<Widget>();
				listView.Content.Tasks.Add(MainTask());
				listView.Content.Tasks.Add(RefreshAnimationPositionTask());
			}

			private IEnumerator<object> RefreshAnimationPositionTask()
			{
				animation.PushToNode(listView.Frame);
				animation.Pivot = new Vector2(0, 1);
				animation.Position = Vector2.Zero;
				animation.Width = listView.Frame.Width;
				while (true) {
					animation.Y = -listView.ScrollPosition;
					yield return null;
				}
			}

			private IEnumerator<object> MainTask()
			{
				var threshold = listView.MinScrollPosition - animation.Height;
				var arrow = animation["Arrow"];
				while (true) {
					yield return Task.WaitWhile(() => listView.ScrollPosition > threshold);
					arrow.Visible = true;
					arrow.RunAnimation("Rotate");
					while (listView.ScrollPosition < threshold) {
						if (!listView.Frame.Input.IsMousePressed()) {
							arrow.Visible = false;
							yield return RefreshTask();
							break;
						}
						yield return null;
					}
					arrow.RunAnimation("Restore");
					yield return Task.WaitWhile(() => listView.ScrollPosition < threshold);
				}
			}

			private IEnumerator<object> RefreshTask()
			{
				animation.RunAnimation("Show");
				listView.IsBeingRefreshed = true;
				try {
					yield return refreshTask.GetEnumerator();
					animation.RunAnimation("Hide");
					while (animation.DefaultAnimation.IsRunning) {
						yield return null;
					}
				} finally {
					listView.IsBeingRefreshed = false;
					listView.StopScrolling();
				}
			}
		}
	}
}
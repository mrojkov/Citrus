using System;
using Lime;

namespace EmptyProject
{
	public static class Extensions
	{
		public static void SafeInvoke(this Action handler)
		{
			if (handler != null) {
				handler();
			}
		}

		public static void SafeInvoke<T>(this Action<T> handler, T value)
		{
			if (handler != null) {
				handler(value);
			}
		}

		public static Widget Play(this Widget widget, string animationName)
		{
			widget.RunAnimation(animationName);
			return widget;
		}

		public static void Play(this Widget widget, string animationName, Action onStopped = null)
		{
			var animation = widget.GetAnimation();
			animation.Play(animationName, onStopped);
		}

		public static void Play(this Animation animation, string animationName, Action onStopped = null)
		{
			Action animationStoppedHandler = null;
			animationStoppedHandler = () => {
				if (onStopped != null) {
					onStopped();
				}

				animation.Stopped -= animationStoppedHandler;
			};

			animation.TryRun(animationName);
			animation.Stopped += animationStoppedHandler;
		}

		public static Animation GetAnimation(this Widget widget)
		{
			Animation animation;
			if (!widget.Animations.TryFind(null, out animation)) {
				throw new Lime.Exception("Unknown animation or marker");
			}

			return animation;
		}

		public static void ExpandToContainer(this Widget widget)
		{
			if (widget.ParentWidget != null) {
				widget.Size = widget.ParentWidget.Size;
			}
			widget.Anchors = Anchors.LeftRightTopBottom;
		}
	}
}

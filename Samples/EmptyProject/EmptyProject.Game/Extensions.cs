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

		public static Widget Play(this Widget widget, string animationNameFormat, params object[] args)
		{
			return widget.Play(string.Format(animationNameFormat, args));
		}

		public static void ExpandToContainer(this Widget widget)
		{
			if (widget.ParentWidget != null)
			{
				widget.Size = widget.ParentWidget.Size;
			}
			widget.Anchors = Anchors.LeftRightTopBottom;
		}
	}
}

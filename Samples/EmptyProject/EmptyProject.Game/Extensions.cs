using System;
using Lime;

namespace EmptyProject
{
	public static class Extensions
	{
		public static Widget Play(this Widget widget, string animationName)
		{
			widget.RunAnimation(animationName);
			return widget;
		}

		public static Widget Play(this Widget widget, string animationNameFormat, params object[] args)
		{
			return widget.Play(String.Format(animationNameFormat, args));
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

#if ANDROID

using System;

namespace Lime
{
	public class WebBrowser : Widget
	{
		public Uri Url { get; set; }

		public WebBrowser(Widget parentWidget)
			: this()
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public WebBrowser()
		{
		}

		public override void Dispose()
		{
		}

		protected override void SelfUpdate(float delta)
		{
		}
	}
}

#endif
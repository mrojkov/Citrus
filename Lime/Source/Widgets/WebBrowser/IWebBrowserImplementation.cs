#if WIN
using System;

namespace Lime
{
	public interface IWebBrowserImplementation
	{
		void Dispose();
		Uri Url { get; set; }
		void Render(Widget widget);
		void Update(Widget widget, float delta);
		void OnSizeChanged(Widget widget, Vector2 sizeDelta);
	}
}
#endif
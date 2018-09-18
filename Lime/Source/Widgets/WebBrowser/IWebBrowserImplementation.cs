#if WIN
using System;

namespace Lime
{
	public interface IWebBrowserImplementation
	{
		void Dispose();
		Uri Url { get; set; }
		void Render();
		void Update(float delta);
		void OnSizeChanged(Vector2 sizeDelta);
	}
}
#endif
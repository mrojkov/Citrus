#if iOS
using System;
using System.Drawing;
using Lime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Lime
{
	public class WebBrowser : IDisposable
	{
		public Uri Url { get { return GetUrl(); } set { SetUrl(value); } }	
		
		public WebBrowser(Widget widgetToLink)
			: this()
		{
			linkedWidget = widgetToLink;
			SetUpdateHandler(linkedWidget);
			linkedWidget.Updating += updateHandler;
		}
		
		public WebBrowser()
		{
			webView = new UIWebView();
			webView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			webView.ScalesPageToFit = true;
			webView.Opaque = false;
			webView.BackgroundColor = new UIColor(0.0f, 0.0f, 0.0f, 1.0f);
			webView.Hidden = true;
			GameView.Instance.AddSubview(webView);
		}
		
		public void Dispose()
		{
			if (linkedWidget != null) {
				linkedWidget.Updating -= updateHandler;
				linkedWidget = null;
			}
			if (webView != null) {
				webView.RemoveFromSuperview();
				webView.Dispose();
				webView = null;
			}
		}
		
		private Widget linkedWidget;
		private UpdateHandler updateHandler;
		
		private void SetUpdateHandler(Widget linkedWidget)
		{
			updateHandler = new UpdateHandler((delta) => {
				float screenHeight = GameView.Instance.Size.Height;
				WindowRect wr = CalculateAABBInWorldSpace(linkedWidget);
				float Height = (float)wr.Height * 0.5f;
				float offsetY = (screenHeight) - Height;
				var position = new PointF((float)wr.X * 0.5f, (float)(wr.Y * 0.5f) + offsetY);
				var size = new SizeF((float)wr.Width * 0.5f, Height);
				webView.Frame = new RectangleF(position, size);
				webView.Hidden = false;
			});
		}
	
		private WindowRect CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(World.Instance);
			// Get the projected AABB coordinates in the normalized OpenGL space
			Matrix44 proj = Renderer.Projection;
			aabb.A = proj.TransformVector(aabb.A);
			aabb.B = proj.TransformVector(aabb.B);
			// Transform to 0,0 - 1,1 coordinate space
			aabb.Left = (1 + aabb.Left) / 2;
			aabb.Right = (1 + aabb.Right) / 2;
			aabb.Top = (1 + aabb.Top) / 2;
			aabb.Bottom = (1 + aabb.Bottom) / 2;
			// Transform to window coordinates
			var viewport = Renderer.Viewport;
			var result = new WindowRect();
			var min = new Vector2(viewport.X, viewport.Y);
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height);
			result.X = Mathf.Lerp(aabb.Left, min.X, max.X).Round();
			result.Width = Mathf.Lerp(aabb.Right, min.X, max.X).Round() - result.X;
			result.Y = Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round();
			result.Height = Mathf.Lerp(aabb.Top, min.Y, max.Y).Round() - result.Y;
			return result;
		}
		
		private UIWebView webView;	

		private Uri GetUrl()
		{
			return new Uri(webView.Request.Url.AbsoluteString);
		}

		private void SetUrl(Uri value)
		{
			NSUrlRequest request = new NSUrlRequest(new NSUrl(value.AbsoluteUri));
			webView.LoadRequest(request);
		}
		
	}
}
#endif
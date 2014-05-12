#if iOS
using System;
using System.Drawing;
using Lime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Lime
{
    public class WebBrowser
    {
		private UIWebView webView = null;
		
		private Widget linkedWidget = null;
		
		private UpdateHandler updateHandler = null;
		
		private float screenH;
		
		private WindowRect CalculateWidgetRectangle(Widget widget)
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
		
        public WebBrowser()
        {
			webView = new UIWebView();
			webView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			webView.ScalesPageToFit = true;
			webView.Hidden = true;	
			GameView.Instance.AddSubview(webView);
        }
		
		public void SetURLToWidget(Widget widget, string url, float sh)
		{
			screenH = sh;
			linkedWidget = widget;
			SetUpdateHandler();
			widget.Updating += updateHandler;
			NSUrlRequest request = new NSUrlRequest(new NSUrl(url));
			webView.LoadRequest(request);
		}
		
		public void UpdateURL(string url)
		{
			NSUrlRequest request = new NSUrlRequest(new NSUrl(url));
			webView.LoadRequest(request);
		}
		
		public void Delete()
		{
			if (linkedWidget != null) {
				linkedWidget.Updating -= updateHandler;
				linkedWidget = null;
			}
			webView.RemoveFromSuperview();
		}
		
		public void Hide()
		{
			webView.Hidden = true;
		}
		
		public void Show()
		{
			webView.Hidden = false;
		}
		
		private void SetUpdateHandler()
		{
			updateHandler = new UpdateHandler((delta) => {
				WindowRect wr = CalculateWidgetRectangle(linkedWidget);
				float Height = (float)wr.Height * 0.5f;
				
				float offsetY = (screenH * 0.5f) - Height;
				
				webView.Frame = new RectangleF(new PointF((float)wr.X * 0.5f, (float)(wr.Y * 0.5f) + offsetY),
											   new SizeF((float)wr.Width * 0.5f, Height));
				webView.Hidden = !linkedWidget.GloballyVisible;	
			});
		}
    }
}
#endif
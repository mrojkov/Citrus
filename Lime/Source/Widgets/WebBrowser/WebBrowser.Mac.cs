#if MAC
using System;
using System.Drawing;
using Lime;
using Foundation;
using AppKit;
using WebKit;
using CoreGraphics;

namespace Lime
{
	public class WebBrowser : Widget
	{
		private WebView webView;

		public Uri Url { get { return GetUrl(); } set { SetUrl(value); } }	

		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public WebBrowser()
		{
			webView = new WebView();
			webView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable; 
			webView.WantsLayer = true;
			Window.Current.NSGameView.AddSubview(webView);
		}

		public override void Dispose()
		{
			if (webView != null) {
				webView.MainFrame.StopLoading();
				var localWebView = webView;
				Application.InvokeOnMainThread(() => {
					localWebView.RemoveFromSuperview(); // RemoveFromSuperview must run in main thread only.
					localWebView.Dispose();
				});
				webView = null;
			}
		}

		public override void Update(float delta)
		{
			if (webView == null) {
				return;
			}
			webView.Frame = CalculateAABBInWorldSpace(this);
			base.Update(delta);
		}

		private CGRect CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(WidgetContext.Current.Root);
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
			var result = new CGRect();
			var min = new Vector2(viewport.X, viewport.Y) / Window.Current.PixelScale;
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height) / Window.Current.PixelScale;
			result.X = Mathf.Lerp(aabb.Left, min.X, max.X).Round();
			result.Width = Mathf.Lerp(aabb.Right, min.X, max.X).Round() - result.X;
			result.Y = Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round();
			result.Height = Mathf.Lerp(aabb.Top, min.Y, max.Y).Round() - result.Y;
			return result;
		}

		private Uri GetUrl()
		{
			return new Uri(webView.MainFrame.DataSource.Request.Url.AbsoluteString);
		}

		private void SetUrl(Uri value)
		{
			NSUrlRequest request = new NSUrlRequest(new NSUrl(value.AbsoluteUri));
			webView.MainFrame.LoadRequest(request);
		}

	}
}
#endif

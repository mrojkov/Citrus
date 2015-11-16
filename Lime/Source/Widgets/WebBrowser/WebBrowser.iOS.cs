#if iOS
using System;
using System.Drawing;
using Lime;
using Foundation;
using UIKit;

namespace Lime
{
	public class WebBrowser : Widget
	{
		private UIWebView webView;
		private UIActivityIndicatorView activityIndicator;

		private bool isActivityIndicatorVisible = false;
	
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
			webView = new UIWebView();
			webView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			webView.ScalesPageToFit = true;
			webView.ScrollView.ShowsHorizontalScrollIndicator = false;
			webView.ScrollView.Scrolled += (object sender, EventArgs e) => {
				webView.ScrollView.ShowsVerticalScrollIndicator = true;
				if (webView.ScrollView.ContentOffset.X != 0.0f) {
					webView.ScrollView.SetContentOffset(new PointF(0.0f, (float)webView.ScrollView.ContentOffset.Y), false);
					webView.ScrollView.ShowsVerticalScrollIndicator = false;
				}
				if (webView.ScrollView.ContentOffset.Y < 0.0f) {
					webView.ScrollView.SetContentOffset(new PointF((float)webView.ScrollView.ContentOffset.X, 0.0f), false);
					webView.ScrollView.ShowsVerticalScrollIndicator = false;
				}
			};
			webView.Opaque = false;
			webView.BackgroundColor = new UIColor(0.0f, 0.0f, 0.0f, 1.0f);
			webView.Hidden = true;
			WidgetContext.Current.Window.UIViewController.View.AddSubview(webView);
		}
		
		public override void Dispose()
		{
			if (activityIndicator != null) {
				if (isActivityIndicatorVisible) {
					activityIndicator.RemoveFromSuperview();
					isActivityIndicatorVisible = false;
				}
				activityIndicator.Dispose();
				activityIndicator = null;
			}
			if (webView != null) {
				webView.StopLoading();
				webView.Delegate = null;
				var localWebView = webView;
				Application.InvokeOnMainThread(() => {
					localWebView.RemoveFromSuperview(); // RemoveFromSuperview must run in main thread only.
					localWebView.Dispose();
				});
				webView = null;
			}
		}

		protected override void SelfUpdate(float delta)
		{
			if (webView == null) {
				return;
			}
			var wr = CalculateAABBInDeviceSpace(this);
			var position = new PointF(wr.Left, wr.Top);
			var size = new SizeF(wr.Width, wr.Height);
			webView.Frame = new RectangleF(position, size);
			webView.Hidden = false;
			var activityIndicatorPosition = new PointF((size.Width * 0.5f) + position.X, (size.Height * 0.5f) + position.Y);
			if (activityIndicator == null) {
				activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
				activityIndicator.Center = activityIndicatorPosition;
				var view = WidgetContext.Current.Window.UIViewController.View;
				webView.LoadStarted += (object sender, EventArgs e) => {
					activityIndicator.StartAnimating();
					view.AddSubview(activityIndicator);
					isActivityIndicatorVisible = true;
				};
				webView.LoadFinished += (object sender, EventArgs e) => {
					activityIndicator.StopAnimating();
					activityIndicator.RemoveFromSuperview();
					isActivityIndicatorVisible = false;
				};
			}
			activityIndicator.Center = activityIndicatorPosition;
		}
	
		private Rectangle CalculateAABBInDeviceSpace(Widget widget)
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
			// Transform to device coordinates
			var viewport = Renderer.Viewport;
			var result = new Rectangle();
			var screenScale = (float)UIScreen.MainScreen.Scale;
			var min = new Vector2(viewport.X, viewport.Y) / screenScale;
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height) / screenScale;
			var displayHeight = WidgetContext.Current.Window.ClientSize.Height / screenScale;
			result.Left = Mathf.Lerp(aabb.Left, min.X, max.X).Round();
			result.Right = Mathf.Lerp(aabb.Right, min.X, max.X).Round();
			result.Top = displayHeight - Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round();
			result.Bottom = displayHeight - Mathf.Lerp(aabb.Top, min.Y, max.Y).Round();
			return result;
		}

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
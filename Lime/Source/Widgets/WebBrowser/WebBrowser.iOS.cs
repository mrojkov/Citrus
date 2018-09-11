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
		private Rectangle aabbInDeviceSpace;
		
		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		public WebBrowser()
		{
			webView = new UIWebView {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				ScalesPageToFit = true,
				Opaque = false,
				BackgroundColor = UIColor.Black,
				Hidden = true,
			};
			webView.ScrollView.Bounces = false;
			webView.ScrollView.BouncesZoom = false;
			webView.LoadStarted += WebView_LoadStarted;
			webView.LoadFinished += WebView_LoadFinished;
			GameView.AddSubview(webView);
		}	

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRightTopBottom;
		}

		public Uri Url 
		{ 
			get { return new Uri(webView.Request.Url.AbsoluteString); } 
			set 
			{ 
				var request = new NSUrlRequest(new NSUrl(value.AbsoluteUri));
				webView.LoadRequest(request); 
			} 
		}

		private void WebView_LoadStarted(object sender, EventArgs e) {
			if (activityIndicator == null) {
				activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
				activityIndicator.Center = ActivityIndicatorPosition;
			}
			activityIndicator.StartAnimating();
			GameView.AddSubview(activityIndicator);
			isActivityIndicatorVisible = true;
		}

		private GameView GameView 
		{
			get { return Window.Current.UIViewController.View; }
		}

		private void WebView_LoadFinished(object sender, EventArgs e) {
			activityIndicator.StopAnimating();
			activityIndicator.RemoveFromSuperview();
			isActivityIndicatorVisible = false;
		}

		private PointF ActivityIndicatorPosition 
		{
			get { 
				return new PointF((WebViewSize.Width * 0.5f) + WebViewPosition.X,
					(WebViewSize.Height * 0.5f) + WebViewPosition.Y);
			}
		}

		private PointF WebViewPosition 
		{
			get { return new PointF(aabbInDeviceSpace.Left, aabbInDeviceSpace.Top); }
		}

		private SizeF WebViewSize 
		{
			get { return new SizeF(aabbInDeviceSpace.Width, aabbInDeviceSpace.Height); }
		}

		public override void Update(float delta)
		{
			if (webView == null) {
				return;
			}
			aabbInDeviceSpace = CalculateAABBInDeviceSpace(this);
			webView.Frame = new RectangleF(WebViewPosition, WebViewSize);
			webView.Hidden = false;
			if (activityIndicator != null) {
				activityIndicator.Center = ActivityIndicatorPosition;
			}
			base.Update(delta);
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
	
		private static Rectangle CalculateAABBInDeviceSpace(Widget widget)
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
			var viewport = ((WindowWidget)WidgetContext.Current.Root).GetViewport();
			var result = new Rectangle();
			var min = new Vector2(viewport.X, viewport.Y) / Window.Current.PixelScale;
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height) / Window.Current.PixelScale;
			var displayHeight = Window.Current.ClientSize.Y;
			result.Left = Mathf.Lerp(aabb.Left, min.X, max.X).Round();
			result.Right = Mathf.Lerp(aabb.Right, min.X, max.X).Round();
			result.Top = displayHeight - Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round();
			result.Bottom = displayHeight - Mathf.Lerp(aabb.Top, min.Y, max.Y).Round();
			return result;
		}
	}
}
#endif
#if ANDROID
using System;
using Android.Webkit;
using System.Drawing;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace Lime
{
	public class WebBrowser : Widget
	{
		private WebView webView;
		public Uri Url { get { return GetUrl(); } set { SetUrl(value); } }

		public WebBrowser(Widget parentWidget)
			: this()
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		~WebBrowser()
		{
			Dispose();
		}

		public WebBrowser()
		{
			CreateWebView();
		}

		private void CreateWebView()
		{
			webView = new WebView(ActivityDelegate.Instance.GameView.Context);
			webView.SetBackgroundColor(Android.Graphics.Color.Transparent);
			webView.Settings.JavaScriptEnabled = true;
			webView.SetWebViewClient(new WebViewClient());
		}

		public override Node DeepCloneFast()
		{
			var result = (WebBrowser)base.DeepCloneFast();
			result.CreateWebView();
			return result;
		}

		public override void Dispose()
		{
			GC.SuppressFinalize(this);
			if (webView != null) {
				webView.StopLoading();
				try {
					webView.Visibility = ViewStates.Gone;
				} catch (System.Exception) {
					// If the device is locked, setting Visibility causes an exception
				}
				// Workaround for a crash in RelativeLayout.onLayout() while rotating the device
				EventHandler<OpenTK.FrameEventArgs> a = null;
				a = (s, e) => {
					((RelativeLayout)ActivityDelegate.Instance.GameView.Parent).RemoveView(webView);
					ActivityDelegate.Instance.GameView.UpdateFrame -= a;
				};
				ActivityDelegate.Instance.GameView.UpdateFrame += a;
				webView = null;
			}
		}

		public void SetBackgroundColor(Android.Graphics.Color color)
		{
			webView.SetBackgroundColor(color);
		}

		protected override void SelfUpdate(float delta)
		{
			if (webView == null) {
				return;
			}
			WindowRect wr = CalculateAABBInWorldSpace(this);
			// Unlink webView once its window got invisible. This fixes webview's disappearance on device rotation
			if (webView.Parent != null && webView.WindowVisibility != ViewStates.Visible) {
				((RelativeLayout)webView.Parent).RemoveView(webView);
			}
			if (webView.Parent == null) {
				((RelativeLayout)ActivityDelegate.Instance.GameView.Parent).AddView(webView);
			}
			var p = (RelativeLayout.MarginLayoutParams)webView.LayoutParameters;
			p.LeftMargin = wr.X;
			p.TopMargin = wr.Y;
			p.Width = wr.Width;
			p.Height = wr.Height;
			webView.RequestLayout();
		}

		private WindowRect CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(WidgetContext.Current.Root);
			var viewport = Renderer.Viewport;
			var scale = new Vector2(viewport.Width, viewport.Height) / WidgetContext.Current.Root.Size;
			return new WindowRect {
				X = (viewport.X + aabb.Left * scale.X).Round(),
				Y = (viewport.Y + aabb.Top * scale.Y).Round(),
				Width = (aabb.Width * scale.X).Round(),
				Height = (aabb.Height * scale.Y).Round()
			};
		}

		private Uri GetUrl()
		{
			return new Uri(webView.Url);
		}

		private void SetUrl(Uri value)
		{
			webView.LoadUrl(value.AbsoluteUri);
		}
	}
}
#endif

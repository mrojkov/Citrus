#if ANDROID
using System;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Android.Widget;

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
			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.LoadWithOverviewMode = true;
			webView.Settings.UseWideViewPort = true;
			webView.Settings.BuiltInZoomControls = true;
			webView.Settings.DisplayZoomControls = false;
			webView.SetWebViewClient(new CustomClient());
		}

		public override Node Clone()
		{
			var result = (WebBrowser)base.Clone();
			result.CreateWebView();
			return result;
		}

		public override void Dispose()
		{
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
					webView.Dispose();
					ActivityDelegate.Instance.GameView.UpdateFrame -= a;
				};
				ActivityDelegate.Instance.GameView.UpdateFrame += a;
			}
			// Browser may request keyboard and we should hide it on our own when closing browser.
			// TODO: Check browser behaviour due to new Input logic
			if (/*KeyboardFocus.Instance.Focused == null &&*/Application.SoftKeyboard.Visible) {
				Application.SoftKeyboard.Show(false);
			}
			GC.SuppressFinalize(this);
		}

		public void SetBackgroundColor(Android.Graphics.Color color)
		{
			webView.SetBackgroundColor(color);
		}

		public override void Update(float delta)
		{
			base.Update(delta);
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
			// Reduce web view height by keyboard overlap to keep focused widget visible
			// and let scroll through the whole page if soft keyboard is shown.
			if (p.TopMargin < CalcKeyboardTop()) {
				var webViewBottom = p.TopMargin + p.Height;
				p.Height -= (int)Math.Max(0, webViewBottom / Window.Current.PixelScale - CalcKeyboardTop());
			}
			webView.RequestLayout();
		}

		private static WindowRect CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(WidgetContext.Current.Root);
			var viewport = ((WindowWidget)WidgetContext.Current.Root).GetViewport();
			var scale = new Vector2(viewport.Width, viewport.Height) / WidgetContext.Current.Root.Size;
			return new WindowRect {
				X = (viewport.X + aabb.Left * scale.X).Round(),
				Y = (viewport.Y + aabb.Top * scale.Y).Round(),
				Width = (aabb.Width * scale.X).Round(),
				Height = (aabb.Height * scale.Y).Round()
			};
		}

		private static float CalcKeyboardTop()
		{
			return Window.Current.ClientSize.Y - Application.SoftKeyboard.Height;
		}

		private Uri GetUrl()
		{
			return new Uri(webView.Url);
		}

		private void SetUrl(Uri value)
		{
			webView.LoadUrl(value.AbsoluteUri);
		}

		private class CustomClient : WebViewClient
		{
			public override bool ShouldOverrideUrlLoading(WebView view, string url)
			{
				if (url.StartsWith("file:") || url.StartsWith("http:") || url.StartsWith("https:")) {
					return false;
				}

				// Otherwise allow the OS to handle things like tel, mailto, etc. 
				var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
				ActivityDelegate.Instance.Activity.StartActivity(intent);
				return true;
			}
		}
	}
}
#endif

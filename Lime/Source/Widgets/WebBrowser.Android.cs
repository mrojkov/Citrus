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
			webView = new WebView(GameView.Instance.Context);
			webView.SetBackgroundColor(new Android.Graphics.Color(0));
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
				Action a = null;
				a = () => {
					((RelativeLayout)GameView.Instance.Parent).RemoveView(webView);
					GameView.DidUpdated -= a;
				};
				GameView.DidUpdated += a;
				webView = null;
			}
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
				((RelativeLayout)GameView.Instance.Parent).AddView(webView);
			}
			webView.Layout (wr.X, wr.Y, wr.Width + wr.X, wr.Height + wr.Y);
		}

		private WindowRect CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(World.Instance);
			var viewport = Renderer.Viewport;
			var scale = new Vector2(viewport.Width, viewport.Height) / World.Instance.Size;
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

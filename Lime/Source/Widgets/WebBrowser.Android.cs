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
				webView.Visibility = ViewStates.Gone;
				// Workaround for a crash in RelativeLayout.onLayout() while rotating the device
				Action a = null;
				a = () => {
					MainActivity.Instance.ContentView.RemoveView(webView);
					GameView.DidUpdated -= a;
				};
				GameView.DidUpdated += a;
				webView = null;
			}
		}

		private RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(0, 0);

		protected override void SelfUpdate(float delta)
		{
			if (webView == null) {
				return;
			}
			var screenHeight = GameView.Instance.Size.Height;
			WindowRect wr = CalculateAABBInWorldSpace(this);
			layoutParams.Width = wr.Width;
			layoutParams.Height = wr.Height;
			layoutParams.LeftMargin = wr.X;
			layoutParams.TopMargin = wr.Y + screenHeight - wr.Height;
			if (webView.Parent == null) {
				MainActivity.Instance.ContentView.AddView(webView, layoutParams);
			} else {
				webView.LayoutParameters = layoutParams;
			}
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

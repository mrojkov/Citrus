#if ANDROID
using System;
using Android.Webkit;
using System.Drawing;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

using System;

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
			webView = new WebView(GameView.Instance.Context);
		}

		public override Node DeepCloneFast()
		{
			var result = (WebBrowser)base.DeepCloneFast();
			result.webView = new WebView(GameView.Instance.Context);
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

		protected override void SelfUpdate(float delta)
		{
			if (webView == null) {
				return;
			}
			float screenHeight = GameView.Instance.Size.Height;
			WindowRect wr = CalculateAABBInWorldSpace(this);
			float screenScale = 1;
			float Height = (float)wr.Height / screenScale;
			float offsetY = (screenHeight) - Height;
			var position = new PointF((float)wr.X / screenScale, (float)(wr.Y / screenScale) + offsetY);
			var size = new SizeF((float)wr.Width / screenScale, Height);
			if (webView.Parent == null) {
				var p = new RelativeLayout.LayoutParams(size.Width.Round(), size.Height.Round());
				p.LeftMargin = (int)position.X;
				p.TopMargin = (int)position.Y;
				MainActivity.Instance.ContentView.AddView(webView, p);
			} else {
				webView.Layout(position.X.Round(), position.Y.Round(), 
					(position.X + size.Width).Round(), 
					(position.Y + size.Height).Round());
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

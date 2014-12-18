#if WIN
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Lime
{
	public class WebBrowser : Image
	{
		public Uri Url { get { return browser.Url; } set { browser.Url = value; } }

		private System.Windows.Forms.WebBrowser browser;

		public WebBrowser() : base()
		{
			browser = new System.Windows.Forms.WebBrowser();
			browser.DocumentCompleted += browser_DocumentCompleted;
			ClearTexture();
		}

		public WebBrowser(Widget parentWidget): this()
		{
			AddToWidget(parentWidget);
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.AddNode(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRightTopBottom;
			browser.Width = (int)Width;
			browser.Height = (int)Height;
			browser.ScrollBarsEnabled = false;
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			if (browser != null) {
				browser.Width = (int)Width;
				browser.Height = (int)Height;
			}
		}

		void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			// FIXME: It's hard to make a browser to work over an OpenGL surface, so at least draw it as a picture.
			var bitmap = new System.Drawing.Bitmap((int)Width, (int)Height);
			browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
			var m = new MemoryStream(1024 * 1024);
			bitmap.Save(m, System.Drawing.Imaging.ImageFormat.Png);
			m.Position = 0;
			var t2d = new Texture2D();
			t2d.LoadImage(m);
			Texture = t2d;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
		}

		public override void Render()
		{
			base.Render();
		}
		
		public override void Dispose()
		{
			if (browser != null)
				browser.Dispose();
			ClearTexture();
		}

		private void ClearTexture()
		{
			var t2d = new Texture2D();
			t2d.LoadImage(new Color4[] { }, 0, 0, false);
			Texture = t2d;
		}
	}
}
#endif
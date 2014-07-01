#if !iOS
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Lime
{
	public class WebBrowser : Image, IDisposable
	{
		public Uri Url { get { return browser.Url; } set { SetUrl(value); } }

		private System.Windows.Forms.WebBrowser browser;

		public WebBrowser() : base(new Texture2D())
		{
			browser = new System.Windows.Forms.WebBrowser();
			browser.DocumentCompleted += browser_DocumentCompleted;
		}

		public WebBrowser(Widget parentWidget): this()
		{
			parentWidget.AddNode(this);
			Size = parentWidget.Size;
			browser.Width = (int)Width;
			browser.Height = (int)Height;
			browser.ScrollBarsEnabled = false;
		}

		private Texture2D browserImage = new Texture2D();

		void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			// FIXME: It's hard to make a browser to work over an OpenGL surface, so at least draw it as a picture.
			var bitmap = new System.Drawing.Bitmap((int)Width, (int)Height);
			browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
			var m = new MemoryStream(1024 * 1024);
			bitmap.Save(m, System.Drawing.Imaging.ImageFormat.Png);
			m.Position = 0;
			(Texture as Texture2D).LoadImage(m);
		}
		
		private void SetUrl(Uri value)
		{
			browser.Navigate(value);
		}

		public void Dispose()
		{
			if (browser != null)
				browser.Dispose();
		}
	}
}
#endif
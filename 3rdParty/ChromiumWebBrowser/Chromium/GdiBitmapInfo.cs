using CefSharp.Internals;

namespace ChromiumWebBrowser
{
	public class GdiBitmapInfo : BitmapInfo
	{
		private bool createNewBitmap;

		public GdiBitmapInfo()
		{
			BytesPerPixel = 4;
		}

		public override bool CreateNewBitmap
		{
			get { return createNewBitmap; }
		}

		public override void ClearBitmap()
		{
			createNewBitmap = true;
		}
	}
}

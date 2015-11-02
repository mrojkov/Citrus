#if ANDROID
namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		private const string Tag = "Clipboard";

		public string Text
		{
			get
			{
				return string.Empty;
			}

			set
			{
			}
		}
	}
}
#endif


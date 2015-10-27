#if ANDROID
namespace Lime
{
	//Mock Android clipboard implementation
	public static partial class Clipboard
	{
		private static string GetTextImpl()
		{
		}

		private static void PutTextImpl(string text)
		{
		}
	}
}
#endif


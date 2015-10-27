#if iOS
namespace Lime
{
	//Mock iOS clipboard implementation
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


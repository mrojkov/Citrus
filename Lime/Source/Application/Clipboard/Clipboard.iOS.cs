#if iOS
namespace Lime
{
	//Mock iOS clipboard implementation
	public static partial class Clipboard
	{
		private static string GetTextImpl()
		{
			return null;
		}

		private static void PutTextImpl(string text)
		{
		}
	}
}
#endif



namespace Lime
{
	public static partial class Clipboard
	{
		public static string GetText()
		{
			return GetTextImpl();
		}

		public static void PutText(string text)
		{
			PutTextImpl(text);
		}
	}
}


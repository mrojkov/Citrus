
namespace Lime
{
	public static partial class Clipboard
	{
		private const string Tag = "Clipboard";

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


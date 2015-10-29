#if WIN
using WinForms = System.Windows.Forms;

namespace Lime
{
	public static partial class Clipboard
	{
		private static string GetTextImpl()
		{
			string pasteText = string.Empty;
			try {
				if (WinForms.Clipboard.ContainsText()) {
					pasteText = WinForms.Clipboard.GetText();
				}
			} catch (System.Exception ex) {
				Logger.Write("[{0}]: {1}", Tag, ex.Message);
			}
			return pasteText;
		}

		private static void PutTextImpl(string text)
		{
			if (text == null || text == string.Empty) {
				return;
			}
			try {
				WinForms.Clipboard.SetText(text);
			} catch (System.Exception ex) {
				Logger.Write("[{0}]: {1}", Tag, ex.Message);
			}
		}
	}
}

#endif

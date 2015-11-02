#if WIN
using WinForms = System.Windows.Forms;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		private const string Tag = "Clipboard";

		public string Text
		{
			get
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

			set
			{
				if (value == null || value == string.Empty) {
					return;
				}
				try {
					WinForms.Clipboard.SetText(value);
				} catch (System.Exception ex) {
					Logger.Write("[{0}]: {1}", Tag, ex.Message);
				}
			}
		}
	}
}
#endif


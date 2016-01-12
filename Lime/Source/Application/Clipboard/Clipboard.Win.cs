#if WIN
using WinForms = System.Windows.Forms;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			get
			{
				if (WinForms.Clipboard.ContainsText()) {
					return WinForms.Clipboard.GetText();
				}
				return string.Empty;
			}

			set
			{
				if (!string.IsNullOrEmpty(value)) {
					WinForms.Clipboard.SetText(value);
				}
			}
		}
	}
}
#endif

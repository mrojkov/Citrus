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

		private static class TypeData<T>
		{
			public static T Data { get; set; } = default;
		}

		public T GetData<T>() => TypeData<T>.Data;

		public void SetData<T>(T value) => TypeData<T>.Data = value;
	}
}
#endif

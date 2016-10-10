namespace Lime
{
	interface IClipboardImplementation
	{
		string Text { get; set; }
	}

	public static class Clipboard
	{
#if MONOMAC
		private static readonly IClipboardImplementation implementation;
#else
		private static readonly IClipboardImplementation implementation = new ClipboardImplementation();
#endif

		public static string Text
		{
			get { return implementation.Text; }
			set { implementation.Text = value; }
		}
	}
}

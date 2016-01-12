namespace Lime
{
	interface IClipboardImplementation
	{
		string Text { get; set; }
	}

	public static class Clipboard
	{
		private static readonly IClipboardImplementation implementation = new ClipboardImplementation();

		public static string Text
		{
			get { return implementation.Text; }
			set { implementation.Text = value; }
		}
	}
}

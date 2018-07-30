namespace Lime
{
	interface IClipboardImplementation
	{
		string Text { get; set; }
		T GetData<T>();
		void SetData<T>(T value);
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

		public static void SetData<T>(T data)
		{
			implementation.SetData(data);
		}

		public static T GetData<T>()
		{
			return implementation.GetData<T>();
		}
	}
}

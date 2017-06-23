using System.IO;
using System.Text;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	public class TextView : ScrollViewWidget
	{
		private List<SimpleText> lines = new List<SimpleText>();

		public TextView()
		{
			Theme.Current.Apply(this, typeof(ScrollViewWidget));
			Behaviour.Content.Padding = new Thickness(4);
			Behaviour.Content.Layout = new VBoxLayout();
			Behaviour.Frame.CompoundPresenter.Add(new DesktopTheme.BorderedFramePresenter(DesktopTheme.Colors.WhiteBackground, DesktopTheme.Colors.ControlBorder));
		}

		public void Append(string text)
		{
			var lastLine = lines.Count > 0 ? lines[lines.Count - 1] : null;
			foreach (var l in text.Split('\n')) {
				if (lastLine != null) {
					lastLine.Text += l;
					lastLine = null;
				} else {
					var line = new SimpleText(l);
					lines.Add(line);
					Behaviour.Content.AddNode(line);
				}
			}
		}

		public override string Text
		{
			get
			{
				var sb = new StringBuilder();
				foreach (var l in lines) {
					sb.AppendLine(l.Text);
				}
				return sb.ToString();
			}
			set
			{
				Clear();
				Append(value);
			}
		}

		public void ScrollToEnd()
		{
			Behaviour.ScrollPosition = Behaviour.MaxScrollPosition;
		}

		public void Clear()
		{
			Behaviour.Content.Nodes.Clear();
			lines.Clear();
			Behaviour.ScrollPosition = 0;
		}

		public TextWriter GetTextWriter() => new Writer(this);

		private class Writer : TextWriter
		{
			private readonly TextView text;

			public Writer(TextView text)
			{
				this.text = text;
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			public override void Write(string value)
			{
				Application.InvokeOnMainThread(() => {
					text.Append(value);
					text.ScrollToEnd();
				});
			}

			public override Encoding Encoding { get; }
		}
	}
}
#if !ANDROID && !iOS
using System;
using System.Text;
using System.Collections.Generic;

namespace Lime
{
	public class ThemedTextView : ThemedScrollView
	{
		private List<SimpleText> lines = new List<SimpleText>();

		public override bool IsNotDecorated() => false;

		public ThemedTextView()
		{
			Behaviour.Content.Padding = new Thickness(4);
			Behaviour.Content.Layout = new VBoxLayout();
			Behaviour.Frame.CompoundPresenter.Add(new ThemedFramePresenter(Theme.Colors.WhiteBackground, Theme.Colors.ControlBorder));
		}

		public void Append(string text)
		{
			var lastLine = lines.Count > 0 ? lines[lines.Count - 1] : null;
			foreach (var l in text.Split('\n')) {
				if (lastLine != null) {
					lastLine.Text += l;
					lastLine = null;
				} else {
					var line = new ThemedSimpleText(l);
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
	}
}
#endif
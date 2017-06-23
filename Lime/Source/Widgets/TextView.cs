using System;
using System.Collections.Generic;
using System.Text;

namespace Lime
{
	public class TextView : ScrollViewWidget
	{
		private List<SimpleText> lines = new List<SimpleText>();

		public TextView()
		{
			Theme.Current.Apply(this);
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
	}
}
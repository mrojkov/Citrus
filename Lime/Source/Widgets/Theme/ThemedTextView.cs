#if !ANDROID && !iOS
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedTextView : ThemedScrollView
	{
		public class TextLineMultiplicity : NodeComponent
		{
			public int Multiplicity = 1;
		}
		private readonly List<SimpleText> lines = new List<SimpleText>();

		public override bool IsNotDecorated() => false;

		public bool SquashDuplicateLines { get; set; } = false;

		public ThemedTextView()
		{
			Behaviour.Content.Padding = new Thickness(4);
			Behaviour.Content.Layout = new VBoxLayout();
			Behaviour.Frame.CompoundPresenter.Add(new ThemedFramePresenter(Theme.Colors.WhiteBackground, Theme.Colors.ControlBorder));
		}

		public void Append(string text)
		{
			var lastLine = lines.Count > 0 ? lines[lines.Count - 1] : null;
			SimpleText lastNonSentinelLine = null;
			if (SquashDuplicateLines) {
				lastNonSentinelLine = lines.Count > 1 ? lines[lines.Count - 2] : null;
			}
			var newLines = text.Split('\n');
			for (int i = 0; i < newLines.Length; i++) {
				var l = newLines[i];
				if (SquashDuplicateLines) {
					if (lastNonSentinelLine != null && lastNonSentinelLine.Text == l) {
						lastNonSentinelLine.Components.Get<ThemedTextView.TextLineMultiplicity>().Multiplicity++;
						lastNonSentinelLine.Invalidate();
						//TODO: invalidate window only if it isn't docked
						(GetRoot() as WindowWidget)?.Window.Invalidate();
						continue;
					}
				}
				if (lastLine != null) {
					lastLine.Text += l;
					if (SquashDuplicateLines) {
						lastNonSentinelLine = lastLine;
					}
					lastLine = null;
				} else {
					var line = new ThemedSimpleText(l);
					line.TextProcessor += ProcessTextLine;
					line.Components.Add(new TextLineMultiplicity());
					lines.Add(line);
					Behaviour.Content.AddNode(line);
				}
			}
		}

		private void ProcessTextLine(ref string text, Widget w)
		{
			var m = w.Components.Get<TextLineMultiplicity>().Multiplicity;
			text = m == 1 ? text : $"{text} ({m})";
		}

		public bool IsEmpty => lines.Count == 0;

		public SimpleText LastLine => lines[lines.Count - 1];

		public override string Text
		{
			get
			{
				var sb = new StringBuilder();
				foreach (var l in lines) {
					var m = l.Components.Get<TextLineMultiplicity>().Multiplicity;
					for (int i = 0; i < m; i++) {
						sb.AppendLine(l.Text);
					}
				}
				return sb.ToString();
			}
			set
			{
				Clear();
				Append(value);
			}
		}

		public string DisplayText
		{
			get {
				var sb = new StringBuilder();
				foreach (var l in lines) {
					sb.AppendLine(l.DisplayText);
				}
				return sb.ToString();
			}
			set {
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

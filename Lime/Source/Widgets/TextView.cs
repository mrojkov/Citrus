using System;
using System.Collections.Generic;

namespace Lime
{
	public class TextView : Frame
	{
		private SimpleText text;
		private ScrollViewWithSlider scrollView;

		public TextView()
		{
			Theme.Current.Apply(this);
			scrollView = new ScrollViewWithSlider(this);
			text = new SimpleText();
			text.OverflowMode = TextOverflowMode.Ignore;
			scrollView.Content.Nodes.Add(text);
		}

		public override string Text
		{
			get { return text.Text; }
			set
			{
				text.Text = value;
				scrollView.Content.Size = text.Size = text.MeasureText().Size;
			}
		}
	}
}
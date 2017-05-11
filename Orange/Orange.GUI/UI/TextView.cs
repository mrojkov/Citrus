using System.IO;
using System.Text;
using Lime;

namespace Orange
{
	public class TextView : Widget
	{
		private readonly TextViewEditBox editor;

		public TextView()
		{
			editor = new TextViewEditBox {
				Anchors = Anchors.LeftRightTopBottom
			};
			AddNode(editor);
		}

		public override string Text
		{
			get { return editor.Text; }
			set { editor.Text = value; }
		}

		public void ScrollToEnd()
		{
			editor.Scroll.ScrollPosition = editor.Scroll.MaxScrollPosition;
		}

		public void Clear()
		{
			Text = string.Empty;
			editor.Scroll.ScrollPosition = editor.Scroll.MinScrollPosition;
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
					text.Text += value;
					text.ScrollToEnd();
				});
			}

			public override Encoding Encoding { get; }
		}

		private class TextViewEditBox : CommonEditBox
		{
			public TextViewEditBox()
			{
				Scroll.Dispose();
				Scroll = new ScrollView(this) { ScrollBySlider = true };
				TextWidget.Unlink();
				Scroll.Content.AddNode(TextWidget);
				Theme.Current.Apply(this, typeof(EditBox));
			}
		}
	}
}
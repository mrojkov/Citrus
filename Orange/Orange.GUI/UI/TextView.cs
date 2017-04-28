using System.IO;
using System.Text;
using Lime;

namespace Orange
{
	public class TextViewe : Widget
	{
		private Frame frame;
		private SimpleText text;

		public TextViewe()
		{
			frame = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Anchors = Anchors.LeftRightTopBottom
			};
			frame.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, w.Size, Color4.White);
			}));
			text = new SimpleText {
				VAlignment = VAlignment.Bottom,
				Size = new Vector2(int.MaxValue / 1000, int.MaxValue / 1000),
				Pivot = new Vector2(0, 1),
				Position = frame.Size * new Vector2(0, 1),
				Anchors = Anchors.Bottom | Anchors.Left
			};
			frame.AddNode(text);
			AddNode(frame);
		}

		public override string Text
		{
			get => text.Text;
			set => text.Text = value;
		}

		public TextWriter GetTextWriter() => new Writer(this);

		private class Writer : TextWriter
		{
			private readonly TextViewe text;

			public Writer(TextViewe text)
			{
				this.text = text;
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			public override void Write(string value)
			{
				Application.InvokeOnMainThread(() => text.Text += value);
			}

			public override Encoding Encoding { get; }
		}
	}
}
using System;

namespace Lime
{
	public abstract class CommonEditBox : Frame
	{
		public bool IsReadOnly
		{
			get { return isReadOnly; }
			set
			{
				if (isReadOnly != value) {
					isReadOnly = value;
					if (Editor != null) {
						Editor.ProcessInput = !isReadOnly;
					}
				}
			}
		}

		public Editor Editor
		{
			get { return editor; }
			set
			{
				editor = value;
				editor.ProcessInput = !isReadOnly;
			}
		}

		public SimpleText TextWidget { get; private set; }
		public event Action<string> Submitted;
		public ScrollView Scroll { get; protected set; }

		private Editor editor;
		private bool isReadOnly;

		public override string Text
		{
			get { return TextWidget.Text; }
			set { TextWidget.Text = value; }
		}

		public CommonEditBox()
		{
			Scroll = new ScrollView(this, ScrollDirection.Horizontal);
			Scroll.CanScroll = false;
			TextWidget = new SimpleText();
			TextWidget.Height = Height;
			Scroll.Content.AddNode(TextWidget);
		}

		protected override void Awake()
		{
			TextWidget.Submitted += text => Submitted?.Invoke(text);
			base.Awake();
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			if (TextWidget != null) { // Size is assigned in Widget constructor.
				TextWidget.Height = Height;
				Editor?.AdjustSizeAndScrollToCaret();
			}
		}

		protected void OnSubmit() => Submitted?.Invoke(Text);
	}

	public class EditBox : CommonEditBox { }

	public class NumericEditBox : CommonEditBox
	{
		public float Step = 0.1f;

		public event Action BeginSpin;
		public event Action EndSpin;

		public float Value
		{
			get
			{
				float v;
				return float.TryParse(Text, out v) ? v : 0;
			}
			set
			{
				Text = value.ToString();
				OnSubmit();
			}
		}

		public void RaiseBeginSpin() => BeginSpin?.Invoke();
		public void RaiseEndSpin() => EndSpin?.Invoke();
	}
}


using System;

namespace Lime
{
	public abstract class CommonEditBox : Widget
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

		public Frame ScrollWidget { get; private set; }
		public SimpleText TextWidget { get; private set; }
		public event Action<string> Submitted;
		public ScrollView ScrollView { get; protected set; }

		private Editor editor;
		private bool isReadOnly;

		public override string Text
		{
			get { return TextWidget.Text; }
			set { TextWidget.Text = value; }
		}

		public CommonEditBox()
		{
			ScrollWidget = new Frame();
			ScrollView = new ScrollView(ScrollWidget, ScrollDirection.Horizontal);
			ScrollView.CanScroll = false;
			TextWidget = new SimpleText();
			TextWidget.Height = Height;
			ScrollView.Content.AddNode(TextWidget);
			Layout = new StackLayout();
			AddNode(ScrollWidget);
		}

		protected override void Awake()
		{
			TextWidget.Submitted += text => Submitted?.Invoke(text);
			base.Awake();
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


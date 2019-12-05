using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
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
			ScrollView.Content.Layout = new StackLayout();
			TextWidget = new SimpleText();
			TextWidget.Height = Height;
			ScrollView.Content.AddNode(TextWidget);
			Layout = new StackLayout();
			AddNode(ScrollWidget);
			Awoke += Awake;
		}

		private static void Awake(Node owner)
		{
			var ceb = (CommonEditBox)owner;
			ceb.TextWidget.Submitted += text => ceb.Submitted?.Invoke(text);
		}

		protected void OnSubmit() => Submitted?.Invoke(Text);
	}

	[YuzuDontGenerateDeserializer]
	public class EditBox : CommonEditBox { }

	[YuzuDontGenerateDeserializer]
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


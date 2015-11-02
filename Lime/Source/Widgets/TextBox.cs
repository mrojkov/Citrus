using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	public interface IKeyboardInputProcessor
	{
		bool Visible { get; }
		string Text { get; }
	}

	/// <summary>
	/// Поле ввода текста
	/// </summary>
	[ProtoContract]
	public class TextBox : Widget, IKeyboardInputProcessor
	{
		private string text;
		private float caretBlinkPhase;
		private float lastCharTimer;
		private char lastChar;
		private float lastCharShowTime = 0.5f;

		[ProtoMember(1)]
		public SerializableFont Font = new SerializableFont();

		[ProtoMember(2)]
		public override string Text {
			get { return text ?? ""; } 
			set { text = value; } 
		}

		[ProtoMember(3)]
		public int MaxTextLength = 50;

		[ProtoMember(4)]
		public float FontHeight = 15;

		[ProtoMember(5)]
		public float BorderWidth = 5;

		[ProtoMember(6)]
		public char CaretChar = '_';

		[ProtoMember(7)]
		public float CaretBlinkPeriod = 0.5f;

		/// <summary>
		/// Можно вводить только цифры
		/// </summary>
		[ProtoMember(8)]
		public bool Numeric;

		/// <summary>
		/// Выравнивание текста по горизонтали
		/// </summary>
		[ProtoMember(9)]
		public HAlignment HAlignment;

		[ProtoMember(10)]
		public bool Enabled { get; set; }

		[ProtoMember(11)]
		public bool Autofocus { get; set; }

		/// <summary>
		/// Текст заменяется звездочками
		/// </summary>
		[ProtoMember(12)]
		public bool PasswordField { get; set; }

		public TextBox()
		{
			Enabled = true;
			HitTestMask = ControlsHitTestMask;
		}

		protected override void SelfUpdate(float delta)
		{
			lastCharTimer -= delta;
			var world = World.Instance;
			if (!Enabled) {
				if (world.ActiveTextWidget == this) {
					world.ActiveTextWidget = null;
				}
			} else {
				if (world.ActiveTextWidget == null && Autofocus) {
					world.ActiveTextWidget = this;
				}
				if (Input.WasMouseReleased() && IsMouseOver()) {
					world.ActiveTextWidget = this;
				}
			}
		}

		protected override void SelfLateUpdate(float delta)
		{
			var world = World.Instance;
			if (Enabled) {
				if (world.ActiveTextWidget == this) {
					if (Input.TextInput != null) {
						ProcessInput();
					}
				}
				if (world.ActiveTextWidget == this) {
					world.IsActiveTextWidgetUpdated = true;
				}
				caretBlinkPhase += delta;
			}
		}

		private void ProcessInput()
		{
			foreach (char c in Input.TextInput) {
				if (c >= 32 && Text.Length < MaxTextLength) {
					char? charToAdd = null;
					if (Numeric) {
						float foo;
						if (c != ' ' && ((c == '-' && Text == "") || float.TryParse(Text + c, out foo))) {
							charToAdd = c;
						}
					} else {
						charToAdd = c;
					}
					if (charToAdd != null) {
						Text += charToAdd.Value;
						lastChar = charToAdd.Value;
						lastCharTimer = lastCharShowTime;
					}
				} else if (Text.Length > 0 && c == 8) {
					Text = Text.Remove(Text.Length - 1);
				}
			}
		}

		public override void Render()
		{
			string text = Text;
			if (PasswordField && Text.Length > 0) {
				text = new string('*', Text.Length - 1);
				if (lastCharTimer > 0) {
					text += lastChar;
				} else {
					text += '*';
				}
			}
			if (World.Instance.ActiveTextWidget == this) {
				text += CaretChar;
			}
			int start = 0;
			int length = text.Length;
			Vector2 extent = Renderer.MeasureTextLine(Font.Instance, text, FontHeight);
			while (extent.X > Width - BorderWidth * 2 && length > 0) {
				start++;
				length--;
				extent = Renderer.MeasureTextLine(Font.Instance, text, FontHeight, start, length);
			}
			if (World.Instance.ActiveTextWidget == this) {
				if (caretBlinkPhase % CaretBlinkPeriod < CaretBlinkPeriod / 2 && length > 0) {
					length--;
				}
			}
			var textPosition = new Vector2(BorderWidth, (Height - FontHeight) / 2);
			if (HAlignment == HAlignment.Right)
				textPosition.X = Size.X - BorderWidth - extent.X;
			else if (HAlignment == HAlignment.Center)
				textPosition.X = (Size.X - extent.X) * 0.5f;
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.DrawTextLine(Font.Instance, textPosition, text, GlobalColor, FontHeight, start, length);
		}
	}
}

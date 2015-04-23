using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public interface ICaretParams
	{
		Widget CaretWidget { get; set; }
		float BlinkInterval { get; set; }
	}

	public class CaretParams : ICaretParams
	{
		public Widget CaretWidget { get; set; }
		public float BlinkInterval { get; set; }

		public CaretParams()
		{
			BlinkInterval = 0.5f;
		}
	}

	public class VertiсalLineCaret : Polyline
	{
		public VertiсalLineCaret(SimpleText text) : base(2.0f)
		{
			Points.Add(Vector2.Zero);
			Points.Add(Vector2.Down * text.FontHeight);
			Color = text.Color;
		}
	}

	public class CaretDisplay
	{
		private Widget container;
		private ICaretPosition caretPos;
		private ICaretParams caretParams;

		public CaretDisplay(Widget container, ICaretPosition caretPos, ICaretParams caretParams)
		{
			this.container = container;
			this.caretPos = caretPos;
			this.caretParams = caretParams;
			container.AddNode(caretParams.CaretWidget);
			container.Tasks.Add(CaretDisplayTask());
		}

		private IEnumerator<object> CaretDisplayTask()
		{
			var w = caretParams.CaretWidget;
			var time = 0f;
			bool blinkOn = true;
			while (true) {
				time += container.Tasks.Delta;
				if (time > caretParams.BlinkInterval && caretParams.BlinkInterval > 0f) {
					blinkOn = !blinkOn;
					time = 0f;
				}
				var newPos = caretPos.WorldPos;
				if (!w.Position.Equals(newPos)) {
					w.Position = newPos;
					blinkOn = true;
					time = 0f;
				}
				w.Visible = caretPos.IsVisible && blinkOn;
				yield return null;
			}
		}
	}

	public interface IEditorParams
	{
		float KeyRepeatDelay { get; set; }
		float KeyRepeatInterval { get; set; }
		int MaxLength { get; set; }
		int MaxLines { get; set; }
		float MaxHeight { get; set; }
		char? PasswordChar { get; set; }
		float PasswordLastCharShowTime { get; set; }

		bool IsAcceptableLength(int length);
		bool IsAcceptableLines(int lines);
		bool IsAcceptableHeight(float height);
	}

	public class EditorParams : IEditorParams
	{
		public float KeyRepeatDelay { get; set; }
		public float KeyRepeatInterval { get; set; }
		public int MaxLength { get; set; }
		public int MaxLines { get; set; }
		public float MaxHeight { get; set; }
		public char? PasswordChar { get; set; }
		public float PasswordLastCharShowTime { get; set; }

		public EditorParams()
		{
			KeyRepeatDelay = 0.5f;
			KeyRepeatInterval = 0.05f;
			PasswordLastCharShowTime = 1.0f;
		}

		public bool IsAcceptableLength(int length) { return MaxLength <= 0 || length <= MaxLength; }
		public bool IsAcceptableLines(int lines) { return MaxLines <= 0 || lines <= MaxLines; }
		public bool IsAcceptableHeight(float height) { return MaxHeight <= 0 || height <= MaxHeight; }
	}

	public class Editor
	{
		private Widget container;
		private IKeyboardInputProcessor textInputProcessor;
		private IText text;
		private ICaretPosition caretPos;
		private IEditorParams editorParams;

		public Editor(Widget container, ICaretPosition caretPos, IEditorParams editorParams)
		{
			this.container = container;
			this.textInputProcessor = (IKeyboardInputProcessor)container;
			text = (IText)container;
			text.TrimWhitespaces = false;
			this.caretPos = caretPos;
			this.editorParams = editorParams;
			container.Tasks.Add(FocusTask());
			container.Tasks.Add(HandleKeyboardTask());
			container.Tasks.Add(EnforceDisplayTextTask());
		}

		private bool IsActive() { return World.Instance.ActiveTextWidget == textInputProcessor; }

		private IEnumerator<object> FocusTask()
		{
			var world = World.Instance;
			while (true) {
				if (container.WasClicked()) {
					world.ActiveTextWidget = textInputProcessor;
					world.IsActiveTextWidgetUpdated = true;
				}
				caretPos.IsVisible = IsActive();
				if (IsActive())
					world.IsActiveTextWidgetUpdated = true;
				yield return null;
			}
		}

		private float cursorKeyDownTime;
		private Key prevKeyPressed = 0;
		private Key keyPressed;

		private bool CheckCursorKey(Key key)
		{
			if (!container.Input.IsKeyPressed(key) || keyPressed != 0)
				return false;
			keyPressed = key;
			if (key != prevKeyPressed)
				cursorKeyDownTime = editorParams.KeyRepeatDelay;
			else if (cursorKeyDownTime <= 0)
				cursorKeyDownTime = editorParams.KeyRepeatInterval;
			else
				return false;
			return true;
		}

		private void InsertChar(char ch)
		{
			if (
				caretPos.TextPos >= 0 && caretPos.TextPos <= text.Text.Length &&
				editorParams.IsAcceptableLength(text.Text.Length + 1)
			) {
				var newText = text.Text.Insert(caretPos.TextPos, ch.ToString());
				if (editorParams.MaxHeight <= 0 || editorParams.IsAcceptableHeight(CalcTextHeight(newText))) {
					text.Text = newText;
					caretPos.TextPos++;
				}
			}
		}

		private float CalcTextHeight(string s)
		{
			var displayText = text.DisplayText;
			text.DisplayText = s;
			var height = text.MeasureText().Height;
			text.DisplayText = displayText;
			return height;
		}

		private void HandleCursorKeys()
		{
			cursorKeyDownTime -= container.Tasks.Delta;
			keyPressed = 0;
			if (CheckCursorKey(Key.Left))
				caretPos.TextPos--;
			if (CheckCursorKey(Key.Right))
				caretPos.TextPos++;
			if (CheckCursorKey(Key.Up))
				caretPos.Line--;
			if (CheckCursorKey(Key.Down))
				caretPos.Line++;
			if (CheckCursorKey(Key.Home))
				caretPos.Pos = 0;
			if (CheckCursorKey(Key.End))
				caretPos.Pos = int.MaxValue;
			if (CheckCursorKey(Key.Delete)) {
				if (caretPos.TextPos >= 0 && caretPos.TextPos < text.Text.Length) {
					text.Text = text.Text.Remove(caretPos.TextPos, 1);
					caretPos.TextPos--;
					caretPos.TextPos++; // Enforce revalidation.
				}
			}
			if (CheckCursorKey(Key.Enter) && editorParams.IsAcceptableLines(text.Text.Count(ch => ch == '\n') + 2))
				InsertChar('\n');
#if WIN
			if (container.Input.IsKeyPressed(Key.ControlLeft) && CheckCursorKey(Key.V)) {
				foreach (var ch in System.Windows.Forms.Clipboard.GetText())
					InsertChar(ch);
			}
#endif
			prevKeyPressed = keyPressed;
		}

		private float lastCharShowTimeLeft = 0f;
		private void HandleTextInput()
		{
			if (container.Input.TextInput == null)
				return;
			foreach (var ch in container.Input.TextInput) {
				// Some platforms, notably iOS, do not generate Key.BackSpace.
				// OTOH, '\b' is emulated everywhere.
				if (ch == '\b') {
					if (caretPos.TextPos > 0 && caretPos.TextPos <= text.Text.Length) {
						caretPos.TextPos--;
						text.Text = text.Text.Remove(caretPos.TextPos, 1);
					}
				}
				else if (ch >= ' ') {
					InsertChar(ch);
					lastCharShowTimeLeft = editorParams.PasswordLastCharShowTime;
				}
			}
		}

		private void HandleMouse()
		{
			if (container.WasClicked()) {
				caretPos.WorldPos =
					container.LocalToWorldTransform.CalcInversed().TransformVector(container.Input.MousePosition);
			}
		}

		private IEnumerator<object> HandleKeyboardTask()
		{
			while (true) {
				if (IsActive()) {
					HandleCursorKeys();
					HandleTextInput();
					HandleMouse();
				}
				yield return null;
			}
		}

		private IEnumerator<object> EnforceDisplayTextTask()
		{
			while (true) {
				if (editorParams.PasswordChar != null && text.Text != "") {
					lastCharShowTimeLeft -= TaskList.Current.Delta;
					text.DisplayText = new string(editorParams.PasswordChar.Value, text.Text.Length - 1);
					text.DisplayText += lastCharShowTimeLeft > 0 ? text.Text.Last() : editorParams.PasswordChar; 
				}
				else {
					text.DisplayText = text.Text; // Disable localization.
				}
				yield return null;
			}
		}

	}
}

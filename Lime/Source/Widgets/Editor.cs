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
				yield return 0;
			}
		}
	}

	public interface IEditorParams
	{
		float KeyPressInterval { get; set; }
	}

	public class EditorParams : IEditorParams
	{
		public float KeyPressInterval { get; set; }

		public EditorParams()
		{
			KeyPressInterval = 0.05f;
		}
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
			this.caretPos = caretPos;
			this.editorParams = editorParams;
			container.Tasks.Add(FocusTask());
			container.Tasks.Add(HandleKeyboardTask());
		}

		private bool IsActive() { return World.Instance.ActiveTextWidget == textInputProcessor; }

		private IEnumerator<object> FocusTask()
		{
			var world = World.Instance;
			while (true) {
				if (Input.WasMouseReleased() && container.IsMouseOver()) {
					world.ActiveTextWidget = textInputProcessor;
					world.IsActiveTextWidgetUpdated = true;
				}
				caretPos.IsVisible = IsActive();
				if (IsActive())
					world.IsActiveTextWidgetUpdated = true;
				yield return 0;
			}
		}

		private float cursorKeyDownTime;
		private Key prevKeyPressed = 0;
		private Key keyPressed;

		private bool CheckCursorKey(Key key)
		{
			if (!Input.IsKeyPressed(key) || keyPressed != 0)
				return false;
			keyPressed = key;
			if (key == prevKeyPressed && cursorKeyDownTime < editorParams.KeyPressInterval)
				return false;
			cursorKeyDownTime = 0;
			return true;
		}

		private void InsertChar(char ch)
		{
			if (caretPos.TextPos >= 0 && caretPos.TextPos <= text.Text.Length) {
				text.Text = text.Text.Insert(caretPos.TextPos, ch.ToString());
				caretPos.TextPos++;
			}
		}

		private void HandleCursorKeys()
		{
			cursorKeyDownTime += container.Tasks.Delta;
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
			if (CheckCursorKey(Key.BackSpace)) {
				if (caretPos.TextPos > 0) {
					caretPos.TextPos--;
					text.Text = text.Text.Remove(caretPos.TextPos, 1);
				}
			}
			if (CheckCursorKey(Key.Delete)) {
				if (caretPos.TextPos >= 0 && caretPos.TextPos < text.Text.Length) {
					text.Text = text.Text.Remove(caretPos.TextPos, 1);
					caretPos.TextPos--;
					caretPos.TextPos++; // Enforce revalidation.
				}
			}
			if (CheckCursorKey(Key.Enter))
				InsertChar('\n');
			prevKeyPressed = keyPressed;
		}

		private void HandleTextInput()
		{
			if (Input.TextInput == null)
				return;
			foreach (var ch in Input.TextInput) {
				if (ch >= ' ')
					InsertChar(ch);
			}
		}

		private IEnumerator<object> HandleKeyboardTask()
		{
			while (true) {
				if (IsActive()) {
					HandleCursorKeys();
					HandleTextInput();
				}
				yield return 0;
			}
		}
	}
}

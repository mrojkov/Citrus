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
		bool FollowTextColor { get; set; }
	}

	public class CaretParams : ICaretParams
	{
		public Widget CaretWidget { get; set; }
		public float BlinkInterval { get; set; }
		public bool FollowTextColor { get; set; }

		public CaretParams()
		{
			BlinkInterval = 0.5f;
		}
	}

	public class VerticalLineCaret : Polyline
	{
		public VerticalLineCaret(SimpleText text) : base(2.0f)
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
				if (caretParams.FollowTextColor) {
					w.Color = container.Color;
				}
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

	public interface IFocusable
	{
		bool IsFocused();
		void Focus();
	}

	public class Editor : IFocusable
	{
		public readonly Widget Container;
		private IKeyboardInputProcessor textInputProcessor;
		private IText text;
		private ICaretPosition caretPos;
		private IEditorParams editorParams;

		public Editor(Widget container, ICaretPosition caretPos, IEditorParams editorParams)
		{
			Container = container;
			textInputProcessor = (IKeyboardInputProcessor)container;
			text = (IText)container;
			text.TrimWhitespaces = false;
			this.caretPos = caretPos;
			this.editorParams = editorParams;
			container.Tasks.Add(FocusTask(), this);
			container.Tasks.Add(HandleInputTask(), this);
			container.Tasks.Add(EnforceDisplayTextTask(), this);
		}

		public void Unlink()
		{
			if (IsFocused()) {
				World.Instance.ActiveTextWidget = null;
				caretPos.IsVisible = false;
			}
			Container.Tasks.StopByTag(this);
		}

		public void Focus()
		{
			World.Instance.ActiveTextWidget = textInputProcessor;
			World.Instance.IsActiveTextWidgetUpdated = true;
		}

		public bool IsFocused() { return World.Instance.ActiveTextWidget == textInputProcessor; }

		private IEnumerator<object> FocusTask()
		{
			while (true) {
				if (Container.WasClicked())
					Focus();
				caretPos.IsVisible = IsFocused();
				if (IsFocused())
					World.Instance.IsActiveTextWidgetUpdated = true;
				yield return null;
			}
		}

		private float cursorKeyDownTime;
		private Key prevKeyPressed = 0;
		private Key keyPressed;

		private bool CheckCursorKey(Key key)
		{
			if (!Container.Input.IsKeyPressed(key) || keyPressed != 0)
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
			cursorKeyDownTime -= Container.Tasks.Delta;
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
			if (Container.Input.IsKeyPressed(Key.ControlLeft) && CheckCursorKey(Key.V)) {
				foreach (var ch in System.Windows.Forms.Clipboard.GetText())
					InsertChar(ch);
			}
#endif
			prevKeyPressed = keyPressed;
		}

		private float lastCharShowTimeLeft = 0f;
		private void HandleTextInput()
		{
			if (Container.Input.TextInput == null)
				return;
			foreach (var ch in Container.Input.TextInput) {
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
			if (Container.WasClicked()) {
				caretPos.WorldPos =
					Container.LocalToWorldTransform.CalcInversed().TransformVector(Container.Input.MousePosition);
			}
		}

		private IEnumerator<object> HandleInputTask()
		{
			while (true) {
				if (IsFocused()) {
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

	public struct Shortcut
	{
		public Key Modifier;
		public Key Main;

		public Shortcut(Key modifier, Key main)
		{
			Modifier = modifier;
			Main = main;
		}

		public static implicit operator Shortcut(Key main) { return new Shortcut(Key.Unknown, main); }

		public bool IsPressed(WidgetInput input)
		{
			return
				Main != Key.Unknown && input.WasKeyPressed(Main) &&
				input.IsSingleKeyPressed(Modifier, Key.Unknown + 1, Key.Menu);
		}
	}

	public static class ShortcutExt
	{
		public static Shortcut Plus(this Key modifier, Key main) { return new Shortcut(modifier, main); }
	}

	/// <summary>
	/// Controls switching of focus between <see cref="IFocusable"/> fields based on keyboard shortcuts.
	/// </summary>
	public class KeyboardFocusController
	{
		public Shortcut NextField = Key.Tab;
		public Shortcut PreviousField = Key.LShift.Plus(Key.Tab);
		public Shortcut NextFieldOrSubmit = Key.Enter;
		public Shortcut Cancel = Key.Escape;

		public event Action OnSubmit;
		public event Action OnCancel;

		public readonly Widget Parent;
		public readonly List<IFocusable> Fields = new List<IFocusable>();

		public KeyboardFocusController(Widget parent)
		{
			Parent = parent;
			Parent.Tasks.Add(FocusTask());
		}

		private int Next()
		{
			var focused = Fields.FindIndex(i => i.IsFocused());
			// Submit should work even without focusable entities.
			if (NextFieldOrSubmit.IsPressed(Parent.Input)) {
				if (focused + 1 < Fields.Count)
					return focused + 1;
				if (OnSubmit != null)
					OnSubmit();
			}
			if (focused < 0)
				return -1;
			if (NextField.IsPressed(Parent.Input))
				return (focused + 1) % Fields.Count;
			if (PreviousField.IsPressed(Parent.Input))
				return (focused + Fields.Count - 1) % Fields.Count;
			return -1;
		}

		private IEnumerator<object> FocusTask()
		{
			while (true) {
				if (Cancel.IsPressed(Parent.Input) && OnCancel != null) {
					OnCancel();
					yield break;
				}
				var next = Next();
				if (next >= 0) {
					yield return null;
					Fields[next].Focus();
				}
				yield return null;
			}
		}
	}
}

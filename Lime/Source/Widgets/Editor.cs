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

	/// <summary>
	/// Editor behaviour implemented over the given text display widget.
	/// </summary>
	public class Editor : IFocusable
	{
		public readonly Widget Container;
		public readonly IText Text;

		private IKeyboardInputProcessor textInputProcessor;
		private ICaretPosition caretPos;
		private IEditorParams editorParams;

		public Editor(Widget container, ICaretPosition caretPos, IEditorParams editorParams)
		{
			Container = container;
			textInputProcessor = (IKeyboardInputProcessor)container;
			Text = (IText)container;
			Text.TrimWhitespaces = false;
			this.caretPos = caretPos;
			this.editorParams = editorParams;
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
				caretPos.TextPos >= 0 && caretPos.TextPos <= Text.Text.Length &&
				editorParams.IsAcceptableLength(Text.Text.Length + 1)
			) {
				var newText = Text.Text.Insert(caretPos.TextPos, ch.ToString());
				if (editorParams.MaxHeight <= 0 || editorParams.IsAcceptableHeight(CalcTextHeight(newText))) {
					Text.Text = newText;
					caretPos.TextPos++;
				}
			}
		}

		private float CalcTextHeight(string s)
		{
			var displayText = Text.DisplayText;
			Text.DisplayText = s;
			var height = Text.MeasureText().Height;
			Text.DisplayText = displayText;
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
				if (caretPos.TextPos >= 0 && caretPos.TextPos < Text.Text.Length) {
					Text.Text = Text.Text.Remove(caretPos.TextPos, 1);
					caretPos.TextPos--;
					caretPos.TextPos++; // Enforce revalidation.
				}
			}
			if (CheckCursorKey(Key.Enter) && editorParams.IsAcceptableLines(Text.Text.Count(ch => ch == '\n') + 2))
				InsertChar('\n');
#if WIN
			//TODO: remake with new Clipboard class and hotkey handling
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
					if (caretPos.TextPos > 0 && caretPos.TextPos <= Text.Text.Length) {
						caretPos.TextPos--;
						Text.Text = Text.Text.Remove(caretPos.TextPos, 1);
						lastCharShowTimeLeft = editorParams.PasswordLastCharShowTime;
					}
				}
				else if (ch >= ' ') {
					InsertChar(ch);
					lastCharShowTimeLeft = editorParams.PasswordLastCharShowTime;
				}
			}
		}

		private IEnumerator<object> HandleInputTask()
		{
			while (true) {
				var wasClicked = Container.WasClicked();
				if (wasClicked)
					Focus();
				caretPos.IsVisible = IsFocused();

				if (IsFocused()) {
					World.Instance.IsActiveTextWidgetUpdated = true;
					HandleCursorKeys();
					if (IsHotKeyPressed()) {
						HandleHotKeys();
					} else {
						HandleTextInput();
					}
					if (wasClicked) {
						var t = Container.LocalToWorldTransform.CalcInversed();
						caretPos.WorldPos = t.TransformVector(Container.Input.MousePosition);
					}
				}
				yield return null;
			}
		}

		private bool IsHotKeyPressed()
		{
#if MAC
			return (Input.IsKeyPressed(Key.LWin) || Input.IsKeyPressed(Key.RWin));
#elif WIN
			return (Input.IsKeyPressed(Key.LControl) || Input.IsKeyPressed(Key.RControl));
#else
			return false;
#endif
		}

		private void HandleHotKeys()
		{
			if (Input.IsKeyPressed(Key.V)) {
				PasteFromClipboard();
			} else if (Input.IsKeyPressed(Key.C)) {
				Clipboard.PutText(Text.Text);
			} else if (Input.IsKeyPressed(Key.X)) {
				Clipboard.PutText(Text.Text);
				Text.Text = String.Empty;
			} else if (Input.IsKeyPressed(Key.Z)) {
				//TODO: undo last action
			}
		}

		private void PasteFromClipboard()
		{
			string pasteText = Clipboard.GetText();
			if (pasteText == null) {
				return;
			}
			pasteText = pasteText
				.Replace(System.Environment.NewLine, "\n")
				.Replace('\t', ' ');
			int freeSpace = editorParams.MaxLength - Text.Text.Length;
			if (freeSpace > 0) {
				if (pasteText.Length > freeSpace) {
					pasteText = pasteText.Substring(0, freeSpace);
				}
				foreach (var ch in pasteText) {
					if (ch != '\n' || editorParams.IsAcceptableLines(Text.Text.Count(c => c == '\n') + 2)) {
						InsertChar(ch);
					} else {
						InsertChar(' ');
					}
				}
			}
		}

		private IEnumerator<object> EnforceDisplayTextTask()
		{
			while (true) {
				if (editorParams.PasswordChar != null && Text.Text != "") {
					lastCharShowTimeLeft -= TaskList.Current.Delta;
					Text.DisplayText = new string(editorParams.PasswordChar.Value, Text.Text.Length - 1);
					Text.DisplayText += lastCharShowTimeLeft > 0 ? Text.Text.Last() : editorParams.PasswordChar;
				}
				else {
					Text.DisplayText = Text.Text; // Disable localization.
				}
				yield return null;
			}
		}

	}

	/// <summary>
	/// Represents combination of a key with a keyboard modifier used to trigger some action.
	/// </summary>
	public struct Shortcut
	{
		/// <summary>
		/// Modifier is expected to be in range from Key.LShift to Key.Menu.<br/>
		/// Set Modifier to Key.Unknown to get a key without any modifier.<br/>
		/// </summary>
		public readonly Key Modifier;
		/// <summary>
		/// Set Main to Key.Unknown to disable shortcut.
		/// </summary>
		public readonly Key Main;

		public static Shortcut Disabled = Key.Unknown;

		public Shortcut(Key modifier, Key main)
		{
			Modifier = modifier;
			Main = main;
		}

		public static implicit operator Shortcut(Key main) { return new Shortcut(Key.Unknown, main); }

		public bool WasTriggered(WidgetInput input)
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
			if (NextFieldOrSubmit.WasTriggered(Parent.Input)) {
				if (focused + 1 < Fields.Count)
					return focused + 1;
				if (OnSubmit != null)
					OnSubmit();
			}
			if (focused < 0)
				return -1;
			if (NextField.WasTriggered(Parent.Input))
				return (focused + 1) % Fields.Count;
			if (PreviousField.WasTriggered(Parent.Input))
				return (focused + Fields.Count - 1) % Fields.Count;
			return -1;
		}

		private IEnumerator<object> FocusTask()
		{
			while (true) {
				if (Cancel.WasTriggered(Parent.Input) && OnCancel != null) {
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

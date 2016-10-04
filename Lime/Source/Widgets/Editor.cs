using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lime
{
	public interface ICaretPresenter : IPresenter
	{
		Vector2 Position { get; set; }
		Color4 Color { get; set; }
		bool Visible { get; set; }
		float Thickness { get; set; }
	}

	public interface ICaretParams
	{
		ICaretPresenter CaretPresenter { get; set; }
		float BlinkInterval { get; set; }
		bool FollowTextColor { get; set; }
	}

	public class CaretParams : ICaretParams
	{
		public ICaretPresenter CaretPresenter { get; set; }
		public float BlinkInterval { get; set; }
		public bool FollowTextColor { get; set; }

		public CaretParams()
		{
			BlinkInterval = 0.5f;
		}
	}

	public class VerticalLineCaret : CustomPresenter, ICaretPresenter
	{
		public Vector2 Position { get; set; }
		public Color4 Color { get; set; }
		public bool Visible { get; set; }
		public float Thickness { get; set; }

		public VerticalLineCaret()
		{
			Thickness = 1.0f;
			Color = Color4.Black;
		}

		public override void Render(Node node)
		{
			if (Visible) {
				var text = (SimpleText)node;
				text.PrepareRendererState();
				Renderer.DrawLine(Position, Position + Vector2.Down * text.FontHeight, Color, Thickness);
			}
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
			container.CompoundPostPresenter.Add(caretParams.CaretPresenter);
			container.Tasks.Add(CaretDisplayTask());
		}

		private IEnumerator<object> CaretDisplayTask()
		{
			var p = caretParams.CaretPresenter;
			var time = 0f;
			bool blinkOn = true;
			bool wasVisible = false;
			while (true) {
				if (caretPos.IsVisible) {
					time += Task.Current.Delta;
					if (time > caretParams.BlinkInterval && caretParams.BlinkInterval > 0f) {
						time = 0f;
						blinkOn = !blinkOn;
						Window.Current.Invalidate();
					}
					var newPos = caretPos.WorldPos;
					if (!p.Position.Equals(newPos) || !wasVisible) {
						p.Position = newPos;
						time = 0f;
						blinkOn = true;
						Window.Current.Invalidate();
					}
					p.Visible = blinkOn;
					if (caretParams.FollowTextColor) {
						p.Color = container.Color;
					}
				} else if (p.Visible) {
					p.Visible = false;
					Window.Current.Invalidate();
				}
				wasVisible = caretPos.IsVisible;
				yield return null;
			}
		}
	}

	public interface IEditorParams
	{
		int MaxLength { get; set; }
		int MaxLines { get; set; }
		float MaxHeight { get; set; }
		char? PasswordChar { get; set; }
		float PasswordLastCharShowTime { get; set; }
		Predicate<string> AcceptText { get; set; }

		bool IsAcceptableLength(int length);
		bool IsAcceptableLines(int lines);
		bool IsAcceptableHeight(float height);
	}

	public class EditorParams : IEditorParams
	{
		public int MaxLength { get; set; }
		public int MaxLines { get; set; }
		public float MaxHeight { get; set; }
		public char? PasswordChar { get; set; }
		public float PasswordLastCharShowTime { get; set; }
		public Predicate<string> AcceptText { get; set; }

		public EditorParams()
		{
#if WIN || MAC || MONOMAC
			PasswordLastCharShowTime = 0.0f;
#else
			PasswordLastCharShowTime = 1.0f;
#endif
		}

		public bool IsAcceptableLength(int length) { return MaxLength <= 0 || length <= MaxLength; }
		public bool IsAcceptableLines(int lines) { return MaxLines <= 0 || lines <= MaxLines; }
		public bool IsAcceptableHeight(float height) { return MaxHeight <= 0 || height <= MaxHeight; }

		public const NumberStyles numberStyle =
			NumberStyles.AllowDecimalPoint |
			NumberStyles.AllowLeadingSign;

		public static bool AcceptNumber(string s)
		{
			double temp;
			return s == "-" || Double.TryParse(s, numberStyle, CultureInfo.InvariantCulture, out temp);
		}
	}

	/// <summary>
	/// Editor behaviour implemented over the given text display widget.
	/// </summary>
	public class Editor
	{
		public readonly Widget Container;
		public readonly IText Text;
		public readonly IEditorParams EditorParams;

		private ICaretPosition caretPos;

		public Editor(Widget container, ICaretPosition caretPos, IEditorParams editorParams)
		{
			Container = container;
			Container.HitTestTarget = true;
			Text = (IText)container;
			Text.TrimWhitespaces = false;
			EditorParams = editorParams;
			this.caretPos = caretPos;
			Text.Localizable = false;
			if (editorParams.PasswordChar != null) {
				Text.TextProcessor += ProcessTextAsPassword;
				container.Tasks.Add(TrackLastCharInput, this);
			}
			container.Tasks.Add(HandleInputTask(), this);
		}

		private string PasswordChars(int length) { return new string(EditorParams.PasswordChar.Value, length); }

		private void ProcessTextAsPassword(ref string text)
		{
			if (text != "")
				text = isLastCharVisible ? PasswordChars(text.Length - 1) + text.Last() : PasswordChars(text.Length);
		}

		public void Unlink()
		{
			if (Container.IsFocused()) {
				Container.RevokeFocus();
				caretPos.IsVisible = false;
			}
			Container.Tasks.StopByTag(this);
		}

		private bool WasKeyRepeated(Key key)
		{
			return Container.Input.WasKeyRepeated(key);
		}

		private void InsertChar(char ch)
		{
			if (caretPos.TextPos < 0 || caretPos.TextPos > Text.Text.Length) return;
			if (!EditorParams.IsAcceptableLength(Text.Text.Length + 1)) return;
			var newText = Text.Text.Insert(caretPos.TextPos, ch.ToString());
			if (EditorParams.AcceptText != null && !EditorParams.AcceptText(newText)) return;
			if (EditorParams.MaxHeight > 0 && !EditorParams.IsAcceptableHeight(CalcTextHeight(newText))) return;
			Text.Text = newText;
			caretPos.TextPos++;
		}

		private float CalcTextHeight(string s)
		{
			var text = Text.Text;
			Text.Text = s;
			var height = Text.MeasureText().Height;
			Text.Text = text;
			return height;
		}

		static readonly List<Key> consumingKeys = Key.Enumerate().Where(
			k => k.IsPrintable() || k.IsTextNavigation() || k.IsTextEditing()).ToList();

		private void HandleKeys(string originalText)
		{
			try {
				if (WasKeyRepeated(Key.Left))
					caretPos.TextPos--;
				if (WasKeyRepeated(Key.Right))
					caretPos.TextPos++;
				if (WasKeyRepeated(Key.Up))
					caretPos.Line--;
				if (WasKeyRepeated(Key.Down))
					caretPos.Line++;
				if (WasKeyRepeated(Key.Home))
					caretPos.Pos = 0;
				if (WasKeyRepeated(Key.End))
					caretPos.Pos = int.MaxValue;
				if (WasKeyRepeated(Key.Commands.Delete) || WasKeyRepeated(Key.Delete)) {
					if (caretPos.TextPos >= 0 && caretPos.TextPos < Text.Text.Length) {
						Text.Text = Text.Text.Remove(caretPos.TextPos, 1);
						caretPos.TextPos--;
						caretPos.TextPos++; // Enforce revalidation.
					}
				}
				if (WasKeyRepeated(Key.Enter)) {
					if (EditorParams.IsAcceptableLines(Text.Text.Count(ch => ch == '\n') + 2)) {
						InsertChar('\n');
					} else {
						Container.Input.ConsumeKey(Key.Enter);
						Container.RevokeFocus();
					}
				}
				if (WasKeyRepeated(Key.Escape)) {
					Text.Text = originalText;
					Container.Input.ConsumeKey(Key.Escape);
					Container.RevokeFocus();
				}
				if (WasKeyRepeated(Key.Commands.Paste)) {
					foreach (var ch in Clipboard.Text)
						InsertChar(ch);
				}
			} finally {
				Container.Input.ConsumeKeys(consumingKeys);
			}
		}

		private float lastCharShowTimeLeft;
		private bool isLastCharVisible;

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
						lastCharShowTimeLeft = 0f;
					}
				}
				else if (ch >= ' ') {
					InsertChar(ch);
					lastCharShowTimeLeft = EditorParams.PasswordLastCharShowTime;
				}
			}
		}

		private IEnumerator<object> TrackLastCharInput()
		{
			while (true) {
				if (Text.Text != "") {
					lastCharShowTimeLeft -= Task.Current.Delta;
					var shouldShowLastChar = lastCharShowTimeLeft > 0;
					if (shouldShowLastChar != isLastCharVisible) {
						isLastCharVisible = shouldShowLastChar;
						Text.Invalidate();
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> HandleInputTask()
		{
			bool wasFocused = false;
			string originalText = null;
			while (true) {
				var wasClicked = Container.WasClicked();
				if (wasClicked)
					Container.SetFocus();
				if (Container.IsFocused()) {
					HandleKeys(originalText);
					HandleTextInput();
					if (wasClicked) {
						var t = Container.LocalToWorldTransform.CalcInversed();
						caretPos.WorldPos = t.TransformVector(Container.Input.MousePosition);
					}
					Text.SyncCaretPosition();
				}
				var isFocused = Container.IsFocused();
				caretPos.IsVisible = isFocused;
				if (!wasFocused && isFocused) {
					originalText = Text.Text;
				}
				if (wasFocused && !isFocused) {
					if (originalText != Text.Text) {
						Text.Submit();
					}
				}
				wasFocused = isFocused;
				yield return null;
			}
		}
	}
}

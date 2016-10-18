using System;

namespace Lime
{
	/// <summary>
	/// Types of horizontal alignment.
	/// </summary>
	public enum HAlignment
	{
		Left,
		Center,
		Right,
	}

	/// <summary>
	/// Types of vertical alignment.
	/// </summary>
	public enum VAlignment
	{
		Top,
		Center,
		Bottom,
	}

	/// <summary>
	/// Types of text overflow handling.
	/// </summary>
	public enum TextOverflowMode
	{
		/// <summary>
		/// Carry overflowed text on the next line.
		/// </summary>
		Default,

		/// <summary>
		/// Reduce font size until text is not overflowing.
		/// </summary>
		Minify,

		/// <summary>
		/// Add ellipsis ("...") in place of overflowed part of text.
		/// </summary>
		Ellipsis,

		/// <summary>
		/// Ignore text overflowing.
		/// </summary>
		Ignore
	}

	public interface ICaretPosition
	{
		int Line { get; set; }
		int Col { get; set; }
		int TextPos { get; set; }
		Vector2 WorldPos { get; set; }
		bool IsVisible { get; set; }
		void InvalidatePreservingTextPos();
		void ClampTextPos(int textLength);
		void ClampLine(int lineCount);
		void ClampCol(int maxCol);
		void NextLine();
	}

	public delegate void TextProcessorDelegate(ref string text);

	public interface IText
	{
		event Action<string> Submitted;
		/// <summary>
		/// Returns the text's bounding box.
		/// </summary>
		Rectangle MeasureText();
		void Invalidate();
		void SyncCaretPosition();
		void Submit();
		bool CanDisplay(char ch);

		bool Localizable { get; set; }
		string Text { get; set; }
		string DisplayText { get; }
		event TextProcessorDelegate TextProcessor;
		HAlignment HAlignment { get; set; }
		VAlignment VAlignment { get; set; }
		TextOverflowMode OverflowMode { get; set; }
		bool WordSplitAllowed { get; set; }
		bool TrimWhitespaces { get; set; }
	}
}

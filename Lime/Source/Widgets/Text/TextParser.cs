using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.Text
{
	class TextParser
	{
		private const char Nbsp = ' ';
		public struct Fragment
		{
			public int Style;
			public string Text;
			public bool IsNbsp;
		}

		readonly string text;
		readonly Stack<string> tagStack = new Stack<string>();

		int pos = 0;
		int currentStyle = -1;
		public string ErrorMessage;
		public List<string> Styles = new List<string>();
		public List<Fragment> Fragments = new List<Fragment>();

		public TextParser(string text = null)
		{
			text = text ?? "";
			this.text = text;
			while (pos < text.Length) {
				if (IsNbsp()) {
					ParseNbsp();
				}
				if (text[pos] == '<') {
					ParseTag();
				} else {
					ParseText();
				}
			}
			if (tagStack.Count > 0) {
				ErrorMessage = String.Format("Unmatched tag '&lt;{0}&gt;'", tagStack.Peek());
			}
		}

		void ParseText()
		{
			int p = pos;
			while (pos < text.Length) {
				if (text[pos] == '<' || IsNbsp()) {
					break;
				} else if (text[pos] == '>') {
					ErrorMessage = "Unexpected '&gt;'";
					pos = text.Length;
					return;
				} else {
					pos++;
				}
			}
			if (p != pos) {
				ProcessTextBlock(text.Substring(p, pos - p));
			}
		}

		void ParseTag()
		{
			bool isOpeningTag = true;
			bool isClosedTag = false;
			int p = ++pos;
			while (pos < text.Length) {
				if (text[pos] == '/') {
					if (p == pos) {
						isOpeningTag = false;
						pos++;
						p++;
					} else if (pos + 1 < text.Length && text[pos + 1] == '>' && isOpeningTag) {
						pos++;
						isClosedTag = true;
					} else {
						ErrorMessage = "Unexpected '/'";
						pos = text.Length;
						return;
					}
				} else if (text[pos] == '>') {
					if (isClosedTag) {
						string tag = text.Substring(p, pos - p - 1);
						ProcessOpeningTag(tag);
						ProcessTextBlock ("");
						ProcessClosingTag(tag);
					} else if (isOpeningTag) {
						string tag = text.Substring(p, pos - p);
						if (!ProcessOpeningTag(tag)) {
							pos = text.Length;
							return;
						}
					} else {
						string tag = text.Substring(p, pos - p);
						if (!ProcessClosingTag(tag)) {
							pos = text.Length;
							return;
						}
					}
					pos++;
					return;
				} else {
					pos++;
				}
			}
			ErrorMessage = "Unclosed tag";
			pos = text.Length;
		}

		/// <summary>
		/// Creates "non-breaking space" fragment and skips 1 or 6 chars whether nbsp is unicode or html style.
		/// </summary>
		private void ParseNbsp()
		{
			Fragments.Add(new Fragment { Style = -1, Text = " ", IsNbsp = true });
			if (text[pos] == '&') {
				pos += 6;
			} else {
				pos++;
			}
		}

		private bool IsNbsp()
		{
			return
				text[pos] == Nbsp ||
				(text[pos] == '&' && text.Length - pos >= 6 && text.Substring(pos, 6) == "&nbsp;");
		}

		bool ProcessOpeningTag(string tag)
		{
			tagStack.Push(tag);
			SetStyle(tag);
			return true;
		}

		bool ProcessClosingTag(string tag)
		{
			if (tagStack.Count == 0 || tagStack.Peek() != tag) {
				ErrorMessage = String.Format("Unexpected closing tag '&lt;/{0}&gt;'", tag);
				return false;
			}
			tagStack.Pop();
			SetStyle(tagStack.Count == 0 ? null : tagStack.Peek());
			return true;
		}

		void ProcessTextBlock(string text)
		{
			Fragments.Add(new Fragment { Style = currentStyle, Text = UnescapeTaggedString(text) });
		}

		string UnescapeTaggedString(string text)
		{
			text = text.Replace("&lt;", "<");
			text = text.Replace("&gt;", ">");
			text = text.Replace("&amp;", "&");
			return text;
		}

		void SetStyle(string styleName)
		{
			if (styleName != null) {
				for (int i = 0; i < Styles.Count; i++) {
					if (Styles[i] == styleName) {
						currentStyle = i;
						return;
					}
				}
			} else {
				currentStyle = -1;
				return;
			}
			currentStyle = Styles.Count;
			Styles.Add(styleName);
		}
	}

}

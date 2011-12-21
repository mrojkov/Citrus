using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class TextStyle
	{
		[ProtoContract]
		public enum ImageUsageEnum
		{
			[ProtoEnum]
			Bullet,
			[ProtoEnum]
			Overlay
		}

		[ProtoMember(1)]
		public SerializableTexture ImageTexture { get; set; }
		
		[ProtoMember(2)]
		public Vector2 ImageSize { get; set; }
		
		[ProtoMember(3)]
		public ImageUsageEnum ImageUsage { get; set; }
		
		[ProtoMember(4)]
		public SerializableFont Font { get; set; }
		
		[ProtoMember(5)]
		public float Size { get; set; }
		
		[ProtoMember(6)]
		public float SpaceAfter { get; set; }
		
		[ProtoMember(7)]
		public bool Bold { get; set; }
		
		[ProtoMember(8)]
		public bool CastShadow { get; set; }
		
		[ProtoMember(9)]
		public Vector2 ShadowOffset { get; set; }
	}

	[ProtoContract]
	public class Text
	{
		public string Content { get; set; }
		public HAlignment HAlignment { get; set; }
		public VAlignment VAlignment { get; set; }
	}

	class TextParser
	{
		struct Piece
		{
			public int Style;
			public string Text;
		}

		string text;
		int pos;
		int currentStyle;
		string errorMessage;
		List<string> styles;
		Stack<string> tagStack;
		List<Piece> pieces;

		public bool Parse (string text)
		{
			tagStack.Clear ();
			styles.Clear();
			pieces.Clear();
			currentStyle = -1;
			errorMessage = null;
			pos = 0;
			this.text = text;
			while (pos < text.Length) {
				if (text [pos] == '<') {
					ParseTag ();
				} else {
					ParseText ();
				}
			}
			if (tagStack.Count > 0) {
				errorMessage = String.Format ("Unclosed tag '&lt;{0}&gt;'", tagStack.Peek ());
				return false;
			}
			return true;
		}

		void ParseText ()
		{
			int p = pos;
			while (pos < text.Length) {
				if (text [pos] == '<') {
					break;
				} else if (text [pos] == '>') {
					errorMessage = "Unexpected '&gt;'";
					pos = text.Length;
					return;
				} else {
					pos++;
				}
			}
			if (p != pos) {
				ProcessTextBlock (text.Substring (p, pos - p));
			}
		}

		void ParseTag ()
		{
			bool isOpeningTag = true;
			bool isClosedTag = false;
			int p = ++pos;
			while (pos < text.Length) {
				if (text [pos] == '/') {
					if (p == pos) {
						isOpeningTag = false;
						pos++;
						p++;
					} else if (pos + 1 < text.Length && text [pos + 1] == '>' && isOpeningTag) {
						pos++;
						isClosedTag = true;
					} else {
						errorMessage = "Unexpected '/'";
						pos = text.Length;
						return;
					}
				} else if (text [pos] == '>') {
					if (isClosedTag) {
						string tag = text.Substring (p, pos - p - 1);
						ProcessOpeningTag (tag);
						ProcessTextBlock ("");
						ProcessClosingTag (tag);
					} else if (isOpeningTag) {
						string tag = text.Substring (p, pos - p);
						if (!ProcessOpeningTag (tag)) {
							pos = text.Length;
							return;
						}
					} else {
						string tag = text.Substring (p, pos - p);
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
			errorMessage = "Unclosed tag";
			pos = text.Length;
		}

		bool ProcessOpeningTag (string tag)
		{
			tagStack.Push (tag);
			SetStyle (tag);
			return true;
		}

		bool ProcessClosingTag (string tag)
		{
			if (tagStack.Count == 0 || tagStack.Peek () != tag) {
				errorMessage = String.Format ("Unexpected closing tag '&lt;/{0}&gt;'", tag);
				return false;
			}
			tagStack.Pop ();
			SetStyle (tagStack.Count == 0 ? null : tagStack.Peek());
			return true;
		}

		void ProcessTextBlock (string text)
		{
			Piece p = new Piece { Style = currentStyle, Text = UnescapeTaggedString (text) };
			pieces.Add (p);
		}

		string UnescapeTaggedString (string text)
		{
			text = text.Replace ("&lt;", "<" );
			text = text.Replace ("&gt;", ">" );
			return text;
		}

		void SetStyle (string styleName)
		{
			if (styleName != null) {
				for (int i = 0; i < styles.Count; i++) {
					if (styles [i] == styleName) {
						currentStyle = i;
						return;
					}
				}
			} else {
				currentStyle = -1;
				return;
			}
			currentStyle = styles.Count;
			styles.Add (styleName);
		}
	}
}

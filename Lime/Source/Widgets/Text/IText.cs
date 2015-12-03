using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Types of horizontal alignment.
	/// </summary>
	[ProtoContract]
	public enum HAlignment
	{
		[ProtoEnum]
		Left,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Right,
		[ProtoEnum]
		Expand
	}

	/// <summary>
	/// Types of vertical alignment.
	/// </summary>
	[ProtoContract]
	public enum VAlignment
	{
		[ProtoEnum]
		Top,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Bottom,
		[ProtoEnum]
		Expand
	}

	[ProtoContract]
	public enum TextOverflowMode
	{
		[ProtoEnum]
		Ignore,
		[ProtoEnum]
		Minify,
		[ProtoEnum]
		Ellipsis
	}

	public interface ICaretPosition
	{
		int Line { get; set; }
		int Pos { get; set; }
		int TextPos { get; set; }
		Vector2 WorldPos { get; set; }
		bool IsVisible { get; set; }
	}

	public interface IText
	{
		Rectangle MeasureText();
		string Text { get; set; }
		string DisplayText { get; set; }
		HAlignment HAlignment { get; set; }
		VAlignment VAlignment { get; set; }
		TextOverflowMode OverflowMode { get; set; }
		bool WordSplitAllowed { get; set; }
		bool TrimWhitespaces { get; set; }
	}

}

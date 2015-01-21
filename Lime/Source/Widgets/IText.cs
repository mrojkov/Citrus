using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum HAlignment
	{
		[ProtoEnum]
		Left,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Right,
	}

	[ProtoContract]
	public enum VAlignment
	{
		[ProtoEnum]
		Top,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Bottom,
	}

	[ProtoContract]
	public enum TextOverflowMode
	{
		[ProtoEnum]
		Default,
		[ProtoEnum]
		Minify,
		[ProtoEnum]
		Ellipsis,
		[ProtoEnum]
		Ignore,
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
	}

}

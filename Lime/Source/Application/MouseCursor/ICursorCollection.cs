namespace Lime
{
	internal interface ICursorCollection
	{
		MouseCursor Default { get; }
		MouseCursor Empty { get; }
		MouseCursor Hand { get; }
		MouseCursor IBeam { get; }
		MouseCursor Wait { get; }
		MouseCursor Move { get; }
		MouseCursor SizeNS { get; }
		MouseCursor SizeWE { get; }
		MouseCursor SizeNESW { get; }
		MouseCursor SizeNWSE { get; }
	}
}

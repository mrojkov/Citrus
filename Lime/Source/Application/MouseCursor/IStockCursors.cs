namespace Lime
{
	internal interface IStockCursors
	{
		MouseCursor Default { get; }
		MouseCursor Empty { get; }
		MouseCursor Hand { get; }
		MouseCursor IBeam { get; }
		MouseCursor SizeNS { get; }
		MouseCursor SizeWE { get; }
		MouseCursor SizeAll { get; }
		MouseCursor SizeNWSE { get; }
		MouseCursor SizeNESW { get; }
	}
}

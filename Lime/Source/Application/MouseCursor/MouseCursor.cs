using System;

namespace Lime
{
	public partial class MouseCursor
	{
		private static readonly ICursorCollection cursorCollection = new CursorCollection();

		public MouseCursor() { }

		[Obsolete("Use something else for cursor constructing, this one doesn't do anything anyways.", true)]
		public MouseCursor(string name, IntVector2 hotSpot, string assemblyName = null) { }

		public static MouseCursor Default { get { return cursorCollection.Default; } }
		public static MouseCursor Empty { get { return cursorCollection.Empty; } }
		public static MouseCursor Hand { get { return cursorCollection.Hand; } }
		public static MouseCursor IBeam { get { return cursorCollection.IBeam; } }
		public static MouseCursor Wait { get { return cursorCollection.Wait; } }
		public static MouseCursor Move { get { return cursorCollection.Move; } }

		/// <summary>
		/// Gets the two-headed vertical (north/south) sizing cursor.
		/// </summary>
		public static MouseCursor SizeNS { get { return cursorCollection.SizeNS; } }

		/// <summary>
		/// Gets the two-headed horizontal(west/east) sizing cursor.
		/// </summary>
		public static MouseCursor SizeWE { get { return cursorCollection.SizeWE; } }

		/// <summary>
		/// Gets the two-headed diagonal (northeast/southwest) sizing cursor.
		/// </summary>
		public static MouseCursor SizeNESW { get { return cursorCollection.SizeNESW; } }

		/// <summary>
		/// Gets the two-headed diagonal (northwest/southeast) sizing cursor.
		/// </summary>
		public static MouseCursor SizeNWSE { get { return cursorCollection.SizeNWSE; } }
	}
}
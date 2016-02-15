using System;
#if WIN
using NativeCursor = System.Windows.Forms.Cursor;
#elif MAC || MONOMAC
using NativeCursor = System.Object;
#elif ANDROID || iOS || UNITY
using NativeCursor = System.Object;
#endif

namespace Lime
{
	public class MouseCursor
	{
		private static readonly ICursorCollection cursorCollection = new CursorCollection();
		private readonly MouseCursorImplementation implementation;

		public MouseCursor(MouseCursorImplementation implementation)
		{
			this.implementation = implementation;
		}
		public MouseCursor(Bitmap bitmap, IntVector2 hotSpot)
		{
			implementation = new MouseCursorImplementation(bitmap, hotSpot);
		}

		[Obsolete("Use other constructors as this one doesn't do anything anyways.", true)]
		public MouseCursor(string name, IntVector2 hotSpot, string assemblyName = null) { }

		public NativeCursor NativeCursor
		{
			get { return implementation.NativeCursor; }
		}

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
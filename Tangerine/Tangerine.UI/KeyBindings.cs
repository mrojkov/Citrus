using System;
using Lime;

namespace Tangerine.UI
{
	public static class KeyBindings
	{
		public static readonly Key CloseDialog = Key.Escape;

		public static class Timeline
		{
			public static readonly Key ScrollLeft = Key.Left;
			public static readonly Key ScrollRight = Key.Right;
			public static readonly Key FastScrollLeft = Key.MapShortcut(Modifiers.Alt, Key.Left);
			public static readonly Key FastScrollRight = Key.MapShortcut(Modifiers.Alt, Key.Right);
		}
	}
}


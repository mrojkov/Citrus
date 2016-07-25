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
			public static readonly Key ScrollUp = Key.Up;
			public static readonly Key ScrollDown = Key.Down;
			public static readonly Key SelectUp = Key.MapShortcut(Modifiers.Shift, Key.Up);
			public static readonly Key SelectDown = Key.MapShortcut(Modifiers.Shift, Key.Down);
			public static readonly Key EnterNode = Key.Enter;
			public static readonly Key ExitNode = Key.BackSpace;
		}

		public static class Generic
		{
			public static readonly Shortcut OpenFile = new Shortcut(Modifiers.Command, Key.O);
			public static readonly Shortcut OpenProject = new Shortcut(Modifiers.Command | Modifiers.Shift, Key.O);
			public static readonly Shortcut PreferencesDialog = new Shortcut(Modifiers.Command, Key.P);
		}
	}
}


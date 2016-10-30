using System;
using Lime;

namespace Tangerine.UI
{
	public static class KeyBindings
	{
		public static readonly Key CloseDialog = Key.Escape;

		public static class TimelineKeys
		{
			public static readonly Key ScrollLeft = Key.MapShortcut(Key.Left);
			public static readonly Key ScrollRight = Key.MapShortcut(Key.Right);
			public static readonly Key FastScrollLeft = Key.MapShortcut(Modifiers.Alt, Key.Left);
			public static readonly Key FastScrollRight = Key.MapShortcut(Modifiers.Alt, Key.Right);
			public static readonly Key ScrollUp = Key.MapShortcut(Key.Up);
			public static readonly Key ScrollDown = Key.MapShortcut(Key.Down);
			public static readonly Key SelectUp = Key.MapShortcut(Modifiers.Shift, Key.Up);
			public static readonly Key SelectDown = Key.MapShortcut(Modifiers.Shift, Key.Down);
			public static readonly Key EnterNode = Key.MapShortcut(Key.Enter);
			public static readonly Key ExitNode = Key.MapShortcut(Key.BackSpace);
		}

		public static class GenericKeys
		{
			public static readonly Shortcut Open = new Shortcut(Modifiers.Command, Key.O);
			public static readonly Shortcut Save = new Shortcut(Modifiers.Command, Key.S);
			public static readonly Shortcut SaveAs = new Shortcut(Modifiers.Command | Modifiers.Shift, Key.S);
			public static readonly Shortcut OpenProject = new Shortcut(Modifiers.Command | Modifiers.Shift, Key.O);
			public static readonly Shortcut PreferencesDialog = new Shortcut(Modifiers.Command, Key.P);
			public static readonly Shortcut CloseDocument = new Shortcut(Modifiers.Command, Key.W);

			public static readonly Shortcut NextDocument = new Shortcut(Modifiers.Control, Key.Tab);
			public static readonly Shortcut PreviousDocument = new Shortcut(Modifiers.Control | Modifiers.Shift, Key.Tab);
		}

		public static class SceneViewKeys
		{
			public static readonly Key SceneExposition = Key.MapShortcut(Key.Tab);
			public static readonly Key SceneExpositionMultiSelect = Key.MapShortcut(Modifiers.Shift, Key.Tab);
			public static readonly Key PreviewAnimation = Key.MapShortcut(Key.F5);
			public static readonly Key DragRight = Key.MapShortcut(Key.D);
			public static readonly Key DragLeft = Key.MapShortcut(Key.A);
			public static readonly Key DragUp = Key.MapShortcut(Key.W);
			public static readonly Key DragDown = Key.MapShortcut(Key.S);
			public static readonly Key DragRightFast = Key.MapShortcut(Modifiers.Shift, Key.D);
			public static readonly Key DragLeftFast = Key.MapShortcut(Modifiers.Shift, Key.A);
			public static readonly Key DragUpFast = Key.MapShortcut(Modifiers.Shift, Key.W);
			public static readonly Key DragDownFast = Key.MapShortcut(Modifiers.Shift, Key.S);
		}
	}
}


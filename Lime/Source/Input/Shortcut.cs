#if !UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	[Flags]
	public enum Modifiers
	{
		None = 0,
		Alt = 1,
		Control = 2,
		Shift = 4,
		Win = 8,
#if MAC
		/// <summary>
		/// Command modifier corresponds Control on Windows and Command key on Mac.
		/// </summary>
		Command = Win,
#else
		/// <summary>
		/// Command modifier corresponds Control on Windows and Command key on Mac.
		/// </summary>
		Command = Control,
#endif
	}

	/// <summary>
	/// Represents combination of a key with keyboard modifiers used to trigger some action.
	/// </summary>
	public struct Shortcut
	{
		public readonly Modifiers Modifiers;
		public readonly Key Main;

		public Shortcut(Key main)
		{
			Modifiers = Modifiers.None;
			Main = main;
		}

		public Shortcut(Modifiers modifiers, Key main)
		{
			Modifiers = modifiers;
			Main = main;
		}

		public static bool ValidateMainKey(Key key) =>
			key.IsPrintable() || key.IsTextNavigation() || key.IsTextEditing() || key.IsFunctional() ||
			key == Key.Escape || key == Key.Tab || key == Key.Menu;

		public static implicit operator Shortcut(Key main) { return new Shortcut(main); }

		public override int GetHashCode()
		{
			return Main.Code.GetHashCode() ^ Modifiers.GetHashCode();
		}

		public static bool operator == (Shortcut lhs, Shortcut rhs)
		{
			return lhs.Main == rhs.Main && lhs.Modifiers == rhs.Modifiers;
		}

		public static bool operator != (Shortcut lhs, Shortcut rhs)
		{
			return lhs.Main != rhs.Main || lhs.Modifiers != rhs.Modifiers;
		}

		public override bool Equals(object obj)
		{
			return this == (Shortcut)obj;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (Modifiers.HasFlag(Modifiers.Alt)) sb.Append("Alt+");
			if (Modifiers.HasFlag(Modifiers.Shift)) sb.Append("Shift+");
			if (Modifiers.HasFlag(Modifiers.Control)) sb.Append("Ctrl+");
#if MAC
			if (Modifiers.HasFlag(Modifiers.Command)) sb.Append("Cmd+");
#else
			if (Modifiers.HasFlag(Modifiers.Win)) sb.Append("Win+");
#endif
			sb.Append(Main.ToString());
			return sb.ToString();
		}
	}
}

#endif

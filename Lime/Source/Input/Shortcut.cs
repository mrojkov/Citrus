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

		public static bool ValidateMainKey(Key key)
		{
			return key.IsPrintable() || key.IsTextNavigation() || key.IsTextEditing() || key.IsFunctional() || key == Key.Escape || key == Key.Tab;
		}

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
			string r = "";
			if ((Modifiers & Modifiers.Alt) != 0) {
				r += "Alt+";
			}
			if ((Modifiers & Modifiers.Control) != 0) {
				r += "Ctrl+";
			}
			if ((Modifiers & Modifiers.Shift) != 0) {
				r += "Shift+";
			}
			return r + Main;
		}
	}
}

#endif
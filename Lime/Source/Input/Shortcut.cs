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
		Shift = 1,
		Control = 2,
		Alt = 4,
		Command = 8
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
	}	
}

#endif
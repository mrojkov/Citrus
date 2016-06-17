#if !UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
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
	}	
}

#endif
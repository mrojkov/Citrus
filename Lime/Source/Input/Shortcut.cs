using System;
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
		/// Command modifier corresponds to Control on Windows and Command key on Mac.
		/// </summary>
		Command = Win,
#else
		/// <summary>
		/// Command modifier corresponds to Control on Windows and Command key on Mac.
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

		public Shortcut(string text)
		{
			if (String.IsNullOrEmpty(text)) {
				Main = Key.Unknown;
				Modifiers = Modifiers.None;
				return;
			}

			string[] words = text.Split(new char[] { '+' });
			string key = words[words.Length - 1];
			Main = Key.GetByName(key);
			Modifiers = Modifiers.None;
			if (Main == Key.Unknown) {
				return;
			}
			for (int i = 0; i != words.Length - 1; ++i) {
				string word = words[i].ToLower();
				if (word == "alt")
					Modifiers = Modifiers | Modifiers.Alt;
				else if (word == "control" || word == "ctrl")
					Modifiers = Modifiers | Modifiers.Control;
				else if (word == "shift")
					Modifiers = Modifiers | Modifiers.Shift;
				else if (word == "win")
					Modifiers = Modifiers | Modifiers.Win;
				else if (word == "cmd" || word == "command")
					Modifiers = Modifiers | Modifiers.Command;
				else
					throw new ArgumentException("Wrong modifier", word);
			}
		}

		public static bool ValidateMainKey(Key key) =>
			key.IsPrintable() || key.IsTextNavigation() || key.IsTextEditing() || key.IsFunctional() ||
			key == Key.Escape || key == Key.Tab || key == Key.Menu || key == Key.Unknown;

		public static implicit operator Shortcut(Key main) => new Shortcut(main);

		public override int GetHashCode() => Main.Code.GetHashCode() ^ Modifiers.GetHashCode();

		public static bool operator == (Shortcut lhs, Shortcut rhs) =>
			lhs.Main == rhs.Main && lhs.Modifiers == rhs.Modifiers;

		public static bool operator != (Shortcut lhs, Shortcut rhs) =>
			lhs.Main != rhs.Main || lhs.Modifiers != rhs.Modifiers;

		public override bool Equals(object obj) => this == (Shortcut)obj;

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

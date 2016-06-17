#if !UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public interface IKeyTranslator
	{
		Key Translate(Modifiers modifiers, Key key);
	}

	public class ShortcutTranslator : IKeyTranslator
	{
		public readonly Shortcut Shortcut;
		public Key Command { get; private set; }

		public ShortcutTranslator(Shortcut shortcut, Key command)
		{
			Shortcut = shortcut;
			Command = command;
		}

		public Key Translate(Modifiers modifiers, Key key)
		{
			return (key == Shortcut.Main && Shortcut.Modifiers == modifiers) ? Command : key;
		}
	}
}

#endif
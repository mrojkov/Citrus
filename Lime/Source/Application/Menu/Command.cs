#if WIN || MAC
using System;

namespace Lime
{
	public interface ICommand
	{
		string Text { get; }
		Shortcut Shortcut { get; }
		Menu Submenu { get; }
		bool Enabled { get; }
		bool Visible { get; }
		void Execute();
		void Refresh();
	}

	public class Command : ICommand
	{
		public string Text { get; set; }
		public Shortcut Shortcut { get; set; }
		public Menu Submenu { get; set; }
		public bool Enabled { get; set; }
		public bool Visible { get; set; }

		public static readonly ICommand MenuSeparator = new Command();

		public Command()
		{
			Enabled = true;
			Visible = true;
		}

		public Command(string text) : this()
		{
			Text = text;
		}

		public Command(string text, Shortcut shortcut) : this(text)
		{
			Shortcut = shortcut;
		}

		public virtual void Execute() { }
		public virtual void Refresh() { }
	}

	public sealed class DelegateCommand : Command
	{
		public event Action Executing;

		public DelegateCommand(string text) : base(text) { }

		public DelegateCommand(string text, Action executing) : base(text)
		{
			Executing += executing;
		}

		public DelegateCommand(string text, Shortcut shortcut, Action executing) : base(text, shortcut)
		{
			Executing += executing;
		}

		public override void Execute()
		{
			if (Executing != null) {
				Executing();
			}
		}
	}

	public sealed class KeySendingCommand : Command
	{
		public readonly Key Key;

		public KeySendingCommand(string text, Key key) : base(text)
		{
			Key = key;
		}

		public KeySendingCommand(string text, Shortcut shortcut, Key key) : base(text, shortcut)
		{
			Key = key;
		}

		public override void Execute()
		{
			if (Key != Key.Unknown) {
				PropagateKeystrokeToWindows();
			}
		}

		void PropagateKeystrokeToWindows()
		{
			foreach (var i in Application.Windows) {
				i.Input.SetKeyState(Key, true);
				i.Input.SetKeyState(Key, false);
			}
		}

		public override void Refresh()
		{
			if (Shortcut.Main == Key.Unknown) {
				return;
			}
			var enabled = false;
			foreach (var i in Application.Windows) {
				enabled |= i.Input.IsKeyEnabled(Key);
			}
			Enabled = enabled;
		}
	}
}
#endif
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
		ITexture Icon { get; }
		void Execute();
	}

	public class Command : ICommand
	{
		public virtual string Text { get; private set; }
		public virtual Shortcut Shortcut { get; private set; }
		public virtual bool Enabled => true;
		public virtual bool Visible => true;
		public virtual ITexture Icon => null;
		public virtual Menu Submenu => null;

		public static readonly ICommand MenuSeparator = new Command();

		public Command() { }
		public Command(string text) { Text = text; }
		public Command(string text, Shortcut shortcut) : this(text)
		{
			Shortcut = shortcut;
		}

		public virtual void Execute() { }
	}

	public class Submenu : Menu, ICommand
	{
		private string text;

		string ICommand.Text => text;
		Shortcut ICommand.Shortcut => new Shortcut();
		bool ICommand.Enabled => true;
		bool ICommand.Visible => true;
		ITexture ICommand.Icon => null;
		Menu ICommand.Submenu => this;

		public Submenu(string text)
		{
			this.text = text;
		}

		void ICommand.Execute() { }
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

		public override bool Enabled
		{
			get
			{
				if (Shortcut.Main == Key.Unknown) {
					return false;
				}
				var enabled = false;
				foreach (var i in Application.Windows) {
					enabled |= i.Input.IsKeyEnabled(Key);
				}
				return enabled;
			}
		}
	}

	public sealed class LocalKeySendingCommand : Command
	{
		public readonly Input Input;
		public readonly Key Key;

		public LocalKeySendingCommand(Input input, string text, Key key) : base(text)
		{
			Input = input;
			Key = key;
			Enabled = Input.IsKeyEnabled(Key);
		}

		public override void Execute()
		{
			if (Key != Key.Unknown) {
				Input.SetKeyState(Key, true);
				Input.SetKeyState(Key, false);
			}
		}

		public override bool Enabled { get; }
	}
}
#endif
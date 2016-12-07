using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// The ICommand interface provides an abstract user interface command.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Gets or sets the command's descriptive text.
		/// If the command is added to a menu, the menu option will consist of the text and the shortcut (if there is one).
		/// </summary>
		string Text { get; set; }

		/// <summary>
		/// Gets or sets the command's primary shortcut key.
		/// </summary>
		Shortcut Shortcut { get; set; }

		/// <summary>
		/// Gets or sets the menu contained by this command.
		/// Commands that contain menus can be used to create menu items with submenus,
		/// or inserted into toolbars to create buttons with popup menus.
		/// </summary>
		IMenu Menu { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the command is enabled (e.g. in menus and toolbars).
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the command can be seen (e.g. in menus and toolbars).
		/// </summary>
		bool Visible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the command will auto repeat when the keyboard shortcut combination is held down.
		/// Commands used in the main menu are always repeatable. The default value is true.
		/// </summary>
		bool Repeatable { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the menu item or tool button is checked.
		/// </summary>
		bool Checked { get; set; }

		/// <summary>
		/// Gets or sets the icon. The icon is used as the tool button icon.
		/// </summary>
		ITexture Icon { get; set; }

		/// <summary>
		/// Gets the command's version, which is increased after changing any command's property.
		/// </summary>
		int Version { get; }

		/// <summary>
		/// Returns true, if the command was issued since the last active window update.
		/// </summary>
		bool WasIssued();

		/// <summary>
		/// Consumes the command. WasIssued is false if the command was consumed.
		/// </summary>
		void Consume();

		/// <summary>
		/// Returns true if the command was consumed.
		/// </summary>
		bool IsConsumed();

		/// <summary>
		/// Occurs when command is issued by clicking on menu item, activating menu shortcut or clicking a tool button.
		/// </summary>
		event Action Issued;
	}

	public class Command : ICommand
	{
		public static readonly ICommand Undo = new Command("Undo", new Shortcut(Modifiers.Command, Key.Z));
		public static readonly ICommand Redo = new Command("Redo", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.Z));
		public static readonly ICommand SelectAll = new Command("Select All", new Shortcut(Modifiers.Command, Key.A));
		public static readonly ICommand Cut = new Command("Cut", new Shortcut(Modifiers.Command, Key.X));
		public static readonly ICommand Copy = new Command("Copy", new Shortcut(Modifiers.Command, Key.C));
		public static readonly ICommand Paste = new Command("Paste", new Shortcut(Modifiers.Command, Key.V));
		public static readonly ICommand Delete = new Command("Delete", new Shortcut(Key.Delete));

		public static readonly List<ICommand> Editing = new List<ICommand> {
			Undo, Redo, SelectAll, Cut, Copy, Paste, Delete
		};

		private long issuedAtUpdate;
		private long consumedAtUpdate;
		private string text;
		private Shortcut shortcut;
		private Key mappedShortcut;
		private IMenu submenu;
		private bool enabled = true;
		private bool visible = true;
		private ITexture icon;

		public int Version { get; private set; } = 1;

		public string Text
		{
			get { return text; }
			set
			{
				if (text != value) {
					text = value;
					Version++;
				}
			}
		}

		public Shortcut Shortcut
		{
			get { return shortcut; }
			set
			{
				if (shortcut != value) {
					shortcut = value;
					mappedShortcut = Key.MapShortcut(value);
					Version++;
				}
			}
		}

		public IMenu Menu
		{
			get { return submenu; }
			set
			{
				if (submenu != value) {
					submenu = value;
					Version++;
				}
			}
		}

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value) {
					enabled = value;
					Version++;
				}
			}
		}

		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value) {
					visible = value;
					Version++;
				}
			}
		}

		public ITexture Icon
		{
			get { return icon; }
			set
			{
				if (icon != value) {
					icon = value;
					Version++;
				}
			}
		}

		public bool Repeatable { get; set; } = true;

		public bool Checked { get; set; }

		public event Action Issued;
		public static readonly ICommand MenuSeparator = new Command();

		public Command() { }

		public Command(string text) { Text = text; }

		public Command(Shortcut shortcut)
		{
			Shortcut = shortcut;
		}

		public Command(Modifiers modifiers, Key main)
		{
			Shortcut = new Shortcut(modifiers, main);
		}

		public Command(Key main)
		{
			Shortcut = new Shortcut(main);
		}

		public Command(string text, Shortcut shortcut) : this(text)
		{
			Shortcut = shortcut;
		}

		public Command(string text, Action execute) : this(text)
		{
			Issued += execute;
		}

		public Command(string text, Shortcut shortcut, Action execute) : this(text, shortcut)
		{
			Issued += execute;
		}

		public Command(string text, IMenu menu)
		{
			Text = text;
			Menu = menu;
		}

		public void Issue()
		{
			issuedAtUpdate = Application.UpdateCounter;
			Issued?.Invoke();
		}

		public bool WasIssued()
		{
			var input = Window.Current.Input;
			return Enabled && (
				issuedAtUpdate == Application.UpdateCounter ||
				mappedShortcut != Key.Unknown &&
				(Repeatable && input.WasKeyRepeated(mappedShortcut) ||
		 		!Repeatable && input.WasKeyPressed(mappedShortcut))
			);
		}

		public void Consume()
		{
			consumedAtUpdate = Application.UpdateCounter;
			issuedAtUpdate = 0;
			if (mappedShortcut != Key.Unknown) {
				Window.Current.Input.ConsumeKey(mappedShortcut);
			}
		}

		public bool IsConsumed() => consumedAtUpdate == Application.UpdateCounter;

		public static void ConsumeRange(List<ICommand> commands)
		{
			foreach (var cmd in commands) {
				cmd.Consume();
			}
		}

		public static void ConsumeRange(IEnumerable<ICommand> commands)
		{
			foreach (var cmd in commands) {
				cmd.Consume();
			}
		}
	}
}
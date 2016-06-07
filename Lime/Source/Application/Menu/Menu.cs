using System;
using System.Collections.Generic;

namespace Lime
{
#if WIN || MAC
	public interface ICommand
	{
		string Text { get; set; }
		Shortcut Shortcut { get; set; }
		Menu Submenu { get; set; }
		bool Enabled { get; set; }
		bool Visible { get; set; }
		void Execute();
	}

	public class Command : ICommand
	{
		public string Text { get; set; }
		public Shortcut Shortcut { get; set; }
		public Menu Submenu { get; set; }
		public bool Enabled { get; set; }
		public bool Visible { get; set; }
		public event Action Executing;

		public Command()
		{
			Enabled = true;
			Visible = true;
		}

		public Command(string text, Action executing) : this()
		{
			Text = text;
			Executing += executing;
		}

		public void Execute()
		{
			if (Executing != null) {
				Executing();
			}
		}
	}

	public interface IMenu : IList<ICommand>
	{
		/// <summary>
		/// Shows the menu at the current position of the cursor.
		/// </summary>
		void Popup();

		/// <summary>
		/// Shows the menu at the given position relative to the window left top corner.
		/// </summary>
		/// <param name="position">The location in the window coordinate system to display the menu item.</param>
		/// <param name="command">The menu item to be positioned at the specified location in the window.</param>
		void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command);

		/// <summary>
		/// Refreshes the menu according to its internal state.
		/// </summary>
		void Refresh();
	}
#endif
}

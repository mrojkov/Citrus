using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IMenuItem
	{
		event Action Clicked;
		bool Visible { get; set; }
		bool Enabled { get; set; }
		string Text { get; set; }
		IMenu SubMenu { get; set; }
	}

	public interface IMenu : IList<IMenuItem>
	{
		/// <summary>
		/// Shows the menu at the current position of the cursor.
		/// </summary>
		void Popup();

		/// <summary>
		/// Shows the menu at the given position relative to the window left top corner.
		/// </summary>
		/// <param name="position">The location in the window coordinate system to display the menu item.</param>
		/// <param name="item">The menu item to be positioned at the specified location in the window.</param>
		void Popup(IWindow window, Vector2 position, float minimumWidth, IMenuItem item);
	}
}


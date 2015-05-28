using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	/// <summary>
	/// Элемент отладочного меню
	/// </summary>
	public class MenuItem
	{
		/// <summary>
		/// Высота элементов отладочного меню в пикселях
		/// </summary>
		public static float Height = 40;

		public Frame Frame = new Frame() { Tag = "$MenuItem.cs" };
		
		/// <summary>
		/// Меню, которому принадлежит этот элемент
		/// </summary>
		public Menu Menu;

		public bool Visible = true;
	}
}

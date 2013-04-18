using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class Menu 
	{
		//public QMenu QMenu { get; private set; }
		public string Name { get; set; }// { return QMenu.Title; } set { QMenu.Title = value; } }
		// Храним ссылки на объекты QT, чтобы их не сожрал GC
		private List<object> items = new List<object>();

		public Menu(string title)
		{
			//QMenu = new QMenu(title, The.DefaultQtParent);
		}

		public void AddSeparator()
		{
			//QMenu.AddSeparator();
		}

		public void Add(Action action)
		{
			//QMenu.AddAction(action.QAction);
			items.Add(action);
		}

		public void Add(Menu menu)
		{
			//QMenu.AddMenu(menu.QMenu);
			items.Add(menu);
		}

		public void Clear()
		{
			//QMenu.Clear();
			items.Clear();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class ViewMenu : Menu
	{
		public static ViewMenu Instance = new ViewMenu();

		ViewMenu()
			: base("&View")
		{
			Add(ShowTimeline.Instance);
			AddSeparator();
			Add(ChooseNextDocumentAction.Instance);
			Add(ChoosePrevDocumentAction.Instance);
		}
	}
}
using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using System.IO;
using Tangerine.UI;

namespace Tangerine
{
	class HelpDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly ColorThemeEnum theme;
		readonly Frame Frame;

		public HelpDialog(HelpPage page)
		{
			theme = AppUserPreferences.Instance.Theme;
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(800, 600),
				FixedSize = false,
				Title = "Help" + (String.IsNullOrEmpty(page.PageName) ? "" : " - " + page.PageName),
				MinimumDecoratedSize = new Vector2(400, 300)
			});
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};

			var browser = new WebBrowser();
			browser.Url = new Uri(page.Url);
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					browser
				}
			};
		}
	}
}

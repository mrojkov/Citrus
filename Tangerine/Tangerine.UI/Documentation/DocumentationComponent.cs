using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI
{
	public class DocumentationComponent : NodeComponent
	{
		public readonly string PageName;
		public string Filepath { get; set; }

		public static Action<HelpPage> Clicked { get; set; }

		private HelpGesture helpGesture;

		public DocumentationComponent(string pageName)
		{
			PageName = pageName;
			helpGesture = new HelpGesture(() => {
				if (HelpModeGestureManager.IsHelpModeOn) {
					OpenDocumentation();
				}
			});
		}

		private void OpenDocumentation()
		{
			HelpPage page;
			try {
				page = new HelpPage(PageName);
			}
			catch (System.Exception) {
				page = new HelpPage(HelpPage.ErrorPageName);
			}
			Clicked?.Invoke(page);
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			if (oldOwner != null) {
				oldOwner.Gestures.Remove(helpGesture);
			}
			Owner.Gestures.Insert(0, helpGesture);
		}
	}
}

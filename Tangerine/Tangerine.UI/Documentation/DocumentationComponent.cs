using Lime;
using System;

namespace Tangerine.UI
{
	public class DocumentationComponent : NodeComponent
	{
		public readonly string PageName;
		public string Filepath { get; set; }

		public static Action<string> Clicked { get; set; }

		private HelpGesture helpGesture;

		public DocumentationComponent(string pageName)
		{
			PageName = pageName;
			helpGesture = new HelpGesture(() => {
				if (Documentation.IsHelpModeOn) {
					Clicked?.Invoke(PageName);
					Documentation.IsHelpModeOn = !Documentation.IsHelpModeOn;
				}
			});
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

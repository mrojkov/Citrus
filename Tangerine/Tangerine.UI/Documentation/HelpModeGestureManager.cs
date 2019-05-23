using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.UI
{
	public class HelpModeGestureManager : GestureManager
	{
		private static readonly Task helpModeTask = new Task(HelpModeTask());

		public HelpModeGestureManager(WidgetContext context) : base(context)
		{
			context.Root.LateTasks.Add(helpModeTask);
		}

		~HelpModeGestureManager()
		{
			context.Root.LateTasks.Remove(helpModeTask);
		}

		protected override IEnumerable<Gesture> EnumerateGestures(Node node)
		{
			var noMoreClicks = new bool[3];
			var noMoreDoubleClicks = new bool[3];
			for (; node != null; node = node.Parent) {
				if (node.HasGestures()) {
					foreach (var g in node.Gestures) {
						if (Documentation.IsHelpModeOn != (g is HelpGesture)) {
							continue;
						}
						if (g is ClickGesture cg) {
							if (noMoreClicks[cg.ButtonIndex]) {
								continue;
							}
							noMoreClicks[cg.ButtonIndex] = true;
						}
						if (g is DoubleClickGesture dcg) {
							if (noMoreDoubleClicks[dcg.ButtonIndex]) {
								continue;
							}
							noMoreDoubleClicks[dcg.ButtonIndex] = true;
						}
						yield return g;
					}
				}
			}
		}

		private static IEnumerator<object> HelpModeTask()
		{
			while (true) {
				var manager = WidgetContext.Current.GestureManager as HelpModeGestureManager;
				if (manager != null && Documentation.IsHelpModeOn) {
					var node = WidgetContext.Current.NodeUnderMouse;
					WidgetContext.Current.MouseCursor = Cursors.DisabledHelp;
					while (node != null) {
						if (node.Components.Get<DocumentationComponent>() != null) {
							WidgetContext.Current.MouseCursor = Cursors.EnabledHelp;
							break;
						}
						node = node.Parent;
					}
				}
				yield return null;
			}
		}
	}

	public class HelpGesture : ClickGesture
	{
		public HelpGesture(Action recognized = null) : base(recognized)
		{
		}

		public HelpGesture(int buttonIndex, Action recognized = null) : base(buttonIndex, recognized)
		{
		}
	}
}

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
			bool helpModeOn = Documentation.IsHelpModeOn;
			var noClickGesturesAnymore = false;
			while (node != null) {
				if (node.HasGestures()) {
					var anyClickGesture = false;
					foreach (var r in node.Gestures) {
						if ((!helpModeOn && (r is HelpGesture)) || (helpModeOn && !(r is HelpGesture))) {
							continue;
						}
						bool isWrongGesture = r is ClickGesture || r is DoubleClickGesture;
						if (isWrongGesture && noClickGesturesAnymore) {
							continue;
						}
						anyClickGesture |= isWrongGesture;
						yield return r;
					}
					noClickGesturesAnymore |= anyClickGesture;
				}
				node = node.Parent;
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

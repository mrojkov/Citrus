using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.UI
{
	public class HelpModeGestureManager : GestureManager
	{
		public bool IsHelpModeOn { get; set; } = false;

		private readonly Task helpModeTask;

		public HelpModeGestureManager(WidgetContext context) : base(context)
		{
			helpModeTask = new Task(HelpModeTask());
			context.Root.LateTasks.Add(helpModeTask);
		}

		~HelpModeGestureManager()
		{
			context.Root.LateTasks.Remove(helpModeTask);
		}

		protected override IEnumerable<Gesture> EnumerateGestures(Node node)
		{
			var noClickGesturesAnymore = false;
			while (node != null) {
				if (node.HasGestures()) {
					var anyClickGesture = false;
					foreach (var r in node.Gestures) {
						if ((!IsHelpModeOn && (r is HelpGesture)) || (IsHelpModeOn && !(r is HelpGesture))) {
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

		private IEnumerator<object> HelpModeTask()
		{
			while (true) {
				var manager = WidgetContext.Current.GestureManager as HelpModeGestureManager;
				if (manager != null && manager.IsHelpModeOn) {
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

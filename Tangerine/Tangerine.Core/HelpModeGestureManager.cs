using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	public class HelpModeGestureManager : GestureManager
	{
		public bool IsHelpModeOn { get; set; } = false;

		public HelpModeGestureManager(WidgetContext context) : base(context)
		{

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Tangerine.UI
{
	public class TextArea : Widget
	{
		private readonly DragGesture dragGesture;
		public EditBox Editor;
		public TextArea()
		{
			Gestures.Add(dragGesture = new DragGesture());
			Tasks.Add(ResizeTask);
		}

		private IEnumerator<object> ResizeTask()
		{
			while (true) {
				if ((LocalMousePosition() - Size - Vector2.One * 5f).Length < 10f) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeAll);
					var ipos = LocalMousePosition();
					var isize = MinSize;
					if (dragGesture.WasBegan()) {
						while (!dragGesture.WasEnded()) {
							MinSize = isize + Vector2.Down * (LocalMousePosition() - ipos).Y;
							yield return null;
						}
					}
				}
				yield return null;
			}
		}
	}
}

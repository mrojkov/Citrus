using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class CurveEditorVerticalZoomProcessor : Core.ITaskProvider
	{
		readonly CurveEditorPane curveEditor;

		public CurveEditorVerticalZoomProcessor(CurveEditorPane curveEditor) { this.curveEditor = curveEditor; }

		public IEnumerator<object> Task()
		{
			var input = curveEditor.MainAreaWidget.Input;
			while (true) {
				if (curveEditor.MainAreaWidget.IsMouseOver()) {
					if (input.WasKeyPressed(Key.MouseWheelDown)) {
						ZoomCurveEditor(false);
						Window.Current.Invalidate();
					}
					if (input.WasKeyPressed(Key.MouseWheelUp)) {
						ZoomCurveEditor(true);
						Window.Current.Invalidate();
					}
				}
				yield return null;
			}
		}

		void ZoomCurveEditor(bool zoomIn)
		{
			var d = curveEditor.MaxValue - curveEditor.MinValue;
			if (d < 0.1f && zoomIn || d > 100000 && !zoomIn) {
				return;
			}
			var v = curveEditor.CoordToValue(curveEditor.MainAreaWidget.LocalMousePosition().Y);
			var k = zoomIn ? 0.9f : 1.1f;
			curveEditor.MinValue = (curveEditor.MinValue - v) * k + v;
			curveEditor.MaxValue = (curveEditor.MaxValue - v) * k + v;
		}
	}
}

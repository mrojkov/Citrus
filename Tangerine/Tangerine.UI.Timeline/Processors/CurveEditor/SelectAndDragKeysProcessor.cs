using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class CurveEditorSelectAndDragKeysProcessor : ITaskProvider
	{
		readonly CurveEditorPane curveEditor;

		public CurveEditorSelectAndDragKeysProcessor(CurveEditorPane curveEditor)
		{
			this.curveEditor = curveEditor;
		}

		public IEnumerator<object> Task()
		{
			var widget = curveEditor.MainAreaWidget;
			var input = CommonWindow.Current.Input;
			while (true) {
				if (widget.IsMouseOver() && input.WasKeyPressed(Key.Mouse0)) {
					input.ConsumeKey(Key.Mouse0);
					using (Document.Current.History.BeginTransaction()) {
						var initialMousePos = curveEditor.ContentWidget.LocalMousePosition();
						var currentMousePos = initialMousePos;
						var rectanglePresenter = new SyncDelegatePresenter<Widget>(w => {
							w.PrepareRendererState();
							Renderer.DrawRectOutline(initialMousePos, currentMousePos, ColorTheme.Current.TimelineCurveEditor.Selection);
						});
						curveEditor.ContentWidget.CompoundPostPresenter.Add(rectanglePresenter);
						while (input.IsMousePressed()) {
							currentMousePos = curveEditor.ContentWidget.LocalMousePosition();
							var rect = new Rectangle(initialMousePos, currentMousePos);
							foreach (var c in curveEditor.Curves) {
								SelectKeysWithinRectangle(c, rect);
							}
							Window.Current.Invalidate();
							yield return null;
						}
						curveEditor.ContentWidget.CompoundPostPresenter.Remove(rectanglePresenter);
						Window.Current.Invalidate();
						Document.Current.History.CommitTransaction();
					}
				}
				yield return null;
			}
		}

		void SelectKeysWithinRectangle(Curve curve, Rectangle rect)
		{
			foreach (var k in curve.Animator.ReadonlyKeys.ToList()) {
				var p = curveEditor.CalcPosition(curve, k.Frame);
				var keySelected = curve.SelectedKeys.Contains(k);
				if (!rect.Contains(p) && keySelected) {
					Operations.SelectCurveKey.Perform(curve, k, false);
				} else if (rect.Contains(p) && !keySelected) {
					Operations.SelectCurveKey.Perform(curve, k, true);
				}
			}
		}
	}
}

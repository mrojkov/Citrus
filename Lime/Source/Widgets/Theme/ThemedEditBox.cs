#if !ANDROID && !iOS
using System;
using System.Collections.Generic;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedEditBox : EditBox
	{
		public override bool IsNotDecorated() => false;

		public ThemedEditBox()
		{
			Decorate(this);
		}

		internal static void Decorate(CommonEditBox eb)
		{
			var tw = eb.TextWidget;
			ThemedSimpleText.Decorate(tw);
			tw.ForceUncutText = false;
			eb.MinSize = Theme.Metrics.DefaultEditBoxSize;
			eb.MaxHeight = eb.MinHeight;
			tw.Localizable = false;
			tw.TrimWhitespaces = false;
			tw.OverflowMode = TextOverflowMode.Ignore;
			tw.Padding = Theme.Metrics.ControlsPadding;
			tw.VAlignment = VAlignment.Center;

			var editorParams = new EditorParams {
				MaxLines = 1, Scroll = eb.ScrollView,
				OffsetContextMenu = p => p + new Vector2(1f, tw.FontHeight + 1f),
				SelectAllOnFocus = true
			};
			eb.Editor = new Editor(displayWidget: tw, editorParams: editorParams, focusableWidget: eb, clickableWidget: eb.ScrollWidget);
			var vc = new VerticalLineCaret { Color = Theme.Colors.TextCaret };
			eb.Updated += delta => {
				vc.Width = eb.Editor.OverwriteMode && !eb.Editor.HasSelection() ?
					tw.Font.Chars.Get(eb.Editor.CurrentChar(), tw.FontHeight)?.Width ?? 5f : 0f;
				if (eb.IsMouseOverThisOrDescendant() && WidgetContext.Current.MouseCursor == MouseCursor.Default) {
					var rect = new Rectangle {
						A = tw.ContentPosition,
						B = tw.ContentPosition + tw.ContentSize,
					};
					if (rect.Contains(tw.LocalMousePosition())) {
						WidgetContext.Current.MouseCursor = MouseCursor.IBeam;
					}
				}
			};
			new CaretDisplay(
				tw, eb.Editor.CaretPos, new CaretParams { CaretPresenter = vc });
			new SelectionPresenter(
				tw, eb.Editor.SelectionStart, eb.Editor.SelectionEnd,
				new SelectionParams {
					Padding = Thickness.Zero,
					OutlineColor = Theme.Colors.TextSelection,
					Color = Theme.Colors.TextSelection
				});

			eb.TabTravesable = new TabTraversable();
			eb.CompoundPresenter.Add(new ThemedFramePresenter(Theme.Colors.WhiteBackground, Theme.Colors.ControlBorder));
			eb.CompoundPostPresenter.Add(new Theme.KeyboardFocusBorderPresenter());
			eb.CompoundPostPresenter.Add(new Theme.MouseHoverBorderPresenter());
			eb.LateTasks.Add(Theme.MouseHoverInvalidationTask(eb));
		}
	}

	[YuzuDontGenerateDeserializer]
	public class ThemedNumericEditBox : NumericEditBox
	{
		public override bool IsNotDecorated() => false;

		public ThemedNumericEditBox()
		{
			ThemedEditBox.Decorate(this);
			MinWidth = 0.0f;
			MaxWidth = 105.0f;
			TextWidget.Padding = new Thickness(left: 5.0f, right: 5.0f, top: 2.0f, bottom: 2.0f);
			Layout = new HBoxLayout();
			// Lime.EditorParams.MouseSelectionThreshold is 3 by default and this gesture should be recognized first.
			// To achieve that we're setting its drag threshold to 2.0f, add it to Editor.ClickableWidget (same collection
			// editor adds its widgets to) and do it in LateTasks.
			var dragGesture = new DragGesture(exclusive: true, dragThreshold: 2.0f);
			Updated += (delta) => {
				if (Editor.FocusableWidget.IsFocused()) {
					dragGesture.Cancel();
				} else if (IsMouseOverThisOrDescendant() || isDragging) {
					WidgetContext.Current.MouseCursor = MouseCursor.SizeWE;
				}
			};
			LateTasks.Add(Task.Repeat(() => {
				dragGesture.Recognized += () => {
					if (!GloballyEnabled) {
						return;
					}
					if (Focused != null && !this.SameOrDescendantOf(Focused)) {
						Focused.RevokeFocus();
					}
					Tasks.Add(SpinByDragTask(dragGesture));
				};
				Editor.ClickableWidget.Gestures.Insert(0, dragGesture);
				return false;
			}));
		}

		private bool isDragging;

		private IEnumerator<object> SpinByDragTask(DragGesture dragGesture)
		{
			RaiseBeginSpin();
			try {
				isDragging = true;
				var startValue = Value;
				var startMousePosition = Application.DesktopMousePosition;
				float accumulator = 0.0f;
				while (dragGesture.IsChanging()) {
					var mousePosition = Application.DesktopMousePosition;
					var disp = CommonWindow.Current.Display;
					var wrapped = false;
					if (Application.DesktopMousePosition.X > disp.Position.X + disp.Size.X - 5) {
						accumulator += disp.Position.X + disp.Size.X - 5 - startMousePosition.X;
						startMousePosition.X = mousePosition.X = disp.Position.X + 5;
						wrapped = true;
					}
					if (Application.DesktopMousePosition.X < disp.Position.X + 5) {
						accumulator -= startMousePosition.X - disp.Position.X - 5;
						startMousePosition.X = mousePosition.X = disp.Position.X + disp.Size.X - 5;
						wrapped = true;
					}
					if (wrapped) {
						Application.DesktopMousePosition = new Vector2(mousePosition.X, Application.DesktopMousePosition.Y);
					}
					var delta = (mousePosition.X - startMousePosition.X + accumulator).Round() * Step;
					if (Input.IsKeyPressed(Key.Shift)) {
						delta *= 10f;
					} else if (Input.IsKeyPressed(Key.Control)) {
						delta *= 0.1f;
					}
					if (!IsReadOnly) {
						Value = startValue + delta;
					}
					yield return null;
				}
			} finally {
				isDragging = false;
				RaiseEndSpin();
			}
		}
	}
}
#endif

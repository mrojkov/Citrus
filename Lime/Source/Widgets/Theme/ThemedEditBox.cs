#if !ANDROID && !iOS
using System;
using System.Collections.Generic;

namespace Lime
{
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
			eb.Editor = new Editor(tw, editorParams, eb, eb.ScrollWidget);
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

	public class ThemedNumericEditBox : NumericEditBox
	{
		public override bool IsNotDecorated() => false;

		public ThemedNumericEditBox()
		{
			ThemedEditBox.Decorate(this);
			MinMaxWidth = 80;
			TextWidget.Padding = new Thickness(2);
			Layout = new HBoxLayout();
			Nodes.Insert(0, CreateSpinButton(SpinButtonType.Subtractive));
			Nodes.Add(CreateSpinButton(SpinButtonType.Additive));
		}

		enum SpinButtonType
		{
			Subtractive,
			Additive
		}

		Widget CreateSpinButton(SpinButtonType type)
		{
			var button = new Widget {
				HitTestTarget = true,
				LayoutCell = new LayoutCell { StretchX = 0 },
				MinWidth = SpinButtonPresenter.ButtonWidth,
				PostPresenter = new SpinButtonPresenter(type)
			};
			button.Awoke += instance => {
				var dragGesture = new DragGesture();
				dragGesture.Recognized += () => Tasks.Add(SpinByDragTask(dragGesture));
				var clickGesture = new ClickGesture(() => {
					var delta = (type == SpinButtonType.Additive ? 1 : -1) * Step;
					if (Input.IsKeyPressed(Key.Shift)) {
						delta *= 10f;
					} else if (Input.IsKeyPressed(Key.Control)) {
						delta *= 0.1f;
					}
					if (!IsReadOnly) {
						Value += delta;
					}
				});
				var gestures = ((Widget)instance).Gestures;
				gestures.Add(clickGesture);
				gestures.Add(dragGesture);
			};
			return button;
		}

		private IEnumerator<object> SpinByDragTask(DragGesture dragGesture)
		{
			RaiseBeginSpin();
			try {
				var prevMousePos = Application.DesktopMousePosition;
				while (dragGesture.IsChanging()) {
					var disp = CommonWindow.Current.Display;
					var wrapped = false;
					if (Application.DesktopMousePosition.X > disp.Position.X + disp.Size.X - 5) {
						prevMousePos.X = disp.Position.X + 5;
						wrapped = true;
					}
					if (Application.DesktopMousePosition.X < disp.Position.X + 5) {
						prevMousePos.X = disp.Position.X + disp.Size.X - 5;
						wrapped = true;
					}
					if (wrapped) {
						Application.DesktopMousePosition = new Vector2(prevMousePos.X, Application.DesktopMousePosition.Y);
					}
					var delta = (Application.DesktopMousePosition.X - prevMousePos.X).Round() * Step;
					if (Input.IsKeyPressed(Key.Shift)) {
						delta *= 10f;
					} else if (Input.IsKeyPressed(Key.Control)) {
						delta *= 0.1f;
					}
					if (!IsReadOnly) {
						Value += delta;
					}
					prevMousePos = Application.DesktopMousePosition;
					yield return null;
				}
			} finally {
				RaiseEndSpin();
			}
		}

		class SpinButtonPresenter : CustomPresenter
		{
			public const float ButtonWidth = 10;

			static Color4 color = Color4.Lerp(0.25f, Theme.Colors.ControlBorder, Theme.Colors.BlackText);

			private readonly VectorShape buttonShape = new VectorShape {
				new VectorShape.TriangleFan(new float[] { 0.3f, 0.3f, 0.7f, 0.5f, 0.3f, 0.7f }, color)
			};

			private SpinButtonType type;

			public SpinButtonPresenter(SpinButtonType type)
			{
				this.type = type;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Matrix32 transform;
				if (type == SpinButtonType.Additive) {
					transform = Matrix32.Scaling(ButtonWidth, widget.Height - 2) * Matrix32.Translation(1, 1);
				} else {
					transform = Matrix32.Scaling(-ButtonWidth, widget.Height - 2) * Matrix32.Translation(ButtonWidth + 1, 1);
				}
				buttonShape.Draw(transform);
			}
		}
	}
}
#endif

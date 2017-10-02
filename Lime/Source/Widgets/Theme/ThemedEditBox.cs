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
				MaxLines = 1, Scroll = eb.Scroll,
				OffsetContextMenu = p => p + new Vector2(1f, tw.FontHeight + 1f),
				SelectAllOnFocus = true
			};
			eb.Editor = new Editor(tw, editorParams, eb);
			var vc = new VerticalLineCaret { Color = Theme.Colors.TextCaret };
			eb.Updated += delta => {
				vc.Width = eb.Editor.OverwriteMode && !eb.Editor.HasSelection() ?
					tw.Font.Chars.Get(eb.Editor.CurrentChar(), tw.FontHeight)?.Width ?? 5f : 0f;
				if (eb.IsMouseOverThisOrDescendant() && WidgetContext.Current.MouseCursor == MouseCursor.Default) {
					var rect = new Rectangle {
						A = tw.ContentPosition,
						B = tw.ContentPosition + tw.ContentSize,
					};
					if (rect.Contains(tw.Input.LocalMousePosition)) {
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
			TextWidget.Padding = new Thickness(SpinButtonPresenter.ButtonWidth + 2, 2);
			CompoundPostPresenter.Add(new SpinButtonPresenter(true));
			CompoundPostPresenter.Add(new SpinButtonPresenter(false));
		}

		protected override void Awake()
		{
			base.Awake();
			Tasks.Add(HandleSpinButtonTask(true));
			Tasks.Add(HandleSpinButtonTask(false));
		}

		private IEnumerator<object> HandleSpinButtonTask(bool leftToRight)
		{
			while (true) {
				if (Input.WasMousePressed()) {
					if (
						leftToRight && Input.LocalMousePosition.X > Width - SpinButtonPresenter.ButtonWidth ||
						!leftToRight && Input.LocalMousePosition.X < SpinButtonPresenter.ButtonWidth
					) {
						RaiseBeginSpin();
						Input.CaptureMouse();
						Input.ConsumeKey(Key.Mouse0);
						var prevMousePos = Application.DesktopMousePosition;
						var dragged = false;
						var disp = Window.Current.Display;
						// TODO: Remove focus revoke and block editor input while dragging.
						SetFocus();
						RevokeFocus();
						while (Input.IsMousePressed()) {
							dragged |= Application.DesktopMousePosition != prevMousePos;
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
						if (!dragged) {
							var delta = (leftToRight ? 1 : -1) * Step;
							if (Input.IsKeyPressed(Key.Shift)) {
								delta *= 10f;
							} else if (Input.IsKeyPressed(Key.Control)) {
								delta *= 0.1f;
							}
							if (!IsReadOnly) {
								Value += delta;
							}
						}
						Input.ConsumeKey(Key.Mouse0);
						Input.ReleaseMouse();
						RaiseEndSpin();
					}
				}
				yield return null;
			}
		}

		class SpinButtonPresenter : CustomPresenter
		{
			public const float ButtonWidth = 10;

			static Color4 color = Color4.Lerp(0.25f, Theme.Colors.ControlBorder, Theme.Colors.BlackText);

			private readonly VectorShape buttonShape = new VectorShape {
				new VectorShape.TriangleFan(new float[] { 0.3f, 0.3f, 0.7f, 0.5f, 0.3f, 0.7f }, color)
			};

			private bool leftToRight;

			public SpinButtonPresenter(bool leftToRight)
			{
				this.leftToRight = leftToRight;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Matrix32 transform;
				if (leftToRight) {
					transform = Matrix32.Scaling(ButtonWidth, widget.Height - 2) * Matrix32.Translation(widget.Width - ButtonWidth - 1, 1);
				} else {
					transform = Matrix32.Scaling(-ButtonWidth, widget.Height - 2) * Matrix32.Translation(ButtonWidth + 1, 1);
				}
				buttonShape.Draw(transform);
			}
		}
	}
}
#endif
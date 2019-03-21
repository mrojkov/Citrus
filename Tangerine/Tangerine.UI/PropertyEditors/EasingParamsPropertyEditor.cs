using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.UI
{
	public class EasingParamsPropertyEditor : CommonPropertyEditor<EasingParams>
	{
		public EasingParamsPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			EditorContainer.AddNode(new EasingEditorPanel(this, CoalescedPropertyValue(), (EasingParams p) => SetProperty(p)).Widget);
			EditorContainer.Gestures.Add(new ClickGesture(1, () => {
				var reset = new Command("Reset", () => {
					DoTransaction(() => SetProperty<EasingParams>(_ => EasingParams.Default));
				});
				new Menu { reset }.Popup();
			}));
		}

		private class EasingEditorPanel
		{
			public Widget Widget { get; }
			private IDataflowProvider<CoalescedValue<EasingParams>> valueProvider;
			private Widget canvas;

			public EasingEditorPanel(
				CommonPropertyEditor<EasingParams> editor,
				IDataflowProvider<CoalescedValue<EasingParams>> valueProvider,
				Action<EasingParams> propertySetter)
			{
				this.valueProvider = valueProvider;
				canvas = new Widget {
					MinMaxSize = new Vector2(160, 120),
					Padding = new Thickness(6),
					LayoutCell = new LayoutCell(Alignment.Center),
					HitTestTarget = true
				};
				Widget = new Frame {
					ClipChildren = ClipMethod.ScissorTest,
					Layout = new StackLayout(),
					Nodes = { canvas, new Widget() }
				};
				new BezierHandle(0, editor, valueProvider, propertySetter, canvas);
				new BezierHandle(1, editor, valueProvider, propertySetter, canvas);
				canvas.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(RenderSpline));
				canvas.CompoundPostPresenter.Add(new WidgetBoundsPresenter(ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox));
			}

			private void RenderSpline(Widget widget)
			{
				widget.PrepareRendererState();
				var ep = valueProvider.GetValue().Value;
				var bezier = new CubicBezier(ep.P1X, ep.P1Y, ep.P2X, ep.P2Y);
				var p0 = Vector2.Zero;
				for (var t = 0f; t < 1; t += 0.01f) {
					var p1 = new Vector2((float)bezier.SampleCurveX(t), (float)bezier.SampleCurveY(t));
					Renderer.DrawLine(Project(canvas, p0), Project(canvas, p1), ColorTheme.Current.Basic.SelectedBorder, 2);
					p0 = p1;
				}
			}

			private static Vector2 Project(Widget canvas, Vector2 p)
			{
				return new Vector2(
					p.X * canvas.ContentWidth + canvas.ContentPosition.X,
					(1 - p.Y) * canvas.ContentHeight + canvas.ContentPosition.Y
				);
			}

			private static Vector2 Unproject(Widget canvas, Vector2 p)
			{
				return new Vector2(
					(p.X - canvas.ContentPosition.X) / canvas.ContentWidth,
					1 - (p.Y - canvas.ContentPosition.Y) / canvas.ContentHeight
				);
			}

			private class BezierHandle
			{
				private int index;
				private readonly CommonPropertyEditor<EasingParams> editor;
				private readonly IDataflowProvider<CoalescedValue<EasingParams>> valueProvider;
				private readonly Action<EasingParams> propertySetter;
				private readonly Widget canvas;

				public BezierHandle(
					int index,
					CommonPropertyEditor<EasingParams> editor,
					IDataflowProvider<CoalescedValue<EasingParams>> valueProvider,
					Action<EasingParams> propertySetter,
					Widget canvas)
				{
					this.index = index;
					this.editor = editor;
					this.valueProvider = valueProvider;
					this.propertySetter = propertySetter;
					this.canvas = canvas;
					canvas.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
					canvas.Tasks.Add(DragTask(canvas));
				}

				private IEnumerator<object> DragTask(Widget canvas)
				{
					while (true) {
						yield return null;
						if (!canvas.Input.WasMousePressed()) {
							continue;
						}
						var d = canvas.LocalMousePosition() - Project(canvas, GetPosition());
						if (d.Length > 7) {
							continue;
						}
						canvas.Input.ConsumeKey(Key.Mouse0);
						var history = editor.EditorParams.History;
						try {
							history?.BeginTransaction();
							var v = valueProvider.GetValue().Value;
							while (canvas.Input.IsMousePressed()) {
								history?.RollbackTransaction();
								v = valueProvider.GetValue().Value;
								var p = Unproject(canvas, canvas.LocalMousePosition());
								p.X = Mathf.Clamp(p.X, 0, 1);
								p.Y = Mathf.Clamp(p.Y, 0, 1);
								if (index == 0) {
									v.P1 = p;
								} else {
									v.P2 = p;
								}
								propertySetter(v);
								yield return null;
							}
							propertySetter(v);
							history?.CommitTransaction();
						} finally {
							history?.EndTransaction();
						}
					}
				}

				private Vector2 GetPosition()
				{
					var ep = valueProvider.GetValue().Value;
					return (index == 0) ? new Vector2(ep.P1X, ep.P1Y) : new Vector2(ep.P2X, ep.P2Y);
				}

				private void Render(Widget widget)
				{
					widget.PrepareRendererState();
					var controlsColor = ColorTheme.Current.Basic.GrayText;
					var origin = index == 0 ? Vector2.Zero : Vector2.One;
					Renderer.DrawLine(Project(canvas, origin), Project(canvas, GetPosition()), controlsColor, 2);
					Renderer.DrawRound(Project(canvas, origin), 2, 15, controlsColor);
					Renderer.DrawRound(Project(canvas, GetPosition()), 3, 15, controlsColor);
				}
			}
		}
	}
}

using System;

namespace Lime
{
	/// <summary>
	/// Root of the widgets hierarchy.
	/// </summary>
	public class WindowWidget : Widget, IPresenter
	{
		private IKeyboardInputProcessor prevActiveTextWidget;
		private RenderChain renderChain = new RenderChain();
		private bool continuousRendering;

		public IWindow Window { get; private set; }

		public WindowWidget(IWindow window, bool continuousRendering = true)
		{
			this.continuousRendering = continuousRendering;
			Window = window;
			new WidgetContext(this);
			Window.Context = new CombinedContext(Window.Context, WidgetContext.Current);
			Presenter = this;
			Theme.Current.Apply(this);
		}

		public override void Update(float delta)
		{
			if (!continuousRendering) {
				Window.Invalidate();
			}
			var context = WidgetContext.Current;
			WidgetInput.RemoveInvalidatedCaptures();
			context.IsActiveTextWidgetUpdated = false;
			context.DistanceToNodeUnderCursor = float.MaxValue;
			base.Update(delta);
			if (Application.CurrentThread.IsMain()) {
				if (!context.IsActiveTextWidgetUpdated || Window.Input.WasKeyPressed(Key.DismissSoftKeyboard)) {
					context.ActiveTextWidget = null;
				}
				bool showKeyboard = context.ActiveTextWidget != null && context.ActiveTextWidget.Visible;
				if (prevActiveTextWidget != context.ActiveTextWidget) {
					Application.SoftKeyboard.Show(showKeyboard, context.ActiveTextWidget != null ? context.ActiveTextWidget.Text : "");
				}
				// Handle switching between various text widgets
				if (
					prevActiveTextWidget != context.ActiveTextWidget &&
					context.ActiveTextWidget != null &&
					prevActiveTextWidget != null
				) {
					Application.SoftKeyboard.ChangeText(context.ActiveTextWidget.Text);
				}
				prevActiveTextWidget = context.ActiveTextWidget;
			}
			LayoutManager.Instance.Layout();
		}

		void IPresenter.Render()
		{
			SetViewport();
			foreach (var node in Nodes) {
				node.AddToRenderChain(renderChain);
			}
			renderChain.RenderAndClear();
		}

		IPresenter IPresenter.Clone(Node newNode)
		{
			return (IPresenter)newNode;
		}

		public void SetViewport()
		{
			Renderer.Viewport = new WindowRect {
				X = 0, Y = 0,
				Width = (int)(Window.ClientSize.Width * Window.PixelScale),
				Height = (int)(Window.ClientSize.Height * Window.PixelScale)
			};
		}
	}

	public class DefaultWindowWidget : WindowWidget
	{
		public DefaultWindowWidget(Window window, bool continuousRendering = true)
			: base(window, continuousRendering)
		{
			window.Rendering += () => {
				Renderer.BeginFrame();
				Size = (Vector2)window.ClientSize;
				Renderer.SetOrthogonalProjection(Vector2.Zero, Size);
				Render();
				Renderer.EndFrame();			
			};
			window.Updating += Update;
		}
	}
}
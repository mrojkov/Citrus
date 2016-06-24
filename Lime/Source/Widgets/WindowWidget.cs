using System;

namespace Lime
{
	/// <summary>
	/// Root of the widgets hierarchy.
	/// </summary>
	public class WindowWidget : Widget
	{
		private RenderChain renderChain = new RenderChain();
		private bool continuousRendering;

		public IWindow Window { get; private set; }

		public WindowWidget(IWindow window, bool continuousRendering = true)
		{
			this.continuousRendering = continuousRendering;
			Window = window;
			Window.Context = new CombinedContext(Window.Context, new WidgetContext(this));
			Theme.Current.Apply(this);
		}

		public override void Update(float delta)
		{
			if (continuousRendering) {
				Window.Invalidate();
			}
			WidgetContext.Current.MouseCursor = MouseCursor.Default;
			base.Update(delta);
			if (Window.Input.WasKeyPressed(Key.DismissSoftKeyboard)) {
				KeyboardFocus.Instance.SetFocus(null);
			}
			Window.Cursor = WidgetContext.Current.MouseCursor;
			LayoutManager.Instance.Layout();
			renderChain.Clear();
			AddContentsToRenderChain(renderChain);
			var hitTestArgs = new HitTestArgs(Window.Input.MousePosition);
			renderChain.HitTest(ref hitTestArgs);
			WidgetContext.Current.NodeUnderMouse = hitTestArgs.Node;
		}

		public void RenderAll()
		{
			SetViewport();
			renderChain.Render();
		}

		public void SetViewport()
		{
			Renderer.Viewport = new WindowRect {
				X = 0, Y = 0,
				Width = (int)(Window.ClientSize.X * Window.PixelScale),
				Height = (int)(Window.ClientSize.Y * Window.PixelScale)
			};
		}
	}

	public class DefaultWindowWidget : WindowWidget
	{
		public bool CornerBlinkOnRendering;

		public DefaultWindowWidget(Window window, bool continuousRendering = true)
			: base(window, continuousRendering)
		{
			Theme.Current.Apply(this, typeof(WindowWidget));
			window.Rendering += () => {
				Renderer.BeginFrame();
				Renderer.SetOrthogonalProjection(Vector2.Zero, Size);
				RenderAll();
				if (CornerBlinkOnRendering) {
					RenderRedrawMark();
				}				Renderer.EndFrame();
			};
			window.Updating += delta => {
				Size = (Vector2)window.ClientSize;
				Update(delta);
			};
		}

		void RenderRedrawMark()
		{
			Renderer.Transform1 = Matrix32.Identity;
			Renderer.Blending = Blending.Alpha;
			Renderer.Shader = ShaderId.Diffuse;
			Renderer.DrawRect(Vector2.Zero, Vector2.One * 4, RandomColor());
		}

		private Color4 RandomColor()
		{
			return new Color4((byte)Mathf.RandomInt(0, 255), (byte)Mathf.RandomInt(0, 255), (byte)Mathf.RandomInt(0, 255));
		}
	}
}
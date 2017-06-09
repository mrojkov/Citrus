using System;
using System.Linq;

namespace Lime
{
	/// <summary>
	/// Root of the widgets hierarchy.
	/// </summary>
	public class WindowWidget : Widget
	{
		private bool windowActivated;
		private Widget lastFocused;
		private readonly RenderChain renderChain;

		public IWindow Window { get; private set; }

		public WindowWidget(IWindow window)
		{
			Window = window;
			Window.Context = new CombinedContext(Window.Context, new WidgetContext(this));
			renderChain = new RenderChain();
			Theme.Current.Apply(this);
			window.Activated += () => windowActivated = true;
		}

		protected virtual bool ContinuousRendering() { return true; }

		public override void Update(float delta)
		{
			if (ContinuousRendering()) {
				Window.Invalidate();
			}
			WidgetContext.Current.MouseCursor = MouseCursor.Default;
			base.Update(delta);
			if (Window.Input.WasKeyPressed(Key.DismissSoftKeyboard)) {
				Widget.SetFocus(null);
			}
			Window.Cursor = WidgetContext.Current.MouseCursor;
			LayoutManager.Instance.Layout();
			renderChain.Clear();
			RenderChainBuilder?.AddToRenderChain(this, renderChain);
			var hitTestArgs = new HitTestArgs(Window.Input.MousePosition);
			renderChain.HitTest(ref hitTestArgs);
			WidgetContext.Current.NodeUnderMouse = hitTestArgs.Node;
			ManageFocusOnWindowActivation();
		}

		private void ManageFocusOnWindowActivation()
		{
			if (Window.Active) {
				if (Widget.Focused != null && Widget.Focused.DescendantOrThis(this)) {
					lastFocused = Widget.Focused;
				}
			}
			if (windowActivated) {
				windowActivated = false;
				if (lastFocused == null || !lastFocused.GloballyVisible || !lastFocused.DescendantOrThis(this)) {
					// Looking the first focus scope widget on the window and make it focused.
					lastFocused = Descendants.OfType<Widget>().FirstOrDefault(i => i.FocusScope != null && i.GloballyVisible);
				}
				Widget.SetFocus(lastFocused);
			}
		}

		public virtual void RenderAll()
		{
			Renderer.Viewport = GetViewport();
			renderChain.Render();
		}

		public WindowRect GetViewport()
		{
			return new WindowRect {
				X = 0, Y = 0,
				Width = (int)(Window.ClientSize.X * Window.PixelScale),
				Height = (int)(Window.ClientSize.Y * Window.PixelScale)
			};
		}

		public Matrix44 GetProjection() => Matrix44.CreateOrthographicOffCenter(0, Width, Height, 0, -50, 50);
	}

	public class DefaultWindowWidget : WindowWidget
	{
		public bool LayoutBasedWindowSize { get; set; }

		public DefaultWindowWidget(Window window)
			: base(window)
		{
			Theme.Current.Apply(this, typeof(WindowWidget));
			window.Rendering += () => {
				Renderer.BeginFrame();
				Renderer.Projection = GetProjection();
				RenderAll();
				Renderer.EndFrame();
			};
			window.Updating += UpdateAndResize;
			window.Resized += deviceRotated => UpdateAndResize(0);
			window.VisibleChanging += showing => {
				if (showing) {
					UpdateAndResize(0);
					window.Center();
				}
			};
		}

		private void UpdateAndResize(float delta)
		{
			if (LayoutBasedWindowSize) {
				Update(delta); // Update widgets in order to deduce EffectiveMinSize.
				Size = Window.ClientSize = EffectiveMinSize;
			} else {
				Size = Window.ClientSize;
				Update(delta);
			}
		}
	}

	public class InvalidableWindowWidget : DefaultWindowWidget
	{
		public bool RedrawMarkVisible { get; set; }

		public InvalidableWindowWidget(Window window)
			: base(window)
		{
			Theme.Current.Apply(this, typeof(WindowWidget));
		}

		protected override bool ContinuousRendering() { return false; }

		public override void RenderAll ()
		{
			base.RenderAll ();
			if (RedrawMarkVisible) {
				RenderRedrawMark();
			}
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
			return new Color4(RandomByte(), RandomByte(), RandomByte());
		}

		private byte RandomByte()
		{
			return (byte)Mathf.RandomInt(0, 255);
		}
	}
}
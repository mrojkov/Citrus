using System;
using System.Collections.Generic;
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
		protected readonly RenderChain renderChain;

		public IWindow Window { get; private set; }

		public WindowWidget(IWindow window)
		{
			var widgetContext = new WidgetContext(this);
			Window = window;
			Window.Context = new CombinedContext(Window.Context, widgetContext);
			renderChain = new RenderChain();
			widgetContext.GestureManager = new GestureManager(widgetContext);
			window.Activated += () => windowActivated = true;
			LayoutManager = new LayoutManager();
		}

		protected virtual bool ContinuousRendering() { return true; }

		private bool prevAnyCaptureKeyPressed;

		public override void Update(float delta)
		{
			if (ContinuousRendering()) {
				Window.Invalidate();
			}
			var context = WidgetContext.Current;

			// Find the node under mouse, using the render chain built on one frame before.
			context.NodeUnderMouse = LookForNodeUnderMouse(renderChain);

			// Assign NodeCapturedByMouse if any mouse button was pressed.
			var anyCaptureKeyPressed = IsAnyCaptureKeyPressed();
			if (!prevAnyCaptureKeyPressed && anyCaptureKeyPressed) {
				context.NodeCapturedByMouse = context.NodeUnderMouse;
			}

			// Process mouse/touch screen input.
			context.GestureManager.Process();

			// Update the widget hierarchy.
			context.MouseCursor = MouseCursor.Default;
			base.Update (delta);
			Window.Cursor = context.MouseCursor;

			// Set NodeCapturedByMouse to null if all mouse buttons were released.
			if (prevAnyCaptureKeyPressed && !anyCaptureKeyPressed) {
				context.NodeCapturedByMouse = null;
			}
			prevAnyCaptureKeyPressed = anyCaptureKeyPressed;

			if (Window.Input.WasKeyPressed(Key.DismissSoftKeyboard)) {
				SetFocus(null);
			}

			// Refresh widgets layout.
			LayoutManager.Layout();

			// Rebuild the render chain.
			renderChain.Clear();
			renderChain.ClipRegion = new Rectangle(Vector2.Zero, Size);
			RenderChainBuilder?.AddToRenderChain(renderChain);

			ManageFocusOnWindowActivation();
		}

		private bool IsAnyCaptureKeyPressed()
		{
			foreach (var key in WidgetContext.NodeCaptureKeys) {
				if (Window.Input.IsKeyPressed(key)) {
					return true;
				}
			}
			return false;
		}

		private void ManageFocusOnWindowActivation()
		{
			if (Window.Active) {
				if (Widget.Focused != null && Widget.Focused.SameOrDescendantOf(this)) {
					lastFocused = Widget.Focused;
				}
			}
			if (windowActivated) {
				windowActivated = false;
				if (lastFocused == null || !lastFocused.GloballyVisible || !lastFocused.SameOrDescendantOf(this)) {
					// Looking the first focus scope widget on the window and make it focused.
					lastFocused = Descendants.OfType<Widget>().FirstOrDefault(i => i.FocusScope != null && i.GloballyVisible);
				}
				Widget.SetFocus(lastFocused);
			}
		}

		private Node LookForNodeUnderMouse(RenderChain renderChain)
		{
#if iOS || ANDROID
			if (!Window.Input.IsTouching(0) && !Window.Input.WasTouchEnded(0)) {
				return null;
			}
#endif
			var hitTestArgs = new HitTestArgs(Window.Input.MousePosition);
			renderChain.HitTest(ref hitTestArgs);
			var n = hitTestArgs.Node;
			if (
				n != null &&
				WidgetInput.InputScopeStack.Top != null &&
				!n.SameOrDescendantOf(WidgetInput.InputScopeStack.Top)
			) {
				n = null;
			}
			return n;
		}

		public virtual void RenderAll()
		{
			Renderer.Viewport = new Viewport(GetViewport());
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

		public DefaultWindowWidget(IWindow window)
			: base(window)
		{
			window.Rendering += () => {
				Renderer.BeginFrame();
				Renderer.Projection = GetProjection();
				RenderAll();
				Renderer.EndFrame();
			};
			window.Updating += UpdateAndResize;
			window.Resized += deviceRotated => UpdateAndResize(0);
			window.VisibleChanging += Window_VisibleChanging;
		}

		private void Window_VisibleChanging(bool showing, bool modal)
		{
			if (modal && showing) {
				Input.RestrictScope();
			}
			if (!showing)
				Input.DerestrictScope();{
			}
			if (showing) {
				UpdateAndResize(0);
				Window.Center();
			}
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

		public InvalidableWindowWidget(IWindow window)
			: base(window)
		{
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
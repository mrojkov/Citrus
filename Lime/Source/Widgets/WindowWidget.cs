using System;

namespace Lime
{
	/// <summary>
	/// Root of the widgets hierarchy.
	/// </summary>
	public class WindowWidget : Widget
	{
#if iOS || ANDROID
		private IKeyboardInputProcessor prevActiveTextWidget;
#endif
		private RenderChain renderChain = new RenderChain();
		
		public IWindow Window { get; private set; }
		public IWidgetContext Context { get; private set; }
		
		public WindowWidget(IWindow window)
		{
			Window = window;
			Context = new WidgetContext(window, this);
			Window.Context = Context;
		}

		public override void Update(float delta)
		{
			WidgetInput.RemoveInvalidatedCaptures();
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			Context.IsActiveTextWidgetUpdated = false;
			Context.DistanceToNodeUnderCursor = float.MaxValue;
			base.Update(delta);
			if (!Context.IsActiveTextWidgetUpdated || Window.Input.WasKeyPressed(Key.DismissSoftKeyboard)) {
				Context.ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.CurrentThread.IsMain()) {
				bool showKeyboard = Context.ActiveTextWidget != null && Context.ActiveTextWidget.Visible;
				if (prevActiveTextWidget != Context.ActiveTextWidget) {
					Application.SoftKeyboard.Show(showKeyboard, Context.ActiveTextWidget != null ? Context.ActiveTextWidget.Text : "");
				}
				// Handle switching between various text widgets
				if (prevActiveTextWidget != Context.ActiveTextWidget && Context.ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.SoftKeyboard.ChangeText(Context.ActiveTextWidget.Text);
				}
				prevActiveTextWidget = Context.ActiveTextWidget;
			}
#endif
		}

		public override void Render()
		{
			foreach (var node in Nodes) {
				node.AddToRenderChain(renderChain);
			}
			renderChain.RenderAndClear();
		}
	}
}

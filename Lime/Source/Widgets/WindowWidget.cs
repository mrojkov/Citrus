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

		public override void Update(float delta)
		{
			var context = WidgetContext.Current;
			WidgetInput.RemoveInvalidatedCaptures();
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			context.IsActiveTextWidgetUpdated = false;
			context.DistanceToNodeUnderCursor = float.MaxValue;
			base.Update(delta);
			if (!context.IsActiveTextWidgetUpdated) {
				context.ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.CurrentThread.IsMain()) {
				bool showKeyboard = context.ActiveTextWidget != null && context.ActiveTextWidget.Visible;
				if (prevActiveTextWidget != context.ActiveTextWidget) {
					Application.SoftKeyboard.Show(showKeyboard, context.ActiveTextWidget != null ? context.ActiveTextWidget.Text : "");
				}
#if ANDROID
				if (!Application.SoftKeyboard.Visible) {
					context.ActiveTextWidget = null;
				}
#endif
				// Handle switching between various text widgets
				if (prevActiveTextWidget != context.ActiveTextWidget && context.ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.SoftKeyboard.ChangeText(context.ActiveTextWidget.Text);
				}

				prevActiveTextWidget = context.ActiveTextWidget;
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

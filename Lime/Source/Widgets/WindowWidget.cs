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
		private IWidgetContext context;

		public WindowWidget(IWindow window)
			: this(new WidgetContext { Window = window })
		{
			(context as WidgetContext).Root = this;
		}

		public WindowWidget(IWidgetContext context)
		{
			this.context = context;
		}

		public override void Update(float delta)
		{
			var savedContext = WidgetContext.MakeCurrent(context);
			WidgetInput.RemoveInvalidatedCaptures();
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			context.IsActiveTextWidgetUpdated = false;
			context.DistanceToNodeUnderCursor = float.MaxValue;
			base.Update(delta);
			if (!context.IsActiveTextWidgetUpdated) {
				context.ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.IsMainThread) {
				bool showKeyboard = Context.ActiveTextWidget != null && Context.ActiveTextWidget.Visible;
				if (prevActiveTextWidget != Context.ActiveTextWidget) {
					Application.SoftKeyboard.Show(showKeyboard, Context.ActiveTextWidget != null ? Context.ActiveTextWidget.Text : "");
				}
#if ANDROID
				if (!Application.SoftKeyboard.Visible) {
					Context.ActiveTextWidget = null;
				}
#endif
				// Handle switching between various text widgets
				if (prevActiveTextWidget != Context.ActiveTextWidget && Context.ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.SoftKeyboard.ChangeText(Context.ActiveTextWidget.Text);
				}

				prevActiveTextWidget = Context.ActiveTextWidget;
			}
#endif
			WidgetContext.MakeCurrent(savedContext);
		}

		public override void Render()
		{
			var savedContext = WidgetContext.MakeCurrent(context);
			foreach (var node in Nodes) {
				node.AddToRenderChain(renderChain);
			}
			renderChain.RenderAndClear();
			WidgetContext.MakeCurrent(savedContext);
		}
	}
}

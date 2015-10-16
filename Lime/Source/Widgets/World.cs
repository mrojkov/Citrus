namespace Lime
{
	/// <summary>
	/// Виджет самого верхнего уровня (корень иерархии). Содержит все виджеты сцены
	/// </summary>
	public sealed class World : Frame
	{
		/// <summary>
		/// Виджет, который в данный момент содержит фокус ввода. Прежде чем обращаться к строке Input.TextInput, проверьте ActiveTextWidget == this.
		/// Для сброса фокуса ввода с виджета, обнулите ActiveTextWidget
		/// </summary>
		public IKeyboardInputProcessor ActiveTextWidget;

		/// <summary>
		/// В каждом цикле обновления ActiveTextWidget должен устанавливать этот флаг в true
		/// </summary>
		public bool IsActiveTextWidgetUpdated;

		internal float DistanceToNodeUnderCursor { get; set; }
		public Node NodeUnderCursor { get; internal set; }

		public static World Instance = new World();

#if iOS || ANDROID
		private IKeyboardInputProcessor prevActiveTextWidget;
#endif

		protected override void SelfUpdate(float delta)
		{
			DistanceToNodeUnderCursor = float.MaxValue;
			WidgetInput.RemoveInvalidatedCaptures();
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			IsActiveTextWidgetUpdated = false;
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (!IsActiveTextWidgetUpdated) {
				ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.IsMainThread) {
				bool showKeyboard = ActiveTextWidget != null && ActiveTextWidget.Visible;
				if (prevActiveTextWidget != ActiveTextWidget) {
					Application.Instance.SoftKeyboard.Show(showKeyboard, ActiveTextWidget != null ? ActiveTextWidget.Text : "");
				}
#if ANDROID
				if (!Application.Instance.SoftKeyboard.Visible) {
					ActiveTextWidget = null;
				}
#endif
				// Handle switching between various text widgets
				if (prevActiveTextWidget != ActiveTextWidget && ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.Instance.SoftKeyboard.ChangeText(ActiveTextWidget.Text);
				}

				prevActiveTextWidget = ActiveTextWidget;
			}
#endif
		}
	}
}

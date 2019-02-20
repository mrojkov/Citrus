using System;

namespace Lime
{
	public static class MouseWheelProcessor
	{
		public delegate void OnMouseWheelHandler(Vector2 position, float mouseDelta);

		private class Processor
		{
			public static readonly object Tag = new object();

			private readonly Widget target;
			private readonly OnMouseWheelHandler handler;

			public Processor(Widget widget, OnMouseWheelHandler onMouseWheel)
			{
				target = widget;
				handler = onMouseWheel;
			}

			public void Process()
			{
				if (
					target.Input.WasKeyPressed(Key.MouseWheelUp) ||
					target.Input.WasKeyPressed(Key.MouseWheelDown)
				) {
					handler(target.Input.MousePosition, target.Input.WheelScrollAmount);
				}
			}
		}

		public static void Attach(Widget target, OnMouseWheelHandler onMouseWheel)
		{
			if (target.Tasks.AnyTagged(Processor.Tag)) {
				var message = $"{nameof(target)} already contains {nameof(MouseWheelProcessor)}";
				throw new Exception(message);
			}
			if (onMouseWheel == null) {
				throw new NullReferenceException($"{nameof(onMouseWheel)} is null");
			}
			target.Tasks.AddLoop(new Processor(target, onMouseWheel).Process, Processor.Tag);
		}

		public static void Detach(Widget target)
		{
			target.Tasks.StopByTag(Processor.Tag);
		}
	}
}

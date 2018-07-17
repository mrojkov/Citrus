using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public delegate void KeyEventDelegate(WidgetInput input, Key key);

	public abstract class KeyHandler : ITaskProvider
	{
		readonly Key key;
		readonly KeyEventDelegate handler;

		public KeyHandler(KeyEventDelegate handler) { this.handler = handler; }
		public KeyHandler(Key key, KeyEventDelegate handler) { this.key = key; this.handler = handler; }

		public IEnumerator<object> Task()
		{
			while (true) {
				if (Window.Current.Input.Changed) {
					var widget = TaskList.Current.Node as Widget;
					if (widget != null) {
						var input = widget.Input;
						if (key != Key.Unknown) {
							if (WasKeyTriggered(input, key)) {
								OnKey(input, key);
							}
						} else {
							for (var key = Key.Unknown; key < Key.Count; key++) {
								if (WasKeyTriggered(input, key)) {
									OnKey(input, key);
								}
							}
						}
					}
				}
				yield return null;
			}
		}

		protected virtual void HandleInput() { }
		protected virtual void OnKey(WidgetInput input, Key key) => handler?.Invoke(input, key);
		protected abstract bool WasKeyTriggered(WidgetInput input, Key key);
	}

	public class KeyPressHandler : KeyHandler
	{
		public KeyPressHandler(KeyEventDelegate handler = null) : base(handler) { }
		public KeyPressHandler(Key key, KeyEventDelegate handler = null) : base(key, handler) { }
		protected override bool WasKeyTriggered(WidgetInput input, Key key) => input.WasKeyPressed(key);
	}

	public class KeyRepeatHandler : KeyHandler
	{
		public KeyRepeatHandler(KeyEventDelegate handler = null) : base(handler) { }
		public KeyRepeatHandler(Key key, KeyEventDelegate handler = null) : base(key, handler) { }
		protected override bool WasKeyTriggered(WidgetInput input, Key key) => input.WasKeyRepeated(key);
	}

	public class KeyReleaseHandler : KeyHandler
	{
		public KeyReleaseHandler(KeyEventDelegate handler = null) : base(handler) { }
		public KeyReleaseHandler(Key key, KeyEventDelegate handler = null) : base(key, handler) { }
		protected override bool WasKeyTriggered(WidgetInput input, Key key) => input.WasKeyReleased(key);
	}
}

using System;
using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	using StateFunc = Func<IEnumerator<int>>;

	[ProtoContract]
	public class Button : Widget
	{
		[ProtoMember(1)]
		public override string Text { get; set; }

		[ProtoMember(2)]
		public bool Enabled { get; set; }

		/// <summary>
		/// Indicates whether a button has draggable behaviour. 
		/// It means that if a user quickly swiped across the button it would not be pressed.
		/// </summary>
		[ProtoMember(3)]
		public bool Draggable { get; set; }

		public override Action Clicked { get; set; }
		
		private SimpleText textPresenter;
		private bool wasClicked;
		private StateMachine stateMachine = new StateMachine();
		private StateFunc State
		{
			get { return stateMachine.State; }
			set { stateMachine.SetState(value); }
		}

		public Button()
		{
			Enabled = true;
			State = NormalState;
		}

		public override bool WasClicked()
		{
			return wasClicked;
		}

		public override Node DeepCloneFast()
		{
			var clone = (Button)base.DeepCloneFast();
			clone.stateMachine = new StateMachine();
			clone.State = clone.NormalState;
			return clone;
		}

		private IEnumerator<int> NormalState()
		{
			if (World.Instance != null && TheActiveWidget == this) {
				TheActiveWidget = null;
			}
			TryRunAnimation("Normal");
			while (true) {
#if !iOS
				if (!Input.IsMousePressed()) {
					yield return 0;
					continue;
				}
#endif
				if (HitTest(Input.MousePosition) && TheActiveWidget == null) {
					State = FocusedState;
				}
				yield return 0;
			}
		}

		private static IEnumerable<int> TimeDelay(float secs)
		{
			var time = DateTime.Now;
			while ((DateTime.Now - time).TotalSeconds < secs) {
				yield return 0;
			}
		}

		private IEnumerator<int> FocusedState()
		{
			World.Instance.ActiveWidget = this;
			TryRunAnimation("Focus");
			while (true) {
				if (!HitTest(Input.MousePosition)) {
					State = NormalState;
				} else if (Input.WasKeyPressed(Key.Mouse0)) {
					if (Draggable) {
						State = DetectDraggingState;
					} else {
						State = PressedState;
					}
				}
				yield return 0;
			}
		}

		private IEnumerator<int> DetectDraggingState()
		{
			var mouse = Input.MousePosition;
			foreach (var t in TimeDelay(0.15f)) {
				yield return 0;
				if ((mouse - Input.MousePosition).Length > 5) {
					State = NormalState;
				} else if (Input.WasKeyReleased(Key.Mouse0) && HitTest(Input.MousePosition)) {
					HandleClick();
					State = ReleaseState;
				}
			}
			State = PressedState;
		}

		private IEnumerator<int> PressedState()
		{
			var mouse = Input.MousePosition;
			TryRunAnimation("Press");
			while (true) {
				if (Draggable && (mouse - Input.MousePosition).Length > 5) {
					State = ReleaseState;
				} else if (!HitTest(Input.MousePosition)) {
					State = ReleaseState;
				} else if (Input.WasKeyReleased(Key.Mouse0)) {
					HandleClick();
					State = ReleaseState;
				}
				yield return 0;
			}
		}

		private void HandleClick()
		{
			if (Clicked != null) {
				Clicked();
			}
			wasClicked = true;
		}

		private IEnumerator<int> ReleaseState()
		{
			if (TryRunAnimation("Release")) {
				while (IsRunning) {
					yield return 0;
				}
			}
			if (HitTest(Input.MousePosition)) {
				State = FocusedState;
			} else {
				State = NormalState;
			}
		}

		private IEnumerator<int> DisabledState()
		{
			if (TheActiveWidget == this) {
				TheActiveWidget = null;
			}
			TryRunAnimation("Disable");
			while (IsRunning) {
				yield return 0;
			}
			while (!Enabled) {
				yield return 0;
			}
			TryRunAnimation("Enable");
			while (IsRunning) {
				yield return 0;
			}
			State = NormalState;
		}

		private void UpdateLabel()
		{
			if (textPresenter == null) {
				TryFind<SimpleText>("TextPresenter", out textPresenter);
			}
			if (textPresenter != null) {
				textPresenter.Text = Text;
			}
		}

		public override void Update(int delta)
		{
			wasClicked = false;
			if (GloballyVisible) {
				stateMachine.Advance();
				UpdateLabel();
				SyncActiveWidget();
			}
			if (!Enabled && State != DisabledState) {
				State = DisabledState;
			}
			base.Update(delta);
		}

		void SyncActiveWidget()
		{
			if (!Enabled) {
				return;
			}
			if (World.Instance.ActiveWidget != this && State != NormalState) {
				if (CurrentAnimation != "Release") {
					State = NormalState;
				}
			}
			if (TheActiveWidget == this) {
				World.Instance.IsActiveWidgetUpdated = true;
			}
		}

		private static Widget TheActiveWidget
		{
			get { return World.Instance.ActiveWidget; }
			set { World.Instance.ActiveWidget = value; }
		}

#region StateMachine
		class StateMachine
		{
			private IEnumerator<int> stateHandler;
			public StateFunc State { get; private set; }

			public void SetState(StateFunc state)
			{
				State = state;
				stateHandler = state();
				stateHandler.MoveNext();
			}

			public void Advance()
			{
				stateHandler.MoveNext();
			}
		}
#endregion
	}
}

using System;
using ProtoBuf;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	using StateFunc = Func<IEnumerator<int>>;
	using System.Diagnostics;

	[ProtoContract]
	public class Button : Widget
	{
		[ProtoMember(1)]
		public override string Text { get; set; }

		[ProtoMember(2)]
        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

		/// <summary>
		/// Indicates whether a button has draggable behavior. 
		/// It means that if a user has quickly passed his finger through the button it would not be pressed.
		/// </summary>
		[ProtoMember(3)]
		public bool Draggable { get; set; }

		public override Action Clicked { get; set; }
		
		private List<Widget> textPresenters;
		private bool wasClicked;
		private StateMachine stateMachine = new StateMachine();
		private StateFunc State
		{
			get { return stateMachine.State; }
			set { stateMachine.SetState(value); }
		}

		/// <summary>
		/// The minimum distance which finger should pass through the button,
		/// in order to avoid button click. 
		/// For Draggable buttons only.
		/// </summary>
		private const float DragDistanceThreshold = 15;

		/// <summary>
		/// The period of time while drag detection is working, 
		/// since a finger touched the button.
		/// For Draggable buttons only.
		/// </summary>
		private const float DragDetectionTime = 0.15f;

		/// <summary>
		/// Once button has been clicked, its active area includes circle with a given radius.
		/// </summary>
		public static float ButtonEffectiveRadius = 200;

		/// <summary>
		/// Indicates whether all buttons should use tablet control scheme that doesn't includes
		/// 'focused' state support, but behaves better when multiple buttons overlap each other.
		/// </summary>
#if iOS
		public static bool TabletControlScheme = true;
#else
		public static bool TabletControlScheme = false;
#endif

		public Button()
		{
			HitTestMask = ControlsHitTestMask;
			Enabled = true;
			// On the current frame the button contents may not be loaded, 
			// so delay its initialization until the next frame.
			State = InitialState;
		}

		private IEnumerator<int> InitialState()
		{
			yield return 0;
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
			Input.ReleaseMouse();
			TryRunAnimation("Normal");
			while (true) {
				if (TabletControlScheme) {
					if (Input.WasMousePressed() && IsMouseOver()) {
						if (Draggable) {
							State = DetectDraggingState;
						} else {
							State = PressedState;
						}
					}
				} else {
					if (IsMouseOver()) {
						State = FocusedState;
					}
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

		// Used only in the desktop control scheme
		private IEnumerator<int> FocusedState()
		{
			TryRunAnimation("Focus");
			while (true) {
				if (!IsMouseOver()) {
					State = NormalState;
				} else if (Input.WasMousePressed()) {
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
			foreach (var t in TimeDelay(DragDetectionTime)) {
				yield return 0;
				if ((mouse - Input.MousePosition).Length > DragDistanceThreshold) {
					State = NormalState;
				} else if (!Input.IsMousePressed() && IsMouseOver()) {
					State = QuickClickOnDraggableButtonState;
					yield break;
				}
			}
			State = PressedState;
		}

		private IEnumerator<int> QuickClickOnDraggableButtonState()
		{
			Input.CaptureMouse();
			if (TryRunAnimation("Press")) {
				while (IsRunning) {
					yield return 0;
				}
			}
			HandleClick();
			State = ReleaseState;
		}

		private IEnumerator<int> PressedState()
		{
			Input.CaptureMouse();
			var mouse = Input.MousePosition;
			TryRunAnimation("Press");
			bool wasPressed = true;
			while (true) {
				if (!Input.IsMouseOwner()) {
					State = ReleaseState;
				}
				bool isPressed = IsMouseOver() ||
					(Input.MousePosition - this.GlobalCenter).Length < ButtonEffectiveRadius;
				if (!Input.IsMousePressed()) {
					if (isPressed) {
						HandleClick();
					}
					State = ReleaseState;
				} else if (wasPressed && !isPressed) {
					TryRunAnimation("Release");
				} else if (!wasPressed && isPressed) {
					TryRunAnimation("Press");
				}
				yield return 0;
				wasPressed = isPressed;
			}
		}

		private void HandleClick()
		{
			if (Clicked != null) {
#if !iOS
				if (Debug.BreakOnButtonClick) {
					Debugger.Break();
				}
#endif
				Clicked();
			}
			wasClicked = true;
		}

		private IEnumerator<int> ReleaseState()
		{
			Input.ReleaseMouse();
			if (CurrentAnimation != "Release") {
				if (TryRunAnimation("Release")) {
					while (IsRunning) {
						yield return 0;
					}
				}
			}
			if (TabletControlScheme) {
				State = NormalState;
			} else {
				if (IsMouseOver()) {
					State = FocusedState;
				} else {
					State = NormalState;
				}
			}
		}

		private IEnumerator<int> DisabledState()
		{
			Input.ReleaseMouse();
			if (CurrentAnimation == "Release") {
				// The release animation should be played if we disable the button 
				// right after click on it.
				while (IsRunning) {
					yield return 0;
				}
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
			if (textPresenters == null) {
				textPresenters = new List<Widget>();
				textPresenters.AddRange(Descendants.OfType<Widget>().Where(i => i.Id == "TextPresenter"));
			}
			foreach (var i in textPresenters) {
				i.Text = Text;
			}
		}

		protected override void SelfUpdate(float delta)
		{
			wasClicked = false;
			if (GloballyVisible) {
				stateMachine.Advance();
				UpdateLabel();
			}
			if (!Enabled && State != DisabledState) {
				State = DisabledState;
			}
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

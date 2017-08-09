using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	using StateFunc = Func<IEnumerator<int>>;
	using System.Diagnostics;

	[AllowedChildrenTypes(typeof(Node))]
	public class Button : Widget
	{
		public BitSet32 EnableMask = BitSet32.Full;

		/// <summary>
		/// Текст кнопки
		/// </summary>
		[YuzuMember]
		public override string Text { get; set; }

		/// <summary>
		/// Кнопка включена и принимает ввод от пользователя
		/// </summary>
		[YuzuMember]
		public bool Enabled {
			get { return EnableMask[0]; }
			set { EnableMask[0] = value; }
		}

		/// <summary>
		/// Для реализации кнопки, которая находится в элементе, допускающим скроллинг (например в скроллящемся списке)
		/// Если пользователь нажмет кнопку, и не отпуская палец быстро передвинет его, кнопка не нажмется
		/// </summary>
		[YuzuMember]
		public bool Draggable { get; set; }

		/// <summary>
		/// Генерируется при нажатии кнопки
		/// </summary>
		public override Action Clicked { get; set; }

		private bool wasClicked;
		private bool skipReleaseAnimation;
		private TextPresentersFeeder textPresentersFeeder;
		private StateMachine stateMachine;
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
		/// Indicates whether all buttons should use tablet control scheme that doesn't includes
		/// 'focused' state support, but behaves better when multiple buttons overlap each other.
		///
		/// Означает, что поведение всех кнопок подстроено под ввод с сенсорного экрана.
		/// </summary>
#if iOS || ANDROID
		public static bool TabletControlScheme = true;
#else
		public static bool TabletControlScheme = false;
#endif

		public Button()
		{
			HitTestTarget = true;
			Input.AcceptMouseBeyondWidget = false;
		}

		private IEnumerator<int> InitialState()
		{
			yield return 0;
			State = NormalState;
		}

		/// <summary>
		/// Возвращает true, если кнопка нажата
		/// </summary>
		public override bool WasClicked()
		{
			// If the button is hidden by clicking on it, it might became WasClicked forever.
			return wasClicked && GloballyVisible;
		}

		protected override void Awake()
		{
			stateMachine = new StateMachine();
			// On the current frame the button contents may not be loaded,
			// so delay its initialization until the next frame.
			State = InitialState;
			textPresentersFeeder = new TextPresentersFeeder(this);
		}

		private IEnumerator<int> NormalState()
		{
			skipReleaseAnimation = false;
			Input.ReleaseMouse();
			TryRunAnimation("Normal");
			while (true) {
				if (TabletControlScheme) {
					if (IsMouseOver() && Input.WasMousePressed()) {
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
				} else if (
					// Added checking if mouse over because we need to make sure this button or no one is mouse owner
					// since button may be inside list view which took control of a mouse.
					!Input.IsMousePressed() && IsMouseOver()
				) {
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
			TryRunAnimation("Press");
			bool wasPressed = true;
			while (true) {
				if (!Input.IsMouseOwner()) {
					State = ReleaseState;
				}
				bool isPressed = IsMouseOverThisOrDescendant();
				if (!Input.IsMousePressed()) {
					if (isPressed) {
						HandleClick();
						if (!GloballyVisible) {
							// buz: don't play release animation
							// if button's parent became invisible due to
							// button press (or it will be played when
							// parent is visible again)
							skipReleaseAnimation = true;
						}
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

		protected virtual void HandleClick()
		{
			// Release mouse for case we are showing a modal native dialog.
			Input.ReleaseMouse();
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
			if (CurrentAnimation != "Release" && !skipReleaseAnimation) {
				if (TryRunAnimation("Release")) {
					while (IsRunning) {
						yield return 0;
					}
				}
			}
			skipReleaseAnimation = false;
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
			while (!EnableMask.All()) {
				yield return 0;
			}
			TryRunAnimation("Enable");
			while (IsRunning) {
				yield return 0;
			}
			State = NormalState;
		}

		protected override void SelfUpdate(float delta)
		{
			wasClicked = false;
			if (GloballyVisible) {
				stateMachine.Advance();
				textPresentersFeeder.Update();
			}
			if (!EnableMask.All() && State != DisabledState) {
				State = DisabledState;
			}
			if (Enabled) {
				if (Input.ConsumeKeyPress(Key.Space) || Input.ConsumeKeyPress(Key.Enter)) {
					HandleClick();
				}
			}
		}

#region StateMachine
		class StateMachine
		{
			private IEnumerator<int> stateHandler;
			public StateFunc State { get; private set; }
			private bool isChanging;

			public void SetState(StateFunc state)
			{
				State = state;
				stateHandler = state();
				if (!isChanging) {
					isChanging = true;
					stateHandler.MoveNext();
					isChanging = false;
				}
			}

			public void Advance()
			{
				stateHandler.MoveNext();
			}
		}
#endregion
	}

	internal class TextPresentersFeeder
	{
		private Widget widget;
		private List<Widget> textPresenters;

		public TextPresentersFeeder(Widget widget)
		{
			this.widget = widget;
		}

		public void Update()
		{
			if (textPresenters == null) {
				textPresenters = new List<Widget>();
				textPresenters.AddRange(widget.Descendants.OfType<Widget>().Where(i => i.Id == "TextPresenter"));
			}
			foreach (var i in textPresenters) {
				i.Text = widget.Text;
			}
		}
	}
}

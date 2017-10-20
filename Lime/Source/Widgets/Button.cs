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

		[YuzuMember]
		public override string Text { get; set; }

		[YuzuMember]
		public bool Enabled {
			get { return EnableMask[0]; }
			set { EnableMask[0] = value; }
		}

		public override Action Clicked { get; set; }

		private TextPresentersFeeder textPresentersFeeder;
		private StateMachine stateMachine;
		private StateFunc State
		{
			get { return stateMachine.State; }
			set { stateMachine.SetState(value); }
		}

		private ClickRecognizer clickRecognizer;

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

		public override bool WasClicked() => clickRecognizer?.WasRecognized() ?? false;

		protected override void Awake()
		{
			stateMachine = new StateMachine();
			// On the current frame the button contents may not be loaded,
			// so delay its initialization until the next frame.
			State = InitialState;
			textPresentersFeeder = new TextPresentersFeeder(this);
			clickRecognizer = new ClickRecognizer();
			GestureRecognizers.Add(clickRecognizer);
		}

		private IEnumerator<int> NormalState()
		{
			TryRunAnimation("Normal");
			while (true) {
#if WIN || MAC
				if (IsMouseOverThisOrDescendant()) {
					State = HoveredState;
				}
#else
				if (clickRecognizer.WasBegan()) {
					State = PressedState;
				}
#endif
				yield return 0;
			}
		}

		private IEnumerator<int> HoveredState()
		{
			TryRunAnimation("Focus");
			while (true) {
				if (!IsMouseOverThisOrDescendant()) {
					State = NormalState;
				} else if (clickRecognizer.WasBegan()) {
					State = PressedState;
				}
				yield return 0;
			}
		}

		private IEnumerator<int> PressedState()
		{
			TryRunAnimation("Press");
			var wasMouseOver = true;
			while (true) {
				if (clickRecognizer.WasRecognized()) {
					while (IsRunning) {
						yield return 0;
					}
					HandleClick();
					// buz: don't play release animation
					// if button's parent became invisible due to
					// button press (or it will be played when
					// parent is visible again)
					if (!GloballyVisible) {
#if WIN || MAC
						State = HoveredState;
#else
						State = NormalState;
#endif
					} else {
						State = ReleaseState;
					}
				} else if (clickRecognizer.WasCanceled()) {
					State = NormalState;
				}
				var mouseOver = IsMouseOverThisOrDescendant();
				if (wasMouseOver && !mouseOver) {
					TryRunAnimation("Release");
				} else if (!wasMouseOver && mouseOver) {
					TryRunAnimation("Press");
				}
				wasMouseOver = mouseOver;
				yield return 0;
			}
		}

		protected virtual void HandleClick()
		{
			if (Clicked != null) {
#if !iOS
				if (Debug.BreakOnButtonClick) {
					Debugger.Break();
				}
#endif
				Clicked();
			}
		}

		private IEnumerator<int> ReleaseState()
		{
			if (CurrentAnimation != "Release") {
				if (TryRunAnimation("Release")) {
					while (IsRunning) {
						yield return 0;
					}
				}
			}
#if WIN || MAC
			State = HoveredState;
#else
			State = NormalState;
#endif
		}

		private IEnumerator<int> DisabledState()
		{
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
			if (GloballyVisible) {
				stateMachine.Advance();
				textPresentersFeeder.Update();
			}
			if (!EnableMask.All() && State != DisabledState) {
				State = DisabledState;
			}
#if WIN || MAC
			if (Enabled) {
				if (Input.ConsumeKeyPress(Key.Space) || Input.ConsumeKeyPress(Key.Enter)) {
					HandleClick();
				}
			}
#endif
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
			textPresenters = textPresenters ?? widget.Descendants.OfType<Widget>().Where(i => i.Id == "TextPresenter").ToList();
			foreach (var i in textPresenters) {
				i.Text = widget.Text;
			}
		}
	}
}

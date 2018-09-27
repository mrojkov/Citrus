using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 1)]
	[TangerineNodeBuilder("BuildForTangerine")]
	[TangerineAllowedChildrenTypes(typeof(Node))]
	[TangerineVisualHintGroup("/All/Nodes/Containers")]
	public class Button : Widget
	{
		private TextPresentersFeeder textPresentersFeeder;
		private IEnumerator<int> stateHandler;
		private ClickGesture clickGesture;
		private bool isChangingState;
		private bool isDisabledState;

		public BitSet32 EnableMask = BitSet32.Full;

		[YuzuMember]
		[TangerineKeyframeColor(9)]
		public override string Text { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(18)]
		public bool Enabled
		{
			get { return EnableMask[0]; }
			set { EnableMask[0] = value; }
		}

		public override Action Clicked { get; set; }

		public Button()
		{
			HitTestTarget = true;
			Input.AcceptMouseBeyondWidget = false;
			Awoke += Awake;
		}

		private void SetState(IEnumerator<int> newState)
		{
			stateHandler = newState;
			if (!isChangingState) {
				isChangingState = true;
				stateHandler.MoveNext();
				isChangingState = false;
			}
		}

		private IEnumerator<int> InitialState()
		{
			yield return 0;
			SetState(NormalState());
		}

		public override bool WasClicked() => clickGesture?.WasRecognized() ?? false;

		private static void Awake(Node owner)
		{
			// On the current frame the button contents may not be loaded,
			// so delay its initialization until the next frame.
			var button = (Button)owner;
			button.SetState(button.InitialState());
			button.textPresentersFeeder = new TextPresentersFeeder(button);
			button.clickGesture = new ClickGesture();
			button.Gestures.Add(button.clickGesture);
		}

		private IEnumerator<int> NormalState()
		{
			TryRunAnimation("Normal");
			while (true) {
#if WIN || MAC
				if (IsMouseOverThisOrDescendant()) {
					SetState(HoveredState());
				}
#else
				if (clickGesture.WasBegan()) {
					SetState(PressedState());
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
					SetState(NormalState());
				} else if (clickGesture.WasBegan()) {
					SetState(PressedState());
				}
				yield return 0;
			}
		}

		private IEnumerator<int> PressedState()
		{
			TryRunAnimation("Press");
			var wasMouseOver = true;
			while (true) {
				if (clickGesture.WasRecognized()) {
					Clicked?.Invoke();
					// buz: don't play release animation
					// if button's parent became invisible due to
					// button press (or it will be played when
					// parent is visible again)
					if (!GloballyVisible) {
#if WIN || MAC
						SetState(HoveredState());
#else
						SetState(NormalState());
#endif
					} else {
						SetState(ReleaseState());
					}
				} else if (clickGesture.WasCanceled()) {
					if (CurrentAnimation == "Press") {
						TryRunAnimation("Release");
						while (DefaultAnimation.IsRunning) {
							yield return 0;
						}
					}
					SetState(NormalState());
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

		private IEnumerator<int> ReleaseState()
		{
			if (CurrentAnimation != "Release") {
				if (TryRunAnimation("Release")) {
					while (DefaultAnimation.IsRunning) {
						yield return 0;
					}
				}
			}
#if WIN || MAC
			SetState(HoveredState());
#else
			SetState(NormalState());
#endif
		}

		private IEnumerator<int> DisabledState()
		{
			isDisabledState = true;
			if (CurrentAnimation == "Release") {
				// The release animation should be played if we disable the button
				// right after click on it.
				while (DefaultAnimation.IsRunning) {
					yield return 0;
				}
			}
			TryRunAnimation("Disable");
			while (DefaultAnimation.IsRunning) {
				yield return 0;
			}
			while (!EnableMask.All()) {
				yield return 0;
			}
			TryRunAnimation("Enable");
			while (DefaultAnimation.IsRunning) {
				yield return 0;
			}
			isDisabledState = false;
			SetState(NormalState());
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			if (GloballyVisible) {
				stateHandler.MoveNext();
				textPresentersFeeder.Update();
			}
			if (!EnableMask.All() && !isDisabledState) {
				SetState(DisabledState());
			}
#if WIN || MAC
			if (Enabled) {
				if (Input.ConsumeKeyPress(Key.Space) || Input.ConsumeKeyPress(Key.Enter)) {
					Clicked?.Invoke();
				}
			}
#endif
		}

		private void BuildForTangerine()
		{
			int[] markerFrames = { 0, 10, 20, 30, 40};
			string[] makerIds = { "Normal", "Focus", "Press", "Release", "Disable" };
			for (var i = 0; i < 5; i++) {
				DefaultAnimation.Markers.Add(new Marker(makerIds[i], markerFrames[i], MarkerAction.Stop));
			}
		}
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

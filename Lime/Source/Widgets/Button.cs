using System;
using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	public delegate void ButtonClickEventHandler(Button button);

	[ProtoContract]
	public class Button : Widget
	{
		[ProtoMember(1)]
		public string Caption { get; set; }

        [ProtoMember(2)]
        public bool Enabled { get; set; }

		public ButtonClickEventHandler Clicked;

		private SimpleText textPresenter;
		private BareEventHandler StateHandler;

        public Button()
        {
            Enabled = true;
			SetNormalState();
			// Run animation on the next frame (when button contents will be loaded)
			StateHandler = UpdateInitialState;
        }

		void UpdateInitialState()
		{
			SetNormalState();
		}

		void SetNormalState()
		{
			if (World.Instance != null && World.Instance.ActiveWidget == this) {
				World.Instance.ActiveWidget = null;
			}
			TryRunAnimation("Normal");
			StateHandler = UpdateNormalState;
		}

		void UpdateNormalState()
		{
			if (HitTest(Input.MousePosition) && World.Instance.ActiveWidget == null) {
				SetFocusedState();
			}
		}

		void SetFocusedState()
		{
			World.Instance.ActiveWidget = this;
			TryRunAnimation("Focus");
			StateHandler = UpdateFocusedState;
			UpdateFocusedState();
		}

		void UpdateFocusedState()
		{
#if iOS
			if (!HitTest(Input.MousePosition) || !Input.IsMousePressed()) {
#else
			if (!HitTest(Input.MousePosition)) {
#endif
				SetNormalState();
			} else if (Input.WasKeyPressed(Key.Mouse0)) {
				SetPressedState();
			}
		}

		void SetPressedState()
		{
			TryRunAnimation("Press");
			StateHandler = UpdatePressedState;
			UpdatePressedState();
		}

		void UpdatePressedState()
		{
			if (!HitTest(Input.MousePosition)) {
                RunReleaseAnimation();
				//RunAnimationWithStopHandler("Release", () => SetNormalState());
			} else if (Input.WasKeyReleased(Key.Mouse0)) {
				if (Clicked != null) {
					Clicked(this);
				}
                RunReleaseAnimation();
				//RunAnimationWithStopHandler("Release", () => SetNormalState());
			}
		}

		void SetDisabledState()
		{
			if (World.Instance.ActiveWidget == this) {
				World.Instance.ActiveWidget = null;
			}
			TryRunAnimation("Disable");
			StateHandler = UpdateDisabledState;
		}

		void UpdateDisabledState()
		{
			if (Enabled) {
				RunAnimationWithStopHandler("Enable", () => SetNormalState());
			}
		}

		void RunAnimationWithStopHandler(string name, BareEventHandler onStop)
		{
			if (TryRunAnimation(name)) {
				StateHandler = () => {
					if (IsStopped) {
						onStop();
					}
				};
			} else {
				onStop();
			}
		}

		private void UpdateLabel()
		{
			if (textPresenter == null) {
				textPresenter = TryFind<SimpleText>("TextPresenter");
			}
			if (textPresenter != null) {
				textPresenter.Text = Caption;
			}
		}

		private void RunReleaseAnimation()
		{
            RunAnimationWithStopHandler(
                "Release", 
                () => {
                    if (HitTest(Input.MousePosition))
                        SetFocusedState();
                    else
                        SetNormalState();
                }
            );
		}

		public override void Update(int delta)
		{
			if (GloballyVisible) {
				StateHandler();
				UpdateLabel();
				SyncActiveWidget();
			}
			if (!Enabled && StateHandler != UpdateDisabledState) {
				SetDisabledState();
			}
			base.Update(delta);
		}

		void SyncActiveWidget()
		{
			if (Enabled) {
				if (World.Instance.ActiveWidget != this && StateHandler != UpdateNormalState) {
					SetNormalState();
				}
			    if (World.Instance.ActiveWidget == this) {
				    World.Instance.IsActiveWidgetUpdated = true;
			    }			
			}
		}
	}
}

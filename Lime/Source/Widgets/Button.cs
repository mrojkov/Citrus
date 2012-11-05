using System;
using ProtoBuf;

namespace Lime
{
	public delegate void ButtonClickEvent(Button button);

	[ProtoContract]
	public class Button : Widget
	{
		SimpleText textPresenter;
        bool isAlreadyDisabled;

		[ProtoMember(1)]
		public string Caption { get; set; }

        [ProtoMember(2)]
        public bool Enabled { get; set; }

		public ButtonClickEvent Clicked;

        public Button()
            : base()
        {
            Enabled = true;
            isAlreadyDisabled = false;
        }

		void UpdateHelper(int delta)
		{
			if (textPresenter == null) {
				textPresenter = TryFind<SimpleText>("TextPresenter");
			}
			if (textPresenter != null) {
				textPresenter.Text = Caption;
			}
            if (Enabled) {
                isAlreadyDisabled = false;
                if (HitTest(Input.MousePosition)) {
                    if (World.Instance.ActiveWidget == null) {
                        TryRunAnimation("Focus");
                        World.Instance.ActiveWidget = this;
                    }
                } else {
                    if (World.Instance.ActiveWidget == this) {
                        if (isAlreadyDisabled)
                            TryRunAnimation("Enable");
                        else
                            TryRunAnimation("Normal");
                        World.Instance.ActiveWidget = null;
                    }
                }
                if (World.Instance.ActiveWidget == this) {
                    if (Input.WasKeyPressed(Key.Mouse0)) {
                        TryRunAnimation("Press");
                        Input.ConsumeKeyEvent(Key.Mouse0, true);
                    }
                    if (Input.WasKeyReleased(Key.Mouse0)) {
                        if (HitTest(Input.MousePosition))
                            TryRunAnimation("Focus");
                        else
                            TryRunAnimation("Normal");
                        Input.ConsumeKeyEvent(Key.Mouse0, true);
                        if (Clicked != null) {
                            Clicked(this);
                        }
                    }
                }
                if (World.Instance.ActiveWidget != this && CurrentAnimation != "Normal") {
                    TryRunAnimation("Normal");
                }
			    if (World.Instance.ActiveWidget == this) {
				    World.Instance.IsActiveWidgetUpdated = true;
			    }
            } else {
                if (!isAlreadyDisabled)
                    TryRunAnimation("Disable");
                isAlreadyDisabled = true;
            }
        }

		public override void Update(int delta)
		{
			if (globallyVisible) {
				UpdateHelper(delta);
			}
			base.Update(delta);
		}
	}
}

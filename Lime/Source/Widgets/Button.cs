using System;
using ProtoBuf;

namespace Lime
{
	public delegate void ButtonClickEvent(Button button);

	[ProtoContract]
	public class Button : Widget
	{
		SimpleText textPresenter;

		[ProtoMember(1)]
		public string Caption { get; set; }

		public ButtonClickEvent Clicked;

		void UpdateHelper(int delta)
		{
			if (textPresenter == null) {
				textPresenter = Find<SimpleText>("TextPresenter", false);
			}
			if (textPresenter != null) {
				textPresenter.Text = Caption;
			}
			if (HitTest(Input.MousePosition)) {
				if (World.Instance.ActiveWidget == null) {
					RunAnimation("Focus");
					World.Instance.ActiveWidget = this;
				}
			} else {
				if (World.Instance.ActiveWidget == this) {
					RunAnimation("Normal");
					World.Instance.ActiveWidget = null;
				}
			}
			if (World.Instance.ActiveWidget == this) {
				if (Input.WasKeyPressed(Key.Mouse0)) {
					RunAnimation("Press");
					Input.ConsumeKeyEvent(Key.Mouse0, true);
				}
				if (Input.WasKeyReleased(Key.Mouse0)) {
					if (HitTest(Input.MousePosition))
						RunAnimation("Focus");
					else
						RunAnimation("Normal");
					Input.ConsumeKeyEvent(Key.Mouse0, true);
					if (Clicked != null) {
						Clicked(this);
					}
				}
			}
			if (World.Instance.ActiveWidget != this && CurrentAnimation != "Normal") {
				RunAnimation("Normal");
			}
			if (World.Instance.ActiveWidget == this) {
				World.Instance.ActiveWidgetUpdated = true;
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

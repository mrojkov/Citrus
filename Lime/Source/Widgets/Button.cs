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

		public ButtonClickEvent OnClick;

		void UpdateHelper(int delta)
		{
			if (textPresenter == null) {
				textPresenter = Find<SimpleText>("TextPresenter", false);
			}
			if (textPresenter != null) {
				textPresenter.Text = Caption;
			}
			if (HitTest(Input.MousePosition)) {
				if (RootFrame.Instance.ActiveWidget == null) {
					RunAnimation("Focus");
					RootFrame.Instance.ActiveWidget = this;
				}
			} else {
				if (RootFrame.Instance.ActiveWidget == this) {
					RunAnimation("Normal");
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (RootFrame.Instance.ActiveWidget == this) {
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
					if (OnClick != null) {
						OnClick(this);
					}
				}
			}
			if (RootFrame.Instance.ActiveWidget != this && CurrentAnimation != "Normal") {
				RunAnimation("Normal");
			}
			if (RootFrame.Instance.ActiveWidget == this) {
				RootFrame.Instance.ActiveWidgetUpdated = true;
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

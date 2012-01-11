using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Button : Widget
	{
		SimpleText textPresenter;

		[ProtoMember(1)]
		public string Caption { get; set; }

		public event EventHandler<EventArgs> OnClick;

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
					PlayAnimation("Focus");
					RootFrame.Instance.ActiveWidget = this;
				}
			} else {
				if (RootFrame.Instance.ActiveWidget == this) {
					PlayAnimation("Normal");
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (RootFrame.Instance.ActiveWidget == this) {
				if (Input.GetKeyDown(Key.Mouse0)) {
					PlayAnimation("Press");
					Input.ConsumeKeyEvent(Key.Mouse0, true);
				}
				if (Input.GetKeyUp(Key.Mouse0)) {
					if (HitTest(Input.MousePosition))
						PlayAnimation("Focus");
					else
						PlayAnimation("Normal");
					Input.ConsumeKeyEvent(Key.Mouse0, true);
					if (OnClick != null) {
						OnClick(this, null);
					}
				}
			}
			if (RootFrame.Instance.ActiveWidget != this && CurrentAnimation != "Normal") {
				PlayAnimation("Normal");
			}
		}

		public override void Update(int delta)
		{
			if (worldShown) {
				UpdateHelper(delta);
			}
			base.Update(delta);
		}
	}
}

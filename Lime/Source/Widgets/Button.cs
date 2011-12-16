using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Button : GUIWidget
	{
		SimpleText textPresenter;

		[ProtoMember(1)]
		public string Caption { get; set; }

		public event EventHandler<EventArgs> OnClick;

		protected override void Reset ()
		{
			PlayAnimation ("Normal");
		}

		public override void UpdateGUI ()
		{
			base.UpdateGUI ();
			if (HitTest (Input.MousePosition)) {
				if (GUIWidget.FocusedWidget == null) {
					PlayAnimation ("Focus");
					GUIWidget.FocusedWidget = this;
				}
			} else {
				if (GUIWidget.FocusedWidget == this) {
					PlayAnimation ("Normal");
					GUIWidget.FocusedWidget = null;
				}
			}
			if (GUIWidget.FocusedWidget == this) {
				if (Input.GetKeyDown (Key.Mouse0)) {
					PlayAnimation ("Press");
					Input.ConsumeKeyEvent (Key.Mouse0, true);
				}
				if (Input.GetKeyUp (Key.Mouse0)) {
					if (HitTest (Input.MousePosition))
						PlayAnimation ("Focus");
					else
						PlayAnimation ("Normal");
					Input.ConsumeKeyEvent (Key.Mouse0, true);
					if (OnClick != null) {
						OnClick (this, null);
					}
				}
			}
		}

		public override void Update (int delta)
		{
			if (textPresenter == null) {
				textPresenter = Find<SimpleText> ("TextPresenter", false);
			}
			if (textPresenter != null) {
				textPresenter.Text = Caption;
			}
			base.Update (delta);
		}
	}
}

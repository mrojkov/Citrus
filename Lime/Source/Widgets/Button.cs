using System;
using ProtoBuf;

namespace Lime
{
	public class WidgetEventArgs : EventArgs
	{
	}

    [ProtoContract]
	public class Button : Frame
	{
        [ProtoMember(1)]
		public string Caption { get; set; }

		public Button ()
		{
			AcceptInput = true;
			this.LeftDown += new EventHandler<UIEventArgs> (Button_LeftDown);
			this.Move += new EventHandler<UIEventArgs> (Button_Move);
		}

		public override void Render ()
		{
			base.Render ();
		}

		void Button_Move (object sender, UIEventArgs e)
		{
			if (HitTest (e.Pointer)) {
				if (UICoordinator.ActiveWidget != this) {
					PlayAnimation ("Focus");
					UICoordinator.ActiveWidget = this;
				}
			} else {
				if (UICoordinator.ActiveWidget == this) {
					PlayAnimation ("Normal");
					UICoordinator.ActiveWidget = null;
				}
			}
		}

		void Button_LeftDown (object sender, UIEventArgs e)
		{
			if (UICoordinator.ActiveWidget == this) {
				PlayAnimation ("Press");
			}
		}

		void Button_LeftUp (object sender, UIEventArgs e)
		{
			if (UICoordinator.ActiveWidget == this) {
				PlayAnimation ("Normal");
			}
		}

		public event EventHandler<WidgetEventArgs> Click;
	}
}

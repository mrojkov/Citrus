using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Lemon
{
    public enum UIEventType
    {
        LeftDown,
        LeftUp,
        Move,
    }

    public class UIEventArgs : EventArgs
    {
        public UIEventType Type;
        public Vector2 Pointer;
    }

    public class UICoordinator
    {
        Queue<UIEventArgs> Events = new Queue<UIEventArgs>();

        static readonly UICoordinator instance = new UICoordinator();

        public static Widget ActiveWidget;

        /// <summary>
        /// Global singleton.
        /// </summary>
        public static UICoordinator Instance {
			get { return instance; }
		}

        public void DispatchEvents (Node node)
		{
			while (Events.Count > 0) {
				UIEventArgs e = Events.Dequeue ();
				node.Widget.DispatchUIEvent (e);
			}
		}
/*
#if !MONOTOUCH				
        public void SetupEventHandling(OpenTK.GameWindow gameWindow)
        {
            gameWindow.Mouse.ButtonDown += Mouse_ButtonDown;
            gameWindow.Mouse.ButtonUp += Mouse_ButtonUp;
            gameWindow.Mouse.Move += Mouse_Move;
        }

		void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            Vector2 p = new Vector2(e.X, e.Y);
            Events.Enqueue(new UIEventArgs { Type = UIEventType.Move, Pointer = p });
        }

        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            Vector2 p = new Vector2(e.X, e.Y);
            Events.Enqueue(new UIEventArgs { Type = UIEventType.LeftUp, Pointer = p });
        }

        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            Vector2 p = new Vector2(e.X, e.Y);
            Events.Enqueue(new UIEventArgs { Type = UIEventType.LeftDown, Pointer = p });
        }
#endif
*/
    }
}

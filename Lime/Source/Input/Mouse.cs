using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Mouse
	{
		public bool Connected { get { return OpenTK.Input.Mouse.GetState ().IsConnected; } }
		public bool LeftDown { get { return OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Left); } }
		public bool LeftUp { get { return !LeftDown; } }
		public bool RightDown { get { return OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Right); } }
		public bool RightUp { get { return !RightDown; } }
		public Vector2 Position { get { return position; } }
		internal void SetPosition (Vector2 position) { this.position = position; }
		Vector2 position;
	}
}

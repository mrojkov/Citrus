using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Mouse
	{
#if WIN
		public bool Connected { get { return OpenTK.Input.Mouse.GetState ().IsConnected; } }
		public bool LeftDown { get { return OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Left); } }
		public bool LeftUp { get { return !LeftDown; } }
		public bool RightDown { get { return OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Right); } }
		public bool RightUp { get { return !RightDown; } }
		public Vector2 Position { get { return position; } }
		internal void SetPosition (Vector2 position) { this.position = position; }
		Vector2 position;
#elif iOS
		bool touching;
		public bool LeftDown { get { return touching; } }
		public bool LeftUp { get { return !touching; } }
		public bool RightDown { get { return false; } }
		public bool RightUp { get { return !RightDown; } }
		internal void SetTouching (bool value) { touching = value; }
		
		Vector2 position;
		public Vector2 Position { get { return position; } }
		internal void SetPosition (Vector2 position) { this.position = position; }
#endif
	}
}

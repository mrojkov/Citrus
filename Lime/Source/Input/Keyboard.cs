using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Keyboard
	{
#if WIN
		public bool Connected 
		{
			get {
				return OpenTK.Input.Keyboard.GetState (0).IsConnected;
			}
		}

		public bool this [Key key]
		{
			get {
				return OpenTK.Input.Keyboard.GetState (0).IsKeyDown ((OpenTK.Input.Key)key);
			}
		}

#elif MAC
		public bool Connected 
		{
			get {
				return false;
			}
		}

		public bool this [Key key]
		{
			get {
				return false;
			}
		}
#endif
	}
}

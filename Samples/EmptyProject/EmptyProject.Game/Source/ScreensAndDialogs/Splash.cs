using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	public class Splash : Dialog
	{
		public Splash()
			: base("Shell/Splash", "Start")
		{
			Root.Markers["Done"].CustomAction = () => {
				Close();
			};
		}

		public static IEnumerator<object> Show()
		{
			var splash = new Splash();
			while (!splash.IsClosed) {
				yield return 0;
			}
		}
	}

}

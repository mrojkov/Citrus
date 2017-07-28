using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView
{
	public static class RotatationHelper
	{
		public static float WrapAngle(float angle)
		{
			if (angle > 180) {
				return angle - 360;
			}
			if (angle < -180) {
				return angle + 360;
			}
			return angle;
		}

		public static float GetSnappedRotation(float rotation, bool rounded)
		{
			return rounded ? ((rotation / 15f).Round() * 15f).Snap(0) : rotation.Snap(0);
		}
	}
}

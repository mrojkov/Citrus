using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class PointObjectSelectionComponent : Component
	{
		public const float cornerOffset = 15f;
		public Quadrangle СurrentBounds { get; set; }
	}
}

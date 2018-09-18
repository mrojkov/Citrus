using Lime;

namespace Tangerine.UI.SceneView
{
	public class CommandComponent : Component
	{
		public Command Command { get; set; }
	}

	public class NodeCommandComponent : NodeComponent
	{
		public Command Command { get; set; }
	}
}

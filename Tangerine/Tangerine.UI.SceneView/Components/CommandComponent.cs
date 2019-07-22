using Lime;

namespace Tangerine.UI.SceneView
{
	public class CommandComponent : Component
	{
		public ICommand Command { get; set; }
	}

	public class NodeCommandComponent : NodeComponent
	{
		public ICommand Command { get; set; }
	}
}

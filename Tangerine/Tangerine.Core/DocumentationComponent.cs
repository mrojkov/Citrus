using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class DocumentationComponent : NodeComponent
	{
		public readonly string Filepath;
		public static Action<DocumentationComponent> Clicked { get; set; }

		private HelpGesture helpGesture;

		public DocumentationComponent(string filepath)
		{
			Filepath = filepath;
			helpGesture = new HelpGesture(() => {
				OpenDocumentation();
			});
		}

		public void OpenDocumentation()
		{
			Clicked?.Invoke(this);
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			if (oldOwner != null) {
				oldOwner.Gestures.Remove(helpGesture);
			}
			Owner.Gestures.Insert(0, helpGesture);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI.Drop
{
	public class DropGesture : Gesture
	{
		public delegate IEnumerable<string> DropHandlerDelegate(IEnumerable<string> files);

		public event DropHandlerDelegate FilesDropped;
		public DropGesture(DropHandlerDelegate dropHandler)
		{
			FilesDropped += dropHandler;
		}

		protected override void Cancel()
		{
		}

		protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (Input.TryGetDropData(out var files)) {
				if (FilesDropped != null) {
					Input.ConsumeDropData(FilesDropped.Invoke(files));
				}
			}
		}
	}
}

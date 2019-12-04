using System;
using System.Collections.Generic;

namespace Lime
{
	public class DropFilesGesture : Gesture
	{
		/// <summary>
		/// Invokes on files drop. Handler should clean up processed files.
		/// All unprocessed files will be propagated up to the hierarchy.
		/// </summary>
		public event Action<List<string>> Recognized;

		public override bool IsActive { get; }
		protected internal override bool Cancel(Gesture sender) => false;

		internal protected override void Update(float delta)
		{
			if (Input.DroppedFiles.Count > 0) {
				Recognized?.Invoke(Input.DroppedFiles);
			}
		}
	}
}

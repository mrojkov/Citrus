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

		internal protected override void Cancel() { }

		internal protected override void Update(IEnumerable<Gesture> gestures)
		{
			if (Input.DroppedFiles.Count > 0) {
				Recognized?.Invoke(Input.DroppedFiles);
			}
		}
	}
}

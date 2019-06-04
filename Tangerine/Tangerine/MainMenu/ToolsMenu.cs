using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Lime;
using Tangerine.Core;
using Tangerine.Dialogs;
using Tangerine.UI;
using Tangerine.UI.Timeline;

namespace Tangerine.MainMenu
{
	class RenderToPngSequence : CommandHandler
	{
		public override void Execute()
		{
			if (!GridSelection.GetSelectionBoundaries(out var gs)) {
				new AlertDialog("Select a range on the timeline", "Ok").Show();
				return;
			}
			var currentWindow = Window.Current;
			if (new RenderToPngSequenceDialog().Show(out var options) != RenderToPngSequenceDialog.Result.Ok) {
				return;
			}
			if (!Directory.Exists(options.Folder)) {
				new AlertDialog("Folder does not exist", "Ok").Show();
				return;
			}
			currentWindow.Activate();
			WidgetContext.Current.Root.Tasks.Add(RenderPngSequenceTask(currentWindow, options, gs.Left, gs.Right));
		}

		private static IEnumerator<object> RenderPngSequenceTask(IWindow currentWindow, RenderToPngSequenceDialog.RenderToPngSequenceOptions options, int min, int max)
		{
			var start = AnimationUtils.FramesToSeconds(min);
			Document.Current.AnimationFrame = min;
			var end = AnimationUtils.FramesToSeconds(max);
			var delta = 1f / options.FPS;
			var i = 0;
			var animation = Document.Current.Animation;
			animation.IsRunning = true;
			while (start < end) {
				currentWindow.InvokeOnRendering(() => {
					var bitmap = Document.Current.Container.AsWidget.ToBitmap();
					bitmap.SaveTo(Path.Combine(options.Folder, $"{i:D3}.png"));
					bitmap.Dispose();
				});
				yield return null;
				animation.Advance(delta);
				start += delta;
				Application.InvalidateWindows();
				i += 1;
			}
			animation.IsRunning = false;
		}
	}
}

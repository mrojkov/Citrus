using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			var gs = GridSelection.GetSelectionBoundaries();
			if (!gs.HasValue) {
				new AlertDialog("Select range on timeline", "Ok").Show();
				return;
			}
			var currentWindow = Window.Current;
			if (new RenderToPngSequenceDialog().Show(out var options) != RenderToPngSequenceDialog.Result.Ok) {
				return;
			}
			if (!Directory.Exists(options.Folder)) {
				new AlertDialog("Folder does not exists.", "Ok").Show();
				return;
			}
			currentWindow.Activate();
			currentWindow.InvokeOnRendering(() => {
				var start = AnimationUtils.FramesToSeconds(gs.Value.Left);
				var savedStart = AnimationUtils.FramesToSeconds(Document.Current.AnimationFrame);
				var end = AnimationUtils.FramesToSeconds(gs.Value.Right);
				var delta = 1f / options.FPS;
				var animationHosts = Document.Current.Container.Nodes.Select(n => n as IAnimationHost).Where(ah => ah != null).ToList();
				var i = 0;
				while (start < end) {
					foreach (var animationHost in animationHosts) {
						foreach (var animator in animationHost.Animators) {
							if (animator.AnimationId == Document.Current.AnimationId) {
								animator.Apply(start);
							}
						}
					}
					start += delta;
					var bitmap = Document.Current.Container.AsWidget.ToBitmap();
					bitmap.SaveTo(Path.Combine(options.Folder, $"{i:D3}.png"));
					i += 1;
				}
				foreach (var animationHost in animationHosts) {
					foreach (var animator in animationHost.Animators) {
						if (animator.AnimationId == Document.Current.AnimationId) {
							animator.Apply(savedStart);
						}
					}
				}
			});
		}
	}
}

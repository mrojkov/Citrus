using Lime;
using System;
using System.Linq;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class CreateAnimationClipDialog
	{
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Button okButton;
		private readonly Button cancelButton;
		private readonly ThemedDropDownList animationSelector;

		public CreateAnimationClipDialog()
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(250, 70),
				FixedSize = true,
				Title = "Create Animation Clip",
				Visible = false,
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(animationSelector = new ThemedDropDownList { MinWidth = 100 }),
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell {
							StretchY = 0
						},
						Padding = new Thickness { Top = 5 },
						Nodes = {
							new Widget { MinMaxHeight = 0 },
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						},
					}
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			foreach (var a in Document.Current.Container.TangerineAnimations) {
				if (a.Id != Document.Current.Animation.Id) {
					animationSelector.Items.Add(new CommonDropDownList.Item(a.Id));
				}
			}
			animationSelector.Index = 0;
			cancelButton.Clicked += () => {
				window.Close();
			};
			okButton.Clicked += () => {
				var row = Document.Current.SelectedRows().FirstOrDefault();
				if (row == null) {
					return;
				}
				var track = row.Components.Get<Core.Components.AnimationTrackRow>().Track;
				Document.Current.History.DoTransaction(() => {
					var clip = new AnimationClip {
						AnimationId = animationSelector.Text,
						Begin = Document.Current.AnimationFrame,
						Offset = 0,
						Length = CalcAnimationLength(Document.Current.Container, animationSelector.Text)
					};
					InsertClip(track, Document.Current.AnimationFrame, clip);
				});
				window.Close();
			};
			cancelButton.SetFocus();
			window.ShowModal();
		}

		private static void InsertClip(AnimationTrack track, int animationFrame, AnimationClip newClip)
		{
			using (Document.Current.History.BeginTransaction()) {
				var index = FindClipContainingFrame(track, newClip.Begin);
				if (index >= 0 && newClip.Begin > track.Clips[index].Begin) {
					SplitClip(track, index, newClip.Begin);
				}
				index = FindClipContainingFrame(track, newClip.Begin + newClip.Length);
				if (index >= 0 && newClip.End < track.Clips[index].End) {
					SplitClip(track, index, newClip.End);
				}
				for (int i = track.Clips.Count - 1; i >= 0; i--) {
					var c = track.Clips[i];
					if (c.Begin >= newClip.Begin && c.Begin < newClip.End) {
						RemoveClip(track, i);
					}
				}
				index = FindClipInsertionIndex(track, newClip.Begin);
				Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index, newClip);
				Document.Current.History.CommitTransaction();
			}
		}

		private static void RemoveClip(AnimationTrack track, int index)
		{
			Core.Operations.RemoveFromList<AnimationClipList, AnimationClip>.Perform(track.Clips, index);
		}

		private static void SplitClip(AnimationTrack track, int index, int frame)
		{
			var clip = track.Clips[index];
			if (frame <= clip.Begin || frame > clip.End) {
				throw new InvalidOperationException();
			}
			var newClip = clip.Clone();
			newClip.Begin = frame;
			newClip.End = clip.End;
			Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.End), frame);
			Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index + 1, newClip);
		}

		private static int FindClipContainingFrame(AnimationTrack track, int frame)
		{
			int i = 0;
			foreach (var c in track.Clips) {
				if (c.Begin <= frame && frame < c.End) {
					return i;
				}
				i++;
			}
			return -1;
		}

		private static int FindClipInsertionIndex(AnimationTrack track, int frame)
		{
			int i = 0;
			foreach (var c in track.Clips) {
				if (c.Begin > frame) {
					break;
				}
				i++;
			}
			return i;
		}

		static int CalcAnimationLength(Node node, string animationId)
		{
			int result = 0;
			foreach (var a in node.Animators) {
				if (a.AnimationId == animationId) {
					result = Math.Max(result, a.Duration);
				}
			}
			foreach (var n in node.Nodes) {
				result = Math.Max(result, CalcAnimationLength(n, animationId));
			}
			return result;
		}
	}
}

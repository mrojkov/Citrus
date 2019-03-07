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
				using (Document.Current.History.BeginTransaction()) {
					var clip = new AnimationClip {
						AnimationId = animationSelector.Text,
						Begin = Document.Current.AnimationFrame,
						Offset = 0,
						Length = CalcAnimationLength(Document.Current.Container, animationSelector.Text) + 1
					};
					AnimationClipToolbox.InsertClip(track, clip);
				}
				window.Close();
			};
			cancelButton.SetFocus();
			window.ShowModal();
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

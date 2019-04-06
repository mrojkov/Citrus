using Lime;
using System;
using System.Linq;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class AddAnimationClip
	{
		public static bool IsEnabled() => AddAnimationClipDialog.EnumerateAnimations().Any();

		public static void Perform(IntVector2 cell)
		{
			if (cell.Y >= 0 && IsEnabled()) {
				new AddAnimationClipDialog(cell);
			}
		}

		class AddAnimationClipDialog
		{
			private readonly Window window;
			private readonly WindowWidget rootWidget;
			private readonly Button okButton;
			private readonly Button cancelButton;
			private readonly ThemedDropDownList animationSelector;

			public AddAnimationClipDialog(IntVector2 cell)
			{
				window = new Window(new WindowOptions {
					ClientSize = new Vector2(250, 70),
					FixedSize = true,
					Title = "Add Animation Clip",
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
				foreach (var a in EnumerateAnimations()) {
					animationSelector.Items.Add(new CommonDropDownList.Item(a.Id));
				}
				animationSelector.Index = 0;
				cancelButton.Clicked += () => window.Close();
				okButton.Clicked += () => {
					var track = Document.Current.Rows[cell.Y].Components.Get<Core.Components.AnimationTrackRow>().Track;
					Document.Current.History.DoTransaction(() => {
						var animation = Document.Current.Container.Animations.Find(animationSelector.Text);
						if (!animation.AnimationEngine.AreEffectiveAnimatorsValid(animation)) {
							// Refreshes animation duration either
							animation.AnimationEngine.BuildEffectiveAnimators(animation);
						}
						var clip = new AnimationClip {
							AnimationId = animationSelector.Text,
							BeginFrame = cell.X,
							InFrame = 0,
							DurationInFrames = animation.DurationInFrames + 1
						};
						AnimationClipToolbox.InsertClip(track, clip);
					});
					window.Close();
				};
				cancelButton.SetFocus();
				window.ShowModal();
			}

			public static IEnumerable<Animation> EnumerateAnimations()
			{
				foreach (var a in Document.Current.Animation.Owner.Animations.OrderBy(i => i.Id)) {
					if (!a.IsLegacy && a.Id != Document.Current.Animation.Id) {
						yield return a;
					}
				}
			}
		}
	}
}

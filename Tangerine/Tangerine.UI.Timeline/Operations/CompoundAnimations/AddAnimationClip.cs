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
			private readonly ThemedDropDownList beginMarkerSelector;
			private readonly ThemedDropDownList endMarkerSelector;

			public AddAnimationClipDialog(IntVector2 cell)
			{
				window = new Window(new WindowOptions {
					ClientSize = new Vector2(250, 140),
					FixedSize = true,
					Title = "Add Clip",
					Visible = false,
				});
				rootWidget = new ThemedInvalidableWindowWidget(window) {
					Padding = new Thickness(8),
					Layout = new VBoxLayout(),
					Nodes = {
						new Widget {
							Layout = new TableLayout {
								Spacing = 4,
								RowCount = 3,
								ColumnCount = 2,
								ColumnDefaults = new List<DefaultLayoutCell> {
									new DefaultLayoutCell(Alignment.RightCenter) { StretchX = 1 },
									new DefaultLayoutCell(Alignment.LeftCenter) { StretchX = 2 },
								}
							},
							LayoutCell = new LayoutCell { StretchY = 0 },
							Nodes = {
								new ThemedSimpleText("Animation"),
								(animationSelector = new ThemedDropDownList()),
								new ThemedSimpleText("Begin Marker"),
								(beginMarkerSelector = new ThemedDropDownList()),
								new ThemedSimpleText("End Marker"),
								(endMarkerSelector = new ThemedDropDownList())
							}
						},
						new Widget {
							// Vertical stretcher
						},
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
				animationSelector.Changed += _ => RefreshMarkers();
				RefreshMarkers();
				cancelButton.Clicked += () => window.Close();
				okButton.Clicked += () => {
					var animation = Document.Current.Container.Animations.Find(animationSelector.Text);
					if (!animation.AnimationEngine.AreEffectiveAnimatorsValid(animation)) {
						// Refreshes animation duration either
						animation.AnimationEngine.BuildEffectiveAnimators(animation);
					}
					if (animation.CalcDurationInFrames() == 0) {
						AlertDialog.Show("Please select an animation with non-zero duration", "Ok");
						return;
					}
					int beginFrame = (int?)beginMarkerSelector.Value ?? 0;
					int endFrame = (int?)endMarkerSelector.Value ?? animation.CalcDurationInFrames();
					if (beginFrame >= endFrame) {
						AlertDialog.Show("Please select markers in ascending order", "Ok");
						return;
					}
					var track = Document.Current.Rows[cell.Y].Components.Get<Core.Components.AnimationTrackRow>().Track;
					Document.Current.History.DoTransaction(() => {
						var clip = new AnimationClip {
							AnimationId = animationSelector.Text,
							BeginFrame = cell.X,
							InFrame = beginFrame,
							DurationInFrames = endFrame - beginFrame
						};
						AnimationClipToolbox.InsertClip(track, clip);
						Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.IsSelected), true);
					});
					window.Close();
				};
				cancelButton.SetFocus();
				window.ShowModal();
			}

			private void RefreshMarkers()
			{
				var animation = Document.Current.Animation.Owner.Animations.Find(animationSelector.Text);
				beginMarkerSelector.Items.Clear();
				endMarkerSelector.Items.Clear();
				beginMarkerSelector.Items.Add(new CommonDropDownList.Item("<none>", null));
				endMarkerSelector.Items.Add(new CommonDropDownList.Item("<none>", null));
				beginMarkerSelector.Index = 0;
				endMarkerSelector.Index = 0;
				foreach (var marker in animation.Markers) {
					if (!string.IsNullOrEmpty(marker.Id)) {
						beginMarkerSelector.Items.Add(new CommonDropDownList.Item(marker.Id, marker.Frame));
						endMarkerSelector.Items.Add(new CommonDropDownList.Item(marker.Id, marker.Frame));
					}
				}
				beginMarkerSelector.Enabled = endMarkerSelector.Enabled = animation.Markers.Count > 0;
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

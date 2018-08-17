using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Operations;

namespace Tangerine.UI.Timeline
{
	public class NumericScaleDialog
	{
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Button okButton;
		private readonly Button cancelButton;
		private readonly Widget container;
		public float Scale { get; set; }

		public NumericScaleDialog()
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(250, 70),
				FixedSize = true,
				Title = "Numeric scale",
				Visible = false,
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					(container = new Widget {
						Layout = new VBoxLayout(),
					}),
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
			Scale = 1;
			var editor = new FloatPropertyEditor(new PropertyEditorParams(container, this, nameof(Scale), "Scale"));
			cancelButton.Clicked += () => {
				window.Close();
			};
			this.okButton.Clicked += () => {
				editor.Submit();
				Document.Current.History.DoTransaction(() => {
					ScaleKeyframes();
				});
				window.Close();
			};
			window.ShowModal();
		}

		private void ScaleKeyframes()
		{
			var boundaries = GridSelection.GetSelectionBoundaries();
			if (boundaries.HasValue || Scale < Mathf.ZeroTolerance) {
				var saved = new List<IKeyframe>();
				for (int i = boundaries.Value.Top; i <= boundaries.Value.Bottom; ++i) {
					if (!(Document.Current.Rows[i].Components.Get<NodeRow>()?.Node is IAnimationHost animable)) {
						continue;
					}
					foreach (var animator in animable.Animators.ToList()) {
						saved.Clear();
						IEnumerable<IKeyframe> keys = animator.ReadonlyKeys.Where(k =>
							k.Frame >= boundaries.Value.Left && k.Frame < boundaries.Value.Right
						).ToList();
						if (Scale < 1) {
							keys = keys.Reverse().ToList();
						}
						foreach (var key in keys) {
							saved.Add(key);
							RemoveKeyframe.Perform(animator, key.Frame);
						}
						foreach (var key in saved) {
							int newFrame = (int)(boundaries.Value.Left + (key.Frame - boundaries.Value.Left) * Scale);
							var newKey = key.Clone();
							newKey.Frame = newFrame;
							SetAnimableProperty.Perform(
								animable, animator.TargetPropertyPath, newKey.Value,
								createAnimatorIfNeeded: true,
								createInitialKeyframeForNewAnimator: false,
								newKey.Frame
							);
							SetKeyframe.Perform(animable, animator.TargetPropertyPath, Document.Current.AnimationId, newKey);
						}
					}
				}
				ClearGridSelection.Perform();
				for (int i = boundaries.Value.Top; i <= boundaries.Value.Bottom; ++i) {
					SelectGridSpan.Perform(i, boundaries.Value.Left, (int)(boundaries.Value.Left + (boundaries.Value.Right - boundaries.Value.Left) * Scale));
				}
			} else {
				Document.Current.History.RollbackTransaction();
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Lime;

namespace Orange
{
	class CookOrRevealSelectedBundles
	{
		private static Window window;
		private static WindowWidget windowWidget;
		private static Widget mainVBox;
		private static ThemedEditBox filter;
		private static ThemedScrollView scrollView;
		private static ThemedButton selectButton;
		private static Dictionary<string, ThemedCheckBox> checkboxes;
		private static Dictionary<string, Widget> lines;
		private static Action action;
		private static bool windowClosed;


		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Cook/Reveal Selected Bundles")]
		[ExportMetadata("Priority", 15)]
		public static void CookOrRevealSelectedBundlesAction()
		{
			var target = The.UI.GetActiveTarget();

			windowClosed = false;
			action = null;
			checkboxes = new Dictionary<string, ThemedCheckBox>();
			lines = new Dictionary<string, Widget>();
			Application.InvokeOnMainThread(() => CreateSelectionWindow(target));
			// Selection window can be created only on main thread
			// We should wait for that window to close, or user will
			// be able to run multiple actions at the same time, leading to crash
			while (!windowClosed);
			action?.Invoke();
		}

		private static void CreateSelectionWindow(Target target)
		{
			var windowSize = new Vector2(400, 400);
			window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Bundle selection",
				Style = WindowStyle.Dialog,
				Visible = false
			});
			windowWidget = new ThemedInvalidableWindowWidget(window) {
				Layout = new HBoxLayout {
					Spacing = 6
				},
				Padding = new Thickness(6),
				Size = windowSize
			};
			mainVBox = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				}
			};
			scrollView = new ThemedScrollView();
			scrollView.CompoundPostPresenter.Add(new WidgetBoundsPresenter(Lime.Theme.Colors.ControlBorder));
			scrollView.Content.Layout = new VBoxLayout {
				Spacing = 6
			};
			scrollView.Content.Padding = new Thickness(6);
			scrollView.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.White));

			windowWidget.AddNode(mainVBox);

			selectButton = new ThemedButton {
				Text = "Select all",
				Clicked = SelectButtonClickHandler
			};

			filter = new ThemedEditBox();
			filter.Tasks.Add(FilterBundles);
			mainVBox.AddNode(filter);

			mainVBox.AddNode(scrollView);
			foreach (var bundle in GetBundles(target)) {
				checkboxes[bundle] = new ThemedCheckBox();
				lines[bundle] = new Widget {
					Layout = new HBoxLayout {
						Spacing = 8
					},
					Nodes = {
						checkboxes[bundle],
						new ThemedSimpleText(bundle) {
							HitTestTarget = true,
							Clicked = checkboxes[bundle].Toggle
						}
					}
				};
				scrollView.Content.AddNode(lines[bundle]);
			}

			var cookButton = new ThemedButton {
				Clicked = () => CookButtonClickHandler(target),
				Text = "Cook"
			};
			var revealButton = new ThemedButton {
				Clicked = () => RevealButtonClickHandler(target),
				Text = "Reveal"
			};
			var buttonLine = new Widget {
				Layout = new HBoxLayout {
					Spacing = 6
				}
			};
			mainVBox.AddNode(buttonLine);
			buttonLine.AddNode(selectButton);
			selectButton.Tasks.Add(UpdateTextOfSelectButton());
			buttonLine.AddNode(new Widget { LayoutCell = new LayoutCell { StretchX = float.MaxValue }, MaxHeight = 0 });
			buttonLine.AddNode(cookButton);
			buttonLine.AddNode(revealButton);
			window.Closed += () => { windowClosed = true; };
			window.ShowModal();
		}

		private static List<string> GetBundles(Target target)
		{
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, target);
			var bundles = new HashSet<string>();
			foreach (var dictionaryItem in cookingRulesMap) {
				foreach (var bundle in dictionaryItem.Value.Bundles) {
					bundles.Add(bundle);
				}
			}
			return bundles.ToList();
		}

		private static void SelectButtonClickHandler()
		{
			var deselect = true;
			foreach (var bundle in checkboxes.Keys) {
				if (!checkboxes[bundle].Checked) {
					deselect = false;
				}
			}
			foreach (var bundle in checkboxes.Keys) {
				checkboxes[bundle].Checked = !deselect;
			}
		}

		private static IEnumerator<object> UpdateTextOfSelectButton()
		{
			while (true) {
				var allChecked = true;
				foreach (var bundle in checkboxes.Keys) {
					if (!checkboxes[bundle].Checked) {
						allChecked = false;
					}
				}
				if (allChecked) {
					selectButton.Text = "Deselect all";
				} else {
					selectButton.Text = "Select all";
				}
				yield return null;
			}
		}

		private static IEnumerator<object> FilterBundles()
		{
			var lastText = string.Empty;
			while (true) {
				var text = filter.Text;
				if (text == lastText) {
					yield return null;
				}
				lastText = text;
				foreach (var bundle in lines.Keys) {
					lines[bundle].Visible = bundle.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
				}
				yield return null;
			}
		}

		private static void CookButtonClickHandler(Target target)
		{
			var bundles = new List<string>();
			foreach (var bundle in checkboxes.Keys) {
				if (checkboxes[bundle].Checked) {
					bundles.Add(bundle);
				}
			}
			action = () => AssetCooker.Cook(target, bundles);
			window.Close();
		}

		private static void RevealButtonClickHandler(Target target)
		{
			var bundles = new List<string>();
			foreach (var bundle in checkboxes.Keys) {
				if (checkboxes[bundle].Checked) {
					bundles.Add(bundle);
				}
			}
			action = () => AssetsUnpacker.Unpack(target, bundles);
			window.Close();
		}
	}
}

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
		private static ThemedScrollView scrollView;
		private static ThemedButton selectButton;
		private static Dictionary<string, ThemedCheckBox> bundleMap;
		private static Action action;
		private static bool windowClosed;


		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Cook/Reveal Selected Bundles")]
		[ExportMetadata("Priority", 15)]
		public static void CookOrRevealSelectedBundlesAction()
		{
			windowClosed = false;
			action = null;
			bundleMap = new Dictionary<string, ThemedCheckBox>();
			Application.InvokeOnMainThread(CreateSelectionWindow);
			// Selection window can be created only on main thread
			// We should wait for that window to close, or user will
			// be able to run multiple actions at the same time, leading to crash
			while (!windowClosed);
			action?.Invoke();
		}

		private static void CreateSelectionWindow()
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

			mainVBox.AddNode(scrollView);
			foreach (var bundle in GetBundles()) {
				bundleMap[bundle] = new ThemedCheckBox();
				scrollView.Content.AddNode(new Widget {
					Layout = new HBoxLayout {
						Spacing = 8
					},
					Nodes = {
						bundleMap[bundle],
						new ThemedSimpleText(bundle) {
							HitTestTarget = true,
							Clicked = bundleMap[bundle].Toggle
						}
					}
				});
			}

			var cookButton = new ThemedButton {
				Clicked = CookButtonClickHandler,
				Text = "Cook"
			};
			var revealButton = new ThemedButton {
				Clicked = RevealButtonClickHandler,
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

		private static List<string> GetBundles()
		{
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);
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
			var unfold = true;
			foreach (var bundle in bundleMap.Keys) {
				if (!bundleMap[bundle].Checked) {
					unfold = false;
				}
			}

			foreach (var bundle in bundleMap.Keys) {
				bundleMap[bundle].Checked = !unfold;
			}

		}

		private static IEnumerator<object> UpdateTextOfSelectButton()
		{
			while (true) {
				var allChecked = true;
				foreach (var bundle in bundleMap.Keys) {
					if (!bundleMap[bundle].Checked) {
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

		private static void CookButtonClickHandler()
		{
			var bundles = new List<string>();
			foreach (var bundle in bundleMap.Keys) {
				if (bundleMap[bundle].Checked) {
					bundles.Add(bundle);
				}
			}
			action = () => AssetCooker.Cook(The.Workspace.ActiveTarget, bundles);
			window.Close();
		}

		private static void RevealButtonClickHandler()
		{
			var bundles = new List<string>();
			foreach (var bundle in bundleMap.Keys) {
				if (bundleMap[bundle].Checked) {
					bundles.Add(bundle);
				}
			}
			action = () => AssetsUnpacker.Unpack(The.Workspace.ActivePlatform, bundles);
			window.Close();
		}
	}
}

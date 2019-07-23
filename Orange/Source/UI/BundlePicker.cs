using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Orange
{
	public static class BundlePicker
	{
		private static Widget mainVBox;
		private static ThemedEditBox filter;
		private static ThemedScrollView scrollView;
		private static ThemedButton selectButton;
		private static Dictionary<string, ThemedCheckBox> checkboxes;
		private static Dictionary<string, Widget> lines;

		public static Widget CreateBundlePicker()
		{
			if (mainVBox != null) {
				return mainVBox;
			}

			checkboxes = new Dictionary<string, ThemedCheckBox>();
			lines = new Dictionary<string, Widget>();

			mainVBox = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				},
				MaxWidth = 250f
			};
			scrollView = new ThemedScrollView();
			scrollView.CompoundPostPresenter.Add(new WidgetBoundsPresenter(Lime.Theme.Colors.ControlBorder));
			scrollView.Content.Layout = new VBoxLayout {
				Spacing = 6
			};
			scrollView.Content.Padding = new Thickness(6);
			scrollView.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.White));

			selectButton = new ThemedButton {
				Text = "Select all",
				Clicked = SelectButtonClickHandler
			};

			filter = new ThemedEditBox();
			filter.Tasks.Add(FilterBundles);
			mainVBox.AddNode(filter);
			mainVBox.AddNode(scrollView);
			var buttonLine = new Widget {
				Layout = new HBoxLayout {
					Spacing = 6
				}
			};
			mainVBox.AddNode(buttonLine);
			buttonLine.AddNode(new Widget { LayoutCell = new LayoutCell { StretchX = float.MaxValue }, MaxHeight = 0 });
			buttonLine.AddNode(selectButton);
			selectButton.Tasks.Add(UpdateTextOfSelectButton());

			return mainVBox;
		}

		public static void CreateBundlesList()
		{
			if (scrollView.Content.Nodes.Count > 0) {
				scrollView.Content.Nodes.Clear();
			}
			foreach (var bundle in AssetCooker.GetListOfAllBundles()) {
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
				checkboxes[bundle].Checked = true;
			}
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
				}
				else {
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

		public static List<string> GetSelectedBundles()
		{
			if (scrollView == null || scrollView.Content.Nodes.Count == 0) {
				return AssetCooker.GetListOfAllBundles();
			}
			var bundles = new List<string>();
			foreach (var bundle in checkboxes.Keys) {
				if (checkboxes[bundle].Checked) {
					bundles.Add(bundle);
				}
			}
			return bundles;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Orange
{
	public class BundlePicker
	{
		private Dictionary<string, bool> bundleSelectionStates;
		private List<string> allBundles;

		/// <summary>
		/// When enabled, user can select which bundles to use in actions. When disabled, actions will use all bundles.
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// Creates selection states dictionary for bundles in current project.
		/// To be able to select bundles you should manually enable BundlePicker after setup.
		/// </summary>
		public void Setup()
		{
			if (bundleSelectionStates == null) {
				bundleSelectionStates = new Dictionary<string, bool>();
			} else {
				bundleSelectionStates.Clear();
			}
			Enabled = false;

			allBundles = new AssetCooker(The.UI.GetActiveTarget()).GetListOfAllBundles();
			foreach (var bundle in allBundles) {
				bundleSelectionStates.Add(bundle, true);
			}
		}

		/// <summary>
		/// Updates current bundle list: new bundles will be added to list (default state: checked),
		/// deleted bundles will be removed from list. Returns list of changed (added or deleted) bundles.
		/// </summary>
		public List<string> Refresh()
		{
			var changed = new List<string>();
			allBundles = new AssetCooker(The.UI.GetActiveTarget()).GetListOfAllBundles();

			// Remove no longer existing bundles
			foreach (var bundle in bundleSelectionStates.Keys.ToArray()) {
				if (!allBundles.Contains(bundle)) {
					changed.Add(bundle);
					bundleSelectionStates.Remove(bundle);
				}
			}

			// Add new bundles
			foreach (var bundle in allBundles) {
				if (!bundleSelectionStates.ContainsKey(bundle)) {
					changed.Add(bundle);
					bundleSelectionStates.Add(bundle, true);
				}
			}

			return changed;
		}

		/// <summary>
		/// Returns list of all bundles if not enabled; Returns list of selected bundles otherwise.
		/// </summary>
		public List<string> GetSelectedBundles()
		{
			if (allBundles == null) {
				Setup();
			}
			if (!Enabled) {
				Refresh();
				return allBundles;
			}
			return bundleSelectionStates.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
		}

		/// <summary>
		/// Sets bundle state
		/// </summary>
		/// <param name="bundle">Path to bundle, relative to current project folder</param>
		/// <param name="state">'true' if bundle should be selected, 'false' otherwise</param>
		public void SetBundleSelection(string bundle, bool state)
		{
			bundleSelectionStates[bundle] = state;
		}
	}
}

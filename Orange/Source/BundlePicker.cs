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

		private static BundlePicker instance = new BundlePicker();

		public static BundlePicker Instance
		{
			get {
				if (instance == null) {
                    instance = new BundlePicker();
				}
				return instance;
			}
		}

		private bool enabled;
		/// <summary>
		/// When enabled, user can select which bundles to use in actions. When disabled, actions will use all bundles.
		/// </summary>
		public static bool Enabled
		{
			get => Instance.enabled;
			set => Instance.enabled = value;
		}

		/// <summary>
		/// Creates selection states dictionary for bundles in current project.
		/// To be able to select bundles you should manually enable BundlePicker after setup.
		/// </summary>
		public void Setup()
		{
			if (Instance.bundleSelectionStates == null) {
				Instance.bundleSelectionStates = new Dictionary<string, bool>();
			} else {
                Instance.bundleSelectionStates.Clear();
			}
			Enabled = false;

			Instance.allBundles = new AssetCooker(The.UI.GetActiveTarget()).GetListOfAllBundles();
			foreach (var bundle in Instance.allBundles) {
				Instance.bundleSelectionStates.Add(bundle, true);
			}
		}

		/// <summary>
		/// Returns list of all bundles if not enabled; Returns list of selected bundles otherwise.
		/// </summary>
		public List<string> GetSelectedBundles()
		{
			if (Instance.allBundles == null) {
                Setup();
			}

			if (!Enabled) {
				return Instance.allBundles;
			}
			return Instance.bundleSelectionStates.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
		}

		/// <summary>
		/// Sets bundle state
		/// </summary>
		/// <param name="bundle">Path to bundle, relative to current project folder</param>
		/// <param name="state">'true' if bundle should be selected, 'false' otherwise</param>
		public void SetBundleSelection(string bundle, bool state)
		{
			Instance.bundleSelectionStates[bundle] = state;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lime;
using Orange;
using Tangerine.UI;

namespace Tangerine
{
	public class OrangeInterface : Orange.UserInterface
	{
		public Command CacheBoth;
		public Command CacheRemote;
		public Command CacheLocal;
		public Command CacheNone;

		public OrangePluginUIBuilder PluginUIBuilder;

		public readonly PlatformPicker PlatformPicker = new PlatformPicker();

		public override bool AskConfirmation(string text)
		{
			bool? result = null;
			Application.InvokeOnMainThread(() => result = AlertDialog.Show(text, new string[] { "Yes", "No" }) == 0);
			while (result == null) {
				Thread.Sleep(1);
			}
			return result.Value;
		}

		public override bool AskChoice(string text, out bool yes)
		{
			throw new NotImplementedException();
		}

		public override void ShowError(string message)
		{
			Application.InvokeOnMainThread(() => AlertDialog.Show(message));
		}

		public override Target GetActiveTarget()
		{
			return PlatformPicker.SelectedTarget;
		}

		public override bool DoesNeedSvnUpdate()
		{
			return false;
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			PluginUIBuilder = new OrangePluginUIBuilder();
			return PluginUIBuilder;
		}

		public override void CreatePluginUI(IPluginUIBuilder builder) { }
		public override void DestroyPluginUI() { }

		public override void OnWorkspaceOpened()
		{
			PlatformPicker.Reload();
		}

		public override void SetupProgressBar(int maxPosition) { }
		public override void StopProgressBar() { }
		public override void IncreaseProgressBar(int amount = 1) { }

		public void CacheCommandsHandler(AssetCache.EnableState state)
		{
			CacheBoth.Checked = false;
			CacheRemote.Checked = false;
			CacheLocal.Checked = false;
			CacheNone.Checked = false;
			switch (state) {
				case AssetCache.EnableState.Both:
					CacheBoth.Checked = true;
					break;
				case AssetCache.EnableState.Remote:
					CacheRemote.Checked = true;
					break;
				case AssetCache.EnableState.Local:
					CacheLocal.Checked = true;
					break;
				case AssetCache.EnableState.None:
					CacheNone.Checked = true;
					break;
			}

			The.Workspace.AssetCacheEnabled = state;
		}

		public override void LoadFromWorkspaceConfig(WorkspaceConfig config)
		{
			CacheCommandsHandler(config.AssetCacheEnabled);
		}
	}

	public class PlatformPicker : ThemedDropDownList
	{
		public PlatformPicker()
		{
			Reload();
		}

		public void Reload()
		{
			var savedIndex = Index;
			Index = -1;
			Items.Clear();
			foreach (var target in The.Workspace.Targets) {
				Items.Add(new Item(target.Name, target));
			}
			if (savedIndex >= 0 && savedIndex < Items.Count) {
				Index = savedIndex;
			} else {
				Index = 0;
			}
		}

		public Target SelectedTarget => (Target)Items[Index].Value;
	}

	public class OrangePluginUIBuilder : IPluginUIBuilder
	{
		public IPluginPanel SidePanel { get; } = new OrangePluginPanel();
	}

	public class OrangePluginPanel :  IPluginPanel
	{
		public class PluginCheckBox : ICheckBox
		{
			public string Label { get; }
			public bool Active { get; set; }
			public event EventHandler Toggled;

			public PluginCheckBox(string label)
			{
				Label = label;
			}

			public void Toogle()
			{
				Toggled?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool Enabled { get; set; }
		public string Title { get; set; }
		public List<PluginCheckBox> CheckBoxes { get; } = new List<PluginCheckBox>();

		public ICheckBox AddCheckBox(string label)
		{
			var checkBox = new PluginCheckBox(label);
			CheckBoxes.Add(checkBox);
			return checkBox;
		}
	}
}

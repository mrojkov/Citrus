using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.UI;

namespace Tangerine
{
	public class ShortcutBinding
	{
		public ICommand Command { get; set; }
		public Shortcut Shortcut { get; set; }
	}

	public static class HotkeyRegistry
	{
		private static List<ShortcutBinding> defaults = new List<ShortcutBinding>();

		private static HotkeyProfile currentProfile;
		public static HotkeyProfile CurrentProfile {
			get { return currentProfile; }
			set {
				if (value == null) {
					throw new ArgumentNullException();
				}
				if (currentProfile != null) {
					foreach (var command in currentProfile.Commands) {
						command.Command.Shortcut = new Shortcut();
					}
				}
				currentProfile = value;
				foreach (var command in currentProfile.Commands) {
					command.Command.Shortcut = command.Shortcut;
				}
				AppUserPreferences.Instance.CurrentHotkeyProfile = currentProfile.Name;
			}
		}

		public static List<HotkeyProfile> Profiles { get; private set; } = new List<HotkeyProfile>();
		public static string ProfilesDirectory => Lime.Environment.GetPathInsideDataDirectory("Tangerine", "HotkeyProfiles");
		public static string DefaultProfileName { get; set; } = "Default";
		public static Action Reseted { get; set; }

		public static void InitDefaultShortcuts()
		{
			defaults.Clear();
			foreach (var command in CommandRegistry.RegisteredCommands()) {
				defaults.Add(new ShortcutBinding {
					Command = command,
					Shortcut = command.Shortcut
				});
			}
		}

		public static HotkeyProfile CreateProfile(string profileName)
		{
			var profile = new HotkeyProfile(CommandRegistry.RegisteredCategories(), profileName);
			return profile;
		}

		public static void ResetToDefaults()
		{
			var defaultProfile = new HotkeyProfile(CommandRegistry.RegisteredCategories(), DefaultProfileName);
			foreach (var binding in defaults) {
				binding.Command.Shortcut = binding.Shortcut;
			}
			Profiles.Clear();
			foreach (string file in Directory.EnumerateFiles(ProfilesDirectory)) {
				File.Delete(file);
			}
			currentProfile = CreateProfile(DefaultProfileName);
			currentProfile.Save();
			Profiles.Add(currentProfile);
			Reseted?.Invoke();
		}

		public static void UpdateProfiles()
		{
			string oldCurrentName = CurrentProfile.Name;
			var newProfiles = new List<HotkeyProfile>();
			for (int i = 0; i < Profiles.Count; ++i) {
				var profile = Profiles[i];
				var newProfile = new HotkeyProfile(CommandRegistry.RegisteredCategories(), profile.Name);
				newProfile.Load();
				Profiles[i] = newProfile;
			}
			CurrentProfile = Profiles.FirstOrDefault(i => i.Name == oldCurrentName);
		}
	}

	public class HotkeyProfile
	{
		public string Filepath => Path.Combine(HotkeyRegistry.ProfilesDirectory, Name);

		public IEnumerable<CommandInfo> Commands => Categories.SelectMany(i => i.Commands.Values);
		public readonly List<CommandCategoryInfo> Categories;

		public readonly string Name;

		internal HotkeyProfile(IEnumerable<CommandCategoryInfo> categories, string name)
		{
			Categories = new List<CommandCategoryInfo>();
			foreach (var categoryInfo in categories) {
				var newCategoryInfo = new CommandCategoryInfo(categoryInfo.Id);
				foreach (var commandInfo in categoryInfo.Commands.Values) {
					var newCommandInfo = new CommandInfo(commandInfo.Command, newCategoryInfo, commandInfo.Id) {
						Shortcut = commandInfo.Command.Shortcut
					};
					newCategoryInfo.Commands.Add(newCommandInfo.Id, newCommandInfo);
				}
				Categories.Add(newCategoryInfo);
			}
			Name = name;
		}

		public void Load(string filepath)
		{
			var data =
				Serialization.ReadObjectFromFile<Dictionary<string, Dictionary<string, string>>>(filepath);
			foreach (var i in data) {
				var category = Categories.FirstOrDefault(j => j.Id == i.Key);
				if (category != null) {
					foreach (var binding in i.Value) {
						var info = category.Commands.Values.FirstOrDefault(j => j.Id == binding.Key);
						if (info != null) {
							try {
								info.Shortcut = new Shortcut(binding.Value);
							}
							catch (System.Exception) {
								Debug.Write($"Unknown shortcut: {binding.Value}");
							}
						} else {
							Debug.Write($"Unknown command: {i.Key}.{binding.Key}");
						}
					}
				} else {
					Debug.Write($"Unknown command category: {i.Key}");
				}
			}
		}

		public void Load()
		{
			Load(Filepath);
		}

		public void Save(string file)
		{
			var data = new Dictionary<string, Dictionary<string, string>>();
			foreach (var category in Categories) {
				var bindings = new Dictionary<string, string>();
				foreach (var info in category.Commands.Values) {
					var shortcut = info.Shortcut.ToString();
					bindings.Add(info.Id, shortcut == "Unknown" ? null : shortcut);
				}
				data.Add(category.Id, bindings);
			}
			Serialization.WriteObjectToFile(file, data, Serialization.Format.JSON);
		}

		public void Save()
		{
			Save(Filepath);
		}

		public void Delete()
		{
			File.Delete(Filepath);
			HotkeyRegistry.Profiles.Remove(this);
		}
	}
}

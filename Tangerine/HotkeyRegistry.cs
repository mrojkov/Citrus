using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuzu;

namespace Tangerine
{
	public class ShortcutBinding
	{
		public ICommand Command { get; set; }
		public Shortcut Shortcut { get; set; }
	}

	public class CommandInfo
	{
		public CommandInfo(ICommand command, CommandCategory category, string systemName, string name = null)
		{
			Command = command;
			Category = category;
			SystemName = systemName;
			Name = String.IsNullOrEmpty(name) ? "{" + systemName + "}" : name;
			Shortcut = command.Shortcut;
		}

		public readonly ICommand Command;
		public readonly CommandCategory Category;
		public readonly string SystemName;
		public readonly string Name;
		public Shortcut Shortcut { get; set; }
	}

	public class CommandCategory
	{
		public CommandCategory(string systemName, string name = null)
		{
			SystemName = systemName;
			Name = String.IsNullOrEmpty(name) ? "{" + systemName + "}" : name;
		}

		public List<CommandInfo> Commands { get; private set; } = new List<CommandInfo>();
		public readonly string SystemName;
		public readonly string Name;
	}

	public static class HotkeyRegistry
	{
		private static List<CommandCategory> categories = new List<CommandCategory>();
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

		public static void InitCommands(Type type, string categoryName = null)
		{
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			var category = new CommandCategory(type.Name, categoryName);
			foreach (var field in fields) {
				ICommand command = field.GetValue(null) as ICommand;
				if (command != null) {
					category.Commands.Add(new CommandInfo(command, category, field.Name, command.Text));
					defaults.Add(new ShortcutBinding {
						Command = command,
						Shortcut = command.Shortcut
					});
				}
			}
			categories.Add(category);
		}

		public static void InitCommands(IEnumerable<ICommand> commands, string systemName, string categoryName = null)
		{
			var category = new CommandCategory(systemName, categoryName);
			foreach (var command in commands) {
				category.Commands.Add(new CommandInfo(command, category, command.Text, command.Text));
				defaults.Add(new ShortcutBinding {
					Command = command,
					Shortcut = command.Shortcut
				});
			}
			categories.Add(category);
		}

		public static HotkeyProfile CreateProfile(string profileName)
		{
			var profile = new HotkeyProfile(categories, profileName);
			return profile;
		}

		public static void ResetToDefaults()
		{
			var defaultProfile = new HotkeyProfile(categories, DefaultProfileName);
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
	}

	public class HotkeyProfile
	{
		public string Filepath => Path.Combine(HotkeyRegistry.ProfilesDirectory, Name);

		public IEnumerable<CommandInfo> Commands => Categories.Select(i => i.Commands).SelectMany(j => j);
		public readonly List<CommandCategory> Categories;

		public readonly string Name;

		internal HotkeyProfile(List<CommandCategory> categories, string name)
		{
			Categories = new List<CommandCategory>();
			foreach (var category in categories) {
				var newCategory = new CommandCategory(category.SystemName, category.Name);
				foreach (var command in category.Commands) {
					var newInfo = new CommandInfo(command.Command, newCategory, command.SystemName, command.Name);
					newInfo.Shortcut = command.Command.Shortcut;
					newCategory.Commands.Add(newInfo);
				}
				Categories.Add(newCategory);
			}
			Name = name;
		}

		public void Load(string filepath)
		{
			var data =
				Serialization.ReadObjectFromFile<Dictionary<string, Dictionary<string, string>>>(filepath);
			foreach (var i in data) {
				var category = Categories.FirstOrDefault(j => j.SystemName == i.Key);
				if (category != null) {
					foreach (var binding in i.Value) {
						var info = category.Commands.FirstOrDefault(j => j.SystemName == binding.Key);
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
				foreach (var info in category.Commands) {
					var shortcut = info.Shortcut.ToString();
					bindings.Add(info.SystemName, shortcut == "Unknown" ? null : shortcut);
				}
				data.Add(category.SystemName, bindings);
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

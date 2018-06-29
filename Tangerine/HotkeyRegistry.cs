using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuzu;

namespace Tangerine
{
	public static class HotkeyRegistry
	{
		public static string Filepath => Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "keybindings");

		public static IEnumerable<CommandInfo> Commands => Categories.Select(i => i.Commands).SelectMany(j => j);
		public static List<CommandCategory> Categories { get; private set; } = new List<CommandCategory>();

		private static List<ShortcutBinding> defaults = new List<ShortcutBinding>();

		public static void InitCommands(System.Type type, string categoryName = null)
		{
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			var category = new CommandCategory(type.Name, categoryName);
			foreach (var field in fields) {
				ICommand command = field.GetValue(null) as ICommand;
				if (command != null) {
					var info = new CommandInfo(command, category, field.Name, command.Text);
					info.Shortcut = command.Shortcut;
					category.Commands.Add(info);
					defaults.Add(new ShortcutBinding {
						Command = command,
						Shortcut = command.Shortcut
					});
				}
			}
			Categories.Add(category);
		}
		
		public static void Load()
		{
			var data =
				Serialization.ReadObjectFromFile<Dictionary<string, Dictionary<string, string>>>(Filepath);
			foreach (var i in data) {
				var category = Categories.FirstOrDefault(j => j.SystemName == i.Key);
				if (category != null) {
					foreach (var binding in i.Value) {
						var info = category.Commands.FirstOrDefault(j => j.SystemName == binding.Key);
						if (info != null) {
							Shortcut old = info.Command.Shortcut;
							try {
								info.Command.Shortcut = new Shortcut(binding.Value);
								info.Shortcut = info.Command.Shortcut;
							}
							catch (System.Exception) {
								Debug.Write($"Unknown shortcut: {binding.Value}");
							}
						}
						else {
							Debug.Write($"Unknown command: {i.Key}.{binding.Key}");
						}
					}
				}
				else {
					Debug.Write($"Unknown command category: {i.Key}");
				}
			}
		}
		
		public static void Save()
		{
			var data = new Dictionary<string, Dictionary<string, string>>();
			foreach (var category in Categories) {
				var bindings = new Dictionary<string, string>();
				foreach (var info in category.Commands) {
					var shortcut = info.Command.Shortcut.ToString();
					bindings.Add(info.SystemName, shortcut == "Unknown" ? null : shortcut);
				}
				data.Add(category.SystemName, bindings);
			}
			Serialization.WriteObjectToFile(Filepath, data, Serialization.Format.JSON);
		}

		public static void ResetToDefaults()
		{
			foreach (var binding in defaults) {
				binding.Command.Shortcut = binding.Shortcut;
			}
			foreach (var command in Commands) {
				command.Shortcut = command.Command.Shortcut;
			}
			Save();
		}
	}

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
}

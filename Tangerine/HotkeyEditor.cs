using Lime;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Yuzu;

namespace Tangerine
{
	public static class HotkeyEditor
	{
		public static string Filepath => Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "keybindings");

		public static Dictionary<string, CommandsCategory> Categories { get; private set; } = new Dictionary<string, CommandsCategory>();

		private static List<ShortcutBinding> defaults = new List<ShortcutBinding>();

		public static void InitCommands(System.Type type)
		{
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			var category = new CommandsCategory();
			foreach (var field in fields) {
				ICommand command = field.GetValue(null) as ICommand;
				if (command != null) {
					category.Commands.Add(field.Name, command);
					defaults.Add(new ShortcutBinding {
						Command = command,
						Shortcut = command.Shortcut
					});
				}
			}
			Categories.Add(type.Name, category);
		}

		public static void Load()
		{
			var data =
				Serialization.ReadObjectFromFile<Dictionary<string, Dictionary<string, string>>>(Filepath);
			foreach (var i in data) {
				CommandsCategory category;
				if (Categories.TryGetValue(i.Key, out category)) {
					foreach (var binding in i.Value) {
						ICommand command;
						if (category.Commands.TryGetValue(binding.Key, out command)) {
							Shortcut old = command.Shortcut;
							try {
								command.Shortcut = new Shortcut(binding.Value);
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
				foreach (var command in category.Value.Commands) {
					var shortcut = command.Value.Shortcut.ToString();
					bindings.Add(command.Key, shortcut == "Unknown" ? null : shortcut);
				}
				data.Add(category.Key, bindings);
			}
			Serialization.WriteObjectToFile(Filepath, data, Serialization.Format.JSON);
		}

		public static void ResetToDefaults()
		{
			foreach (var binding in defaults) {
				binding.Command.Shortcut = binding.Shortcut;
			}
		}
	}

	public class ShortcutBinding
	{
		public ICommand Command { get; set; }
		public Shortcut Shortcut { get; set; }
	}

	public class CommandsCategory
	{
		[YuzuRequired]
		public Dictionary<string, ICommand> Commands { get; private set; } = new Dictionary<string, ICommand>();
	}
}

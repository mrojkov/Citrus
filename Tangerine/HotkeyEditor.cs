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

		private static Dictionary<string, CommandsCategory> categories = new Dictionary<string, CommandsCategory>();

		public static void InitCommands(System.Type type)
		{
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			var category = new CommandsCategory();
			foreach (var field in fields) {
				ICommand command = field.GetValue(null) as ICommand;
				if (command != null) {
					category.Commands.Add(field.Name, command);
				}
			}
			categories.Add(type.Name, category);
		}

		public static void Load()
		{
			var data =
				Serialization.ReadObjectFromFile<Dictionary<string, Dictionary<string, string>>>(Filepath);
			foreach (var i in data) {
				CommandsCategory category;
				if (categories.TryGetValue(i.Key, out category)) {
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
			foreach (var category in categories) {
				var bindings = new Dictionary<string, string>();
				foreach (var command in category.Value.Commands) {
					var shortcut = command.Value.Shortcut.ToString();
					bindings.Add(command.Key, shortcut == "Unknown" ? null : shortcut);
				}
				data.Add(category.Key, bindings);
			}
			Serialization.WriteObjectToFile(Filepath, data, Serialization.Format.JSON);
		}
	}

	class CommandsCategory
	{
		[YuzuRequired]
		public Dictionary<string, ICommand> Commands { get; private set; } = new Dictionary<string, ICommand>();
	}
}

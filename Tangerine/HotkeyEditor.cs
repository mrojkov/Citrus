using Lime;
using System.Collections.Generic;
using System.IO;

namespace Tangerine
{
	public static class HotkeyEditor
	{
		public static string Filepath { get; set; } = "keybindings.json";

		public static bool Load(string filepath)
		{
			Filepath = filepath;
			return Load();
		}

		public static bool Load()
		{
			Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
			foreach (var i in CommandHandlerList.Global.GetItems()) {
				if (i.Command.Text == null || i.Command.Text == "")
					continue;
				try {
					commands.Add(i.Command.Text, i.Command);
				}
				catch (System.Exception exception) {
					System.Console.WriteLine($"\"{i.Command.Text}\" - " + exception.Message);
				}
			}

			Dictionary<string, string> data = new Dictionary<string, string>();
			var deserializer = Yuzu.Json.JsonDeserializer.Instance;
			try {
				using (var stream = File.OpenRead(Filepath)) {
					data = deserializer.FromStream(data, stream) as Dictionary<string, string>;
				}
				System.Console.WriteLine($"Key bindings were successfully loaded from \"{Filepath}\"");

				foreach (var i in data) {
					ICommand command;
					if (commands.TryGetValue(i.Key, out command)) {
						if (i.Value == "") {
							command.Shortcut = new Shortcut();
							continue;
						}
						Shortcut old = command.Shortcut;
						try {
							command.Shortcut = new Shortcut(i.Value);
						}
						catch (System.Exception) {
							command.Shortcut = old;
							System.Console.WriteLine($"Wrong shortcut \"{i.Key}\"");
						}
					}
					else {
						System.Console.WriteLine($"Unknown command \"{i.Key}\"");
					}
				}
				return true;
			}
			catch (System.Exception exception) {
				System.Console.WriteLine($"Can not load key bindings from \"{Filepath}\": {exception.Message}");
				return false;
			}
		}

		public static bool Save(string filepath)
		{
			Filepath = filepath;
			return Save();
		}

		public static bool Save()
		{
			var commands = CommandHandlerList.Global.GetItems();
			Dictionary<string, string> data = new Dictionary<string, string>();
			foreach (var i in commands) {
				if (i.Command.Text == null || i.Command.Text == "")
					continue;
				string shortcut = i.Command.Shortcut.ToString();
				try {
					data.Add(i.Command.Text, shortcut == "Unknown" ? "" : shortcut);
				}
				catch (System.Exception exception) {
					System.Console.WriteLine($"\"{i.Command.Text}\" - " + exception.Message);
				}
			}

			var serializer = new Yuzu.Json.JsonSerializer();
			try {
				using (var stream = File.Create(Filepath)) {
					serializer.ToStream(data, stream);
				}
				System.Console.WriteLine($"Key bindings were successfully saved to \"{Filepath}\"");
				return true;
			}
			catch (System.Exception exception) {
				System.Console.WriteLine($"Can not save key bindings to \"{Filepath}\": {exception.Message}");
				return false;
			}
		}
	}
}

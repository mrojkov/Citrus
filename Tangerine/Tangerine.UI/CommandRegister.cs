using Lime;
using System;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public static class CommandRegister
	{
		private readonly static Dictionary<string, Dictionary<string, ICommand>> commands = new Dictionary<string, Dictionary<string, ICommand>> {
			{ "All", new Dictionary<string, ICommand>() }
		};

		public static void Register(string category, string id, ICommand command, bool @override = false)
		{
			if (!commands.ContainsKey(category)) {
				commands.Add(category, new Dictionary<string, ICommand>());
			}
			var commandsCategory = commands[category];
			if (commands["All"].ContainsKey(id)) {
				if (!@override) {
					throw new ArgumentException($"Command with id:'{id}' has already been registered. Use @override=true to override previous command", nameof(id));
				}
				if (commandsCategory.ContainsKey(id)) {
					commandsCategory[id] = command;
				} else {
					commandsCategory.Add(id, command);
				}
				commands["All"][id] = command;
				return;
			}
			commandsCategory.Add(id, command);
			commands["All"].Add(id, command);
		}

		public static void Register(string category, ICommand command, bool @override = false)
		{
			Register(category, command.Text, command, @override);
		}

		public static void Unregister(string id)
		{
			if (commands.ContainsKey(id)) {
				commands.Remove(id);
			}
		}

		public static void Unregister(ICommand command)
		{
			Unregister(command.Text);
		}

		public static bool TryGetCommand(string category, string id, out ICommand command)
		{
			command = null;
			return
				commands.TryGetValue(category, out Dictionary<string, ICommand> commandsCategory) &&
				commandsCategory.TryGetValue(id, out command);
		}

		public static ICommand GetCommand(string category, string id)
		{
			if (TryGetCommand(category, id, out ICommand command)) {
				return command;
			}
			throw new ArgumentException($"Command with category:'{category}' and id:'{id}'hasn't been registered");
		}

		public static IEnumerable<KeyValuePair<string, ICommand>> RegisteredPairs(string category)
		{
			foreach (var pair in commands[category]) {
				yield return pair;
			}
		}

		public static IEnumerable<ICommand> RegisteredCommands()
		{
			foreach (var command in commands["All"].Values) {
				yield return command;
			}
		}

		public static IEnumerable<string> RegisteredIds()
		{
			foreach (var id in commands["All"].Keys) {
				yield return id;
			}
		}

		public static IEnumerable<string> RegisteredCategories()
		{
			foreach (var category in commands.Keys) {
				yield return category;
			}
		}
	}
}

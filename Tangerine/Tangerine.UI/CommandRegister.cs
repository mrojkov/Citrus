using Lime;
using System;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public static class CommandExtentions
	{
		public static void RegisterSelf(this ICommand command, bool @override = false)
		{
			CommandRegister.Register(command, @override);
		}

		public static void UnregisterSelf(this ICommand command)
		{
			CommandRegister.Unregister(command);
		}
	}

	public static class CommandRegister
	{
		private readonly static Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

		public static void Register(string id, ICommand command, bool @override = false)
		{
			if (commands.ContainsKey(id)) {
				if (!@override) {
					throw new ArgumentException($"Command with id:'{id}' has already been registered. Use @override=true to override previous command", nameof(id));
				}
				commands[id] = command;
				return;
			}
			commands.Add(id, command);
		}

		public static void Register(ICommand command, bool @override = false)
		{
			Register(command.Text, command, @override);
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

		public static bool TryGetCommand(string id, out ICommand command)
		{
			return commands.TryGetValue(id, out command);
		}

		public static ICommand GetCommand(string id)
		{
			if (TryGetCommand(id, out ICommand command)) {
				return command;
			}
			throw new ArgumentException($"Command with id:'{id}'hasn't been registered");
		}

		public static IEnumerable<KeyValuePair<string, ICommand>> RegisteredPairs()
		{
			foreach (var pair in commands) {
				yield return pair;
			}
		}

		public static IEnumerable<ICommand> RegisteredCommands()
		{
			foreach (var command in commands.Values) {
				yield return command;
			}
		}

		public static IEnumerable<string> RegisteredIds()
		{
			foreach (var id in commands.Keys) {
				yield return id;
			}
		}
	}
}

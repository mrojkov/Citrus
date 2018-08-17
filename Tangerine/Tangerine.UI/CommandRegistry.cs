using Lime;
using System;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public class CommandCategoryInfo
	{
		public string Id { get; private set; }
		public string Title { get; private set; }
		public Dictionary<string, CommandInfo> Commands { get; private set; } = new Dictionary<string, CommandInfo>();

		public CommandCategoryInfo(string id, string title = null)
		{
			Id = id;
			Title = String.IsNullOrEmpty(title) ? $"{{{id}}}" : title;
		}
	}

	public class CommandInfo
	{
		public ICommand Command { get; private set; }
		public CommandCategoryInfo CategoryInfo { get; private set; }
		public string Id { get; private set; }
		public string Title { get; private set; }
		public Shortcut Shortcut { get; set ; }

		public CommandInfo(ICommand command, CommandCategoryInfo categoryInfo, string id)
		{
			Command = command;
			CategoryInfo = categoryInfo;
			Id = id;
			Title = command.Text ?? id;
		}
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public static class CommandRegistry
	{
		private readonly static Dictionary<string, CommandCategoryInfo> categories = new Dictionary<string, CommandCategoryInfo>();

		public readonly static CommandCategoryInfo AllCommands = new CommandCategoryInfo("All");

		public static void Register(ICommand command, string categoryId, string categoryTitle, string commandId, bool @override = false)
		{
			if (!categories.ContainsKey(categoryId)) {
				categories.Add(categoryId, new CommandCategoryInfo(categoryId, categoryTitle));
			}
			var categoryInfo = categories[categoryId];
			var commandInfo = new CommandInfo(command, categoryInfo, commandId);
			if (AllCommands.Commands.ContainsKey(commandId)) {
				if (!@override) {
					throw new ArgumentException($"Command with id:'{commandId}' has already been registered. Use @override=true to override previous command", nameof(commandId));
				}
				if (categoryInfo.Commands.ContainsKey(commandId)) {
					categoryInfo.Commands[commandId] = commandInfo;
				} else {
					categoryInfo.Commands.Add(commandId, commandInfo);
				}
				AllCommands.Commands[commandId] = commandInfo;
				return;
			}
			categoryInfo.Commands.Add(commandId, commandInfo);
			AllCommands.Commands.Add(commandId, commandInfo);
		}

		public static void Unregister(string id)
		{
			if (categories.ContainsKey(id)) {
				categories.Remove(id);
			}
		}

		public static void Unregister(ICommand command)
		{
			Unregister(command.Text);
		}

		public static bool TryGetCommandCategoryInfo(string categoryId, out CommandCategoryInfo categoryInfo)
		{
			return categories.TryGetValue(categoryId, out categoryInfo);
		}

		public static CommandCategoryInfo GetCommandCategoryInfo(string categoryId)
		{
			if (TryGetCommandCategoryInfo(categoryId, out CommandCategoryInfo categoryInfo)) {
				return categoryInfo;
			}
			throw new ArgumentException($"Category with id:'{categoryId}' hasn't been registrered");
		}

		public static bool TryGetCommand(string categoryId, string commandId, out ICommand command)
		{
			if (TryGetCommandCategoryInfo(categoryId, out CommandCategoryInfo categoryInfo)) {
				return TryGetCommand(categoryInfo, commandId, out command);
			}
			command = null;
			return false;
		}

		public static bool TryGetCommand(CommandCategoryInfo categoryInfo, string commandId, out ICommand command)
		{
			if (categoryInfo.Commands.TryGetValue(commandId, out CommandInfo commandInfo)) {
				command = commandInfo.Command;
				return true;
			}
			command = null;
			return false;
		}

		public static bool TryGetCommand(string commandId, out ICommand command)
		{
			return TryGetCommand(AllCommands, commandId, out command);
		}

		public static ICommand GetCommand(CommandCategoryInfo categoryInfo, string commandId)
		{
			if (TryGetCommand(categoryInfo, commandId, out ICommand command)) {
				return command;
			}
			throw new ArgumentException($"Command with category:'{categoryInfo.Id}' and id:'{commandId}' hasn't been registered");
		}

		public static ICommand GetCommand(string categoryId, string commandId)
		{
			return GetCommand(categories[categoryId], commandId);
		}

		public static ICommand GetCommand(string commandId)
		{
			return GetCommand(AllCommands, commandId);
		}

		public static bool TryGetCommandInfo(string categoryId, string commandId, out CommandInfo commandInfo)
		{
			if (categories.TryGetValue(categoryId, out CommandCategoryInfo categoryInfo)) {
				return TryGetCommandInfo(categoryInfo, commandId, out commandInfo);
			}
			commandInfo = null;
			return false;
		}

		public static bool TryGetCommandInfo(CommandCategoryInfo categoryInfo, string commandId, out CommandInfo commandInfo)
		{
			return categoryInfo.Commands.TryGetValue(commandId, out commandInfo);
		}

		public static bool TryGetCommandInfo(string commandId, out CommandInfo commandInfo)
		{
			return TryGetCommandInfo(AllCommands, commandId, out commandInfo);
		}

		public static CommandInfo GetCommandInfo(CommandCategoryInfo categoryInfo, string commandId)
		{
			if (TryGetCommandInfo(categoryInfo, commandId, out CommandInfo commandInfo)) {
				return commandInfo;
			}
			throw new ArgumentException($"Command with category:'{categoryInfo.Id}' and id:'{commandId}' hasn't been registered");
		}

		public static CommandInfo GetCommandInfo(string categoryId, string commandId)
		{
			return GetCommandInfo(categories[categoryId], commandId);
		}

		public static CommandInfo GetCommandInfo(string commandId)
		{
			return GetCommandInfo(AllCommands, commandId);
		}

		public static IEnumerable<CommandInfo> RegisteredCommandInfo(CommandCategoryInfo categoryInfo)
		{
			foreach (var commandInfo in categoryInfo.Commands.Values) {
				yield return commandInfo;
			}
		}

		public static IEnumerable<CommandInfo> RegisteredCommandInfo(string categoryId) =>
			RegisteredCommandInfo(categories[categoryId]);

		public static IEnumerable<CommandInfo> RegisteredCommandInfo() =>
			RegisteredCommandInfo(AllCommands);

		public static IEnumerable<ICommand> RegisteredCommands(CommandCategoryInfo categoryInfo)
		{
			foreach (var commandInfo in categoryInfo.Commands.Values) {
				yield return commandInfo.Command;
			}
		}

		public static IEnumerable<ICommand> RegisteredCommands(string category) =>
			RegisteredCommands(categories[category]);

		public static IEnumerable<ICommand> RegisteredCommands() =>
			RegisteredCommands(AllCommands);

		public static IEnumerable<string> RegisteredIds(CommandCategoryInfo categoryInfo)
		{
			foreach (var id in categoryInfo.Commands.Keys) {
				yield return id;
			}
		}

		public static IEnumerable<string> RegisteredIds(string category) =>
			RegisteredIds(categories[category]);

		public static IEnumerable<string> RegisteredIds() =>
			RegisteredIds(AllCommands);


		public static IEnumerable<CommandCategoryInfo> RegisteredCategories()
		{
			foreach (var categoryInfo in categories.Values) {
				yield return categoryInfo;
			}
		}
	}
}

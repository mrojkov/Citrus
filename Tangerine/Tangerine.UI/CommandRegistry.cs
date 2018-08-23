using Lime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tangerine.UI
{
	public class CommandCategoryInfo
	{
		public string Id { get; }
		public string Title { get; }
		public Dictionary<string, CommandInfo> Commands { get; } = new Dictionary<string, CommandInfo>();

		public CommandCategoryInfo(string id)
		{
			Id = id;
			Title = Regex.Replace(id, @"(\S)(\p{Lu}|\d)", "$1 $2");
		}
	}

	public class CommandInfo
	{
		public ICommand Command { get; }
		public CommandCategoryInfo CategoryInfo { get; }
		public string Id { get; }
		public string Title { get; }
		public Shortcut Shortcut { get; set; }

		public CommandInfo(ICommand command, CommandCategoryInfo categoryInfo, string id)
		{
			Command = command;
			CategoryInfo = categoryInfo;
			Id = id;
			Title = string.IsNullOrEmpty(command.Text) ? Regex.Replace(id, @"(\S)(\p{Lu}|\d)", "$1 $2") : command.Text;
		}
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public static class CommandRegistry
	{
		private static readonly Dictionary<string, CommandCategoryInfo> categories = new Dictionary<string, CommandCategoryInfo>();

		public static readonly CommandCategoryInfo AllCommands = new CommandCategoryInfo("All");

		public static void Register(ICommand command, string categoryId, string commandId, bool @override = false)
		{
			if (!categories.ContainsKey(categoryId)) {
				categories.Add(categoryId, new CommandCategoryInfo(categoryId));
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

		public static bool TryGetCommandInfo(CommandCategoryInfo categoryInfo, string commandId, out CommandInfo commandInfo)
		{
			return categoryInfo.Commands.TryGetValue(commandId, out commandInfo);
		}

		public static bool TryGetCommandInfo(string commandId, out CommandInfo commandInfo)
		{
			return TryGetCommandInfo(AllCommands, commandId, out commandInfo);
		}

		public static IEnumerable<CommandInfo> RegisteredCommandInfo(CommandCategoryInfo categoryInfo)
		{
			foreach (var commandInfo in categoryInfo.Commands.Values) {
				yield return commandInfo;
			}
		}

		public static IEnumerable<ICommand> RegisteredCommands()
		{
			foreach (var commandInfo in AllCommands.Commands.Values) {
				yield return commandInfo.Command;
			}
		}

		public static IEnumerable<CommandCategoryInfo> RegisteredCategories()
		{
			foreach (var categoryInfo in categories.Values) {
				yield return categoryInfo;
			}
		}
	}
}

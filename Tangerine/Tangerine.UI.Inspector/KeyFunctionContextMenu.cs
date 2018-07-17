using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	static class KeyFunctionContextMenu
	{
		public static void Create(KeyFunctionButtonBinding binding)
		{
			var menu = new Menu();
			foreach (KeyFunction keyFunction in Enum.GetValues(typeof(KeyFunction))) {
				menu.Add(new Command(Enum.GetName(typeof(KeyFunction), keyFunction),
					new ChangeKeyFunction(binding, keyFunction).Execute));
			}
			menu.Popup();
		}

		private class ChangeKeyFunction : CommandHandler
		{

			private KeyFunctionButtonBinding binding;
			private KeyFunction keyFunction;

			public ChangeKeyFunction(KeyFunctionButtonBinding binding, KeyFunction keyFunction)
			{
				this.binding = binding;
				this.keyFunction = keyFunction;
			}

			sealed public override void Execute()
			{
				Document.Current.History.DoTransaction(() => {
					binding.SetKeyFunction(keyFunction);
				});
			}
		}

	}
}

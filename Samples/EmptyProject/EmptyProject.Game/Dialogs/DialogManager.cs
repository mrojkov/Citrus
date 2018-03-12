using System.Collections.Generic;
using System.Linq;

namespace EmptyProject.Dialogs
{
	public class DialogManager
	{
		public static DialogManager Instance { get; } = new DialogManager();

		public List<Dialog> ActiveDialogs { get; } = new List<Dialog>();
		public Dialog Top => ActiveDialogs.FirstOrDefault();

		public void Open<T>() where T : Dialog, new()
		{
			Open(new T());
		}

		public void Open<T>(T dialog) where T : Dialog
		{
			dialog.Attach(The.World);
			ActiveDialogs.Add(dialog);
		}

		public void Remove<T>(T dialog) where T : Dialog
		{
			ActiveDialogs.Remove(dialog);
		}
	}
}
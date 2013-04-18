using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus
{
	public interface ICommandBackend : Xwt.Backends.IBackend
	{
		void Realize();
		void OnCommandChanged();
	}

	[Xwt.Backends.BackendType(typeof(ICommandBackend))]
	public class Command : Xwt.XwtComponent
	{
		private string text;
		private bool sensitive;
		private bool visible;
		private string keySequence;
		private Xwt.Widget context;
		private bool realized;
		private List<XwtPlus.MenuItem> menuItems = new List<XwtPlus.MenuItem>();

		public string Text {
			get { return text; }
			set { text = value; Backend.OnCommandChanged(); }
		}

		public bool Sensitive { 
			get { return sensitive; }
			set { sensitive = value; Backend.OnCommandChanged(); }
		}

		public bool Visible
		{
			get { return visible; }
			set { visible = value; Backend.OnCommandChanged(); }
		}

		public string KeySequence {
			get { return keySequence; }
			set { CheckRealized(); keySequence = value; }
		}

		public Xwt.Widget Context {
			get { return context; }
			set { CheckRealized(); context = value; }
		}
		
		public event Action Triggered;

		public IEnumerable<XwtPlus.MenuItem> MenuItems
		{
			get { return menuItems; }
		}

		public Command()
		{
			Sensitive = true;
			Visible = true;
			CommandManager.Add(this);
		}

		public Command(string text, string keySequence) :
			this(text, keySequence, null)
		{
		}

		public Command(string text, string keySequence, Xwt.Widget context)
		{
			Sensitive = true;
			Visible = true;
			Text = text;
			Context = context;
			KeySequence = keySequence;
			CommandManager.Add(this);
		}

		public void Bind(XwtPlus.MenuItem menuItem)
		{
			CheckRealized();
			menuItems.Add(menuItem);
		}

		internal void Realize()
		{
			realized = true;
			Backend.Realize();
		}

		public virtual void OnTriggered()
		{
			if (Triggered != null) {
				Triggered();
			}
		}

		ICommandBackend Backend
		{
			get { return (ICommandBackend)BackendHost.Backend; }
		}

		private void CheckRealized()
		{
			if (realized) {
				throw new InvalidOperationException("Command already realized");
			}
		}
	}
}

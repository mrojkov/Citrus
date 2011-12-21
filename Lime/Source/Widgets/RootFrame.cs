using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class RootFrame : Frame
	{
		public static RootFrame Instance;

		public RootFrame ()
		{
			Instance = this;
		}

		/// <summary>
		/// Dialog user interact with. All dialogs below that should ignore input events.
		/// </summary>
		public Widget ActiveDialog;

		/// <summary>
		/// Widget which holds input focus. Before processing mouse down event you should test whether ActiveWidget == this.
		/// For revoking input focus from button, slider or any other UI control you should nullify ActiveWidget.
		/// </summary>
		public Widget ActiveWidget;

		public override void Update (int delta)
		{
			ActiveDialog = null;
			base.Update (delta);
		}
	}
}

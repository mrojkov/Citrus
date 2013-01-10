using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	class SlotWrapper : QObject
	{
		private Lime.BareEventHandler signalHandler;

		public SlotWrapper(Lime.BareEventHandler signalHandler)
			: base(The.DefaultQtParent)
		{
			this.signalHandler = signalHandler;
		}

		[Q_SLOT]
		public void OnSignal()
		{
			if (signalHandler != null) {
				signalHandler();
			}
		}
	}
}

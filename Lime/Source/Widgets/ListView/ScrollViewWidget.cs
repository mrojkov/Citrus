using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	[TangerineClass(allowChildren: true)]
	public class ScrollViewWidget : Frame
	{
		public ScrollView Behaviour { get; set; }

		public float ScrollPosition
		{
			get { return Behaviour.ScrollPosition; }
			set { Behaviour.ScrollPosition = value; }
		}

		public float MinScrollPosition
		{
			get { return Behaviour.MinScrollPosition; }
		}

		public float MaxScrollPosition
		{
			get { return Behaviour.MaxScrollPosition; }
		}

		public Widget Content
		{
			get { return Behaviour.Content; }
		}

		public ScrollViewWidget()
		{
			Theme.Current.Apply(this);
		}
	}
}
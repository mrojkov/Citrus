using System;
using Lime;

namespace Tangerine.UI
{
	public class WidgetPresenter : IPresenter
	{
		private Widget widget;
		private Action<Widget> onRender;
		
		public WidgetPresenter(Action<Widget> onRender)
		{
			this.onRender = onRender;
		}

		public void OnAssign(Node node)
		{
			this.widget = (Widget)node;
		}

		public void Render()
		{
			onRender(widget);
		}
		
		public IPresenter Clone(Node node)
		{
			return new WidgetPresenter(onRender);
		}
	}
}


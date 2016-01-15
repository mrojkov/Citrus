using System;
using System.Collections.Generic;

namespace Lime
{
	public interface ITheme
	{
		void Apply(Widget widget);
	}

	public class Theme : ITheme
	{
		protected Dictionary<Type, Action<Widget>> Decorators = new Dictionary<Type, Action<Widget>>();

		public static ITheme Current = new DefaultTheme();

		public void Apply(Widget widget)
		{
			Action<Widget> decorator;
			var type = widget.GetType();
			while (type != typeof(Object)) {
				if (Decorators.TryGetValue(type, out decorator)) {
					decorator(widget);
					break;
				}
				type = type.BaseType;
			}
		}
	}
}


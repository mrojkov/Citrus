using System;
using System.Collections.Generic;

namespace Lime
{
	public interface ITheme
	{
		void Apply(Widget widget);
		void Apply(Widget widget, Type type);
	}

	public class Theme : ITheme
	{
		private static Stack<ITheme> stack = new Stack<ITheme>();
		public static ITheme Current = DefaultTheme.Instance;

		protected Dictionary<Type, Action<Widget>> Decorators = new Dictionary<Type, Action<Widget>>();

		public static ThemeScope Push(ITheme newTheme)
		{
			stack.Push(Current);
			Current = newTheme;
			return new ThemeScope();
		}

		public static void Pop()
		{
			Current = stack.Pop();
		}

		public void Apply(Widget widget)
		{
			Apply(widget, widget.GetType());
		}

		public void Apply(Widget widget, Type type)
		{
			Action<Widget> decorator;
			if (Decorators.TryGetValue(type, out decorator)) {
				decorator(widget);
			}
		}

		public struct ThemeScope : IDisposable
		{
			public void Dispose()
			{
				Theme.Pop();
			}
		}
	}
}


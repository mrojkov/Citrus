using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace XwtPlus.WPFBackend
{
	class CommandBackend : ICommandBackend
	{
		Command frontend;
		CommandBinding binding;
		RoutedCommand routedCommand;

		public void InitializeBackend(object frontend, Xwt.Backends.ApplicationContext context)
		{
			this.frontend = frontend as XwtPlus.Command;
			routedCommand = new RoutedCommand();
			binding = new CommandBinding(routedCommand);
			binding.Executed += binding_Executed;
			binding.CanExecute += binding_CanExecute;
		}

		void binding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = frontend.Visible && frontend.Sensitive;
		}

		public void Realize()
		{
			if (frontend.Context != null) {
				BindToWidget(frontend.Context);
			} else {
				foreach (var w in System.Windows.Application.Current.Windows) {
					AddCommandBinding(w as System.Windows.Window);
				}
			}
			foreach (var mi in frontend.MenuItems) {
				AttachToMenu(mi);
			}
		}

		private void BindToWidget(Xwt.Widget widget)
		{
			var uiElement = widget.Surface.NativeWidget as System.Windows.UIElement;
			AddCommandBinding(uiElement);
		}

		private void AddCommandBinding(System.Windows.UIElement uiElement)
		{
			var gesture = KeyGestureFromString(frontend.KeySequence);
			routedCommand.InputGestures.Clear();
			routedCommand.InputGestures.Add(gesture);
			uiElement.CommandBindings.Add(binding);
		}

		private void AttachToMenu(XwtPlus.MenuItem menuItem)
		{
			SetMenuAcceleratorString(menuItem);
			menuItem.Clicked += menuItem_Clicked;
			OnCommandChanged();
		}

		void menuItem_Clicked(object sender, EventArgs e)
		{
			frontend.OnTriggered();
		}

		private void SetMenuAcceleratorString(XwtPlus.MenuItem menuItem)
		{
			var wpfMenuItem = menuItem.Backend.GetNativeMenuItem() as System.Windows.Controls.MenuItem;
			wpfMenuItem.InputGestureText = frontend.KeySequence;
		}

		public void OnCommandChanged()
		{
			foreach (var item in frontend.MenuItems) {
				item.Label = frontend.Text;
				item.Visible = frontend.Visible;
				item.Sensitive = frontend.Sensitive;
			}
		}

		void Xwt.Backends.IBackend.EnableEvent(object eventId)
		{
		}

		void Xwt.Backends.IBackend.DisableEvent(object eventId)
		{
		}

		private void binding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			frontend.OnTriggered();
		}

		private static KeyGesture KeyGestureFromString(string keySequence)
		{
			var converter = new KeyGestureConverter();
			return converter.ConvertFromString(keySequence) as KeyGesture;
		}

		private static string KeyGestureToString(KeyGesture gesture)
		{
			var converter = new KeyGestureConverter();
			return converter.ConvertToString(gesture);
		}
	}
}

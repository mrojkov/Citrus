using System;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class InstancePropertyEditor<T> : CommonPropertyEditor<T>
	{
		public DropDownList Selector { get; }

		public InstancePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(Selector);
			var propertyType = typeof(T);
			// TODO: cache derived types if this somehow proves to be slow enough to notice
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				var assignables = assembly
					.GetTypes()
					.Where(t =>
						!t.IsInterface &&
						t.GetCustomAttribute<TangerineIgnoreAttribute>(false) == null &&
						t != propertyType &&
						propertyType.IsAssignableFrom(t));
				foreach (var type in assignables) {
					Selector.Items.Add(new CommonDropDownList.Item(type.Name, type));
				}
			}
			Selector.Changed += a => {
				if (a.ChangedByUser) {
					Type type = (Type)Selector.Items[a.Index].Value;
					SetProperty(Activator.CreateInstance(type));
				}
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(), v => Selector.Value = v.GetType());
		}
	}
}

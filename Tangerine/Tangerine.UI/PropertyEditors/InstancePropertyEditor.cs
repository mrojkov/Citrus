using System;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class InstancePropertyEditor<T> : ExpandablePropertyEditor<T>
	{
		public DropDownList Selector { get; }

		public InstancePropertyEditor(IPropertyEditorParams editorParams, Action<Widget> OnValueChanged) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(Selector);
			var propertyType = typeof(T);
			var meta = Yuzu.Metadata.Meta.Get(editorParams.Type, Serialization.DefaultYuzuCommonOptions);
			var propertyMetaItem = meta.Items.Where(i => i.Name == editorParams.PropertyName).First();
			var defaultValue = propertyMetaItem.GetValue(meta.Default);
			var resetToDefaultButton = new ToolbarButton(IconPool.GetTexture("Tools.Revert"));
			resetToDefaultButton.Clicked = () => {
				SetProperty(defaultValue);
			};
			ContainerWidget.AddNode(resetToDefaultButton);
			if (!propertyType.IsInterface) {
				Selector.Items.Add(new CommonDropDownList.Item(propertyType.Name, propertyType));
			}
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
					SetProperty(() => type != null ? Activator.CreateInstance(type) : null);
				}
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(), v => {
				OnValueChanged?.Invoke(ExpandableContent);
				resetToDefaultButton.Visible = !Equals(v, defaultValue);
				Selector.Value = v?.GetType();
			});
		}
	}
}

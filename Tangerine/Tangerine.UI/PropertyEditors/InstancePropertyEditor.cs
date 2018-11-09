using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class InstancePropertyEditor<T> : ExpandablePropertyEditor<T>
	{
		public DropDownList Selector { get; }

		private static Dictionary<Type, List<Type>> derivedTypesCache = new Dictionary<Type,List<Type>>();

		public InstancePropertyEditor(IPropertyEditorParams editorParams, Action<Widget> OnValueChanged) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(Selector);
			var propertyType = typeof(T);
			var meta = Yuzu.Metadata.Meta.Get(editorParams.Type, Serialization.YuzuCommonOptions);
			var propertyMetaItem = meta.Items.Where(i => i.Name == editorParams.PropertyName).FirstOrDefault();
			if (propertyMetaItem != null) {
				var defaultValue = propertyMetaItem.GetValue(meta.Default);
				var resetToDefaultButton = new ToolbarButton(IconPool.GetTexture("Tools.Revert")) {
					Clicked = () => { SetProperty(defaultValue); }
				};
				EditorContainer.AddNode(resetToDefaultButton);
				Selector.AddChangeWatcher(CoalescedPropertyValue(), v => {
					resetToDefaultButton.Visible = !Equals(v.Value, defaultValue);
				});
			}
			if (!propertyType.IsInterface) {
				Selector.Items.Add(new CommonDropDownList.Item(propertyType.Name, propertyType));
			}
			// TODO: invalidate cache on loading new assemblies at runtime
			if (!derivedTypesCache.TryGetValue(propertyType, out List<Type> derivedTypes)) {
				derivedTypes = new List<Type>();
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
					var assignables = assembly
						.GetTypes()
						.Where(t =>
							!t.IsInterface &&
							t.GetCustomAttribute<TangerineIgnoreAttribute>(false) == null &&
							t != propertyType &&
							propertyType.IsAssignableFrom(t));
					foreach (var type in assignables) {
						derivedTypes.Add(type);
					}
				}
				derivedTypesCache.Add(propertyType, derivedTypes);
			}
			foreach (var t in derivedTypes) {
				Selector.Items.Add(new CommonDropDownList.Item(t.Name, t));
			}
			Selector.Changed += a => {
				if (a.ChangedByUser) {
					Type type = (Type)Selector.Items[a.Index].Value;
					SetProperty<object>((_) => type != null ? Activator.CreateInstance(type) : null);
				}
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(
				comparator: (t1, t2) => t1 == null && t2 == null || t1 != null && t2 != null && t1.GetType() == t2.GetType()),
				v => {
				OnValueChanged?.Invoke(ExpandableContent);
				if (v.IsUndefined) {
					Selector.Value = v.Value?.GetType();
				} else {
					Selector.Text = ManyValuesText;
				}
			});
		}
	}
}

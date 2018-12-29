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

		public InstancePropertyEditor(IPropertyEditorParams editorParams, Action<Widget> OnValueChanged) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(Selector);
			var propertyType = typeof(T);
			var meta = Yuzu.Metadata.Meta.Get(editorParams.Type, Serialization.YuzuCommonOptions);
			var propertyMetaItem = meta.Items.FirstOrDefault(i => i.Name == editorParams.PropertyName);
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
			foreach (var t in DerivedTypesCache.GetDerivedTypesFor(propertyType)) {
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
				if (v.IsDefined) {
					Selector.Value = v.Value?.GetType();
				} else {
					Selector.Text = ManyValuesText;
				}
			});
		}

		private static class DerivedTypesCache
		{
			private static Dictionary<Type, HashSet<Type>> cache = new Dictionary<Type, HashSet<Type>>();

			static DerivedTypesCache()
			{
				Project.Opening += _ => cache.Clear();
			}

			public static IEnumerable<Type> GetDerivedTypesFor(Type propertyType)
			{
				if (!cache.TryGetValue(propertyType, out HashSet<Type> derivedTypes)) {
					cache.Add(propertyType, derivedTypes = new HashSet<Type>());
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
						var types = assembly
							.GetTypes()
							.Where(t =>
								!t.IsInterface &&
								t.GetCustomAttribute<TangerineIgnoreAttribute>(false) == null &&
								t != propertyType &&
								propertyType.IsAssignableFrom(t)).ToList();
							foreach (var type in types) {
								derivedTypes.Add(type);
							}
					}
				}
				return derivedTypes;
			}
		}
	}
}

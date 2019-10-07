using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;
using Exception = Lime.Exception;

namespace Tangerine.UI
{
	public class InstancePropertyEditor<T> : ExpandablePropertyEditor<T>
	{
		private ToolbarButton resetToDefaultButton;
		public DropDownList Selector { get; }

		public InstancePropertyEditor(IPropertyEditorParams editorParams, Action<Widget> OnValueChanged) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			var propertyType = typeof(T);
			var meta = Yuzu.Metadata.Meta.Get(editorParams.Type, Serialization.YuzuCommonOptions);
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
			EditorContainer.AddChangeLateWatcher(CoalescedPropertyValue(
				comparator: (t1, t2) => t1 == null && t2 == null || t1 != null && t2 != null && t1.GetType() == t2.GetType()),
				v => {
					OnValueChanged?.Invoke(ExpandableContent);
					if (v.IsDefined) {
						Selector.Value = v.Value?.GetType();
					} else {
						Selector.Text = ManyValuesText;
					}
				}
			);

			var propertyMetaItem = meta.Items.FirstOrDefault(i => i.Name == editorParams.PropertyName);
			object defaultValue = null;
			if (propertyMetaItem != null) {
				defaultValue = propertyMetaItem.GetValue(meta.Default);
				resetToDefaultButton = new ToolbarButton(IconPool.GetTexture("Tools.Revert")) {
					Clicked = () => SetProperty(defaultValue)
				};
				if (Selector.Items.Count == 1) {
					var t = Selector.Items[0].Value as Type;
					var b = new ToolbarButton("Create") {
						TabTravesable = new TabTraversable(),
						LayoutCell = new LayoutCell(Alignment.LeftCenter),
						Padding = new Thickness(left: 5.0f),
						HitTestTarget = true,
						MinWidth = 0,
						MaxWidth = float.PositiveInfinity
					};
					b.Clicked = () => {
						b.Visible = false;
						SetProperty<object>(_ => t != null ? Activator.CreateInstance(t) : null);
						OnValueChanged?.Invoke(ExpandableContent);
						Expanded = true;
					};
					var value = CoalescedPropertyValue().GetValue();
					b.Visible = Equals(value.Value, defaultValue);
					resetToDefaultButton.Clicked = () => {
						b.Visible = true;
						SetProperty(defaultValue);
						OnValueChanged?.Invoke(ExpandableContent);
					};
					EditorContainer.AddNode(b);
					EditorContainer.AddNode(Spacer.HStretch());
					OnValueChanged?.Invoke(ExpandableContent);
				} else {
					EditorContainer.Nodes.Insert(0, Selector);
				}
				EditorContainer.AddNode(resetToDefaultButton);
				Selector.AddChangeLateWatcher(CoalescedPropertyValue(), v => {
					resetToDefaultButton.Visible = !Equals(v.Value, defaultValue);
				});
			} else if (Selector.Items.Count == 1) {
				EditorContainer.Nodes.Insert(0, new ThemedSimpleText {
					Text = Selector.Items[0].Text,
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.LeftCenter),
					ForceUncutText = false,
					Padding = new Thickness(left: 5.0f),
					HitTestTarget = true,
					TabTravesable = new TabTraversable()
				});
				OnValueChanged?.Invoke(ExpandableContent);
			} else {
				EditorContainer.Nodes.Insert(0, Selector);
			}
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			Selector.Enabled = Enabled;
			if (resetToDefaultButton != null) {
				resetToDefaultButton.Enabled = Enabled;
			}
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
						try {
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
						} catch (ReflectionTypeLoadException e) {
							Debug.Write($"Failed to enumerate types in '{assembly.FullName}'");
						}
					}
				}
				return derivedTypes;
			}
		}
	}
}

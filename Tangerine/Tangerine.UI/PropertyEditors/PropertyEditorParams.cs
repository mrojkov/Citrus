using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class PropertyEditorParams : IPropertyEditorParams, IPropertyEditorParamsInternal
	{
		private PropertySetterDelegate propertySetter;
		private Func<NumericEditBox> numericEditBoxFactory;

		public bool ShowLabel { get; set; } = true;
		public Widget InspectorPane { get; set; }
		public IEnumerable<object> RootObjects { get; set; }
		public IEnumerable<object> Objects { get; set; }
		public Type Type { get; set; }
		public string PropertyName { get; set; }
		public string PropertyPath { get; set; }
		public string DisplayName { get; set; }
		public TangerineKeyframeColorAttribute TangerineAttribute { get; set; }
		public string Group { get; set; }
		public System.Reflection.PropertyInfo PropertyInfo { get; set; }

		public Func<NumericEditBox> NumericEditBoxFactory
		{
			get => numericEditBoxFactory;
			set {
				numericEditBoxFactory = () => {
					var attr = PropertyAttributes<TangerineNumericEditBoxStepAttribute>.Get(PropertyInfo);
					var editBox = value();
					attr?.SetProperty(editBox);
					return editBox;
				};
			}
		}

		public Func<EditBox> EditBoxFactory { get; set; }
		public Func<DropDownList> DropDownListFactory { get; set; }
		public Func<object> DefaultValueGetter { get; set; }
		public ITransactionalHistory History { get; set; }
		PropertySetterDelegate IPropertyEditorParamsInternal.PropertySetter => propertySetter;
		public PropertySetterDelegate PropertySetter { set => propertySetter = value; }
		public float LabelWidth { get; set; } = 140;
		public int IndexInList { get; set; } = -1;
		public bool Editable { get; set; }
		public bool IsAnimableByPath { get; set; }
		public bool IsAnimable => RootObjects.All(a => a is IAnimationHost) &&
			PropertyAttributes<TangerineStaticPropertyAttribute>.Get(PropertyInfo) == null &&
			AnimatorRegistry.Instance.Contains(PropertyInfo.PropertyType) &&
			IsAnimableByPath;

		public PropertyEditorParams(Widget inspectorPane, IEnumerable<object> objects, IEnumerable<object> rootObjects, Type type, string propertyName, string propertyPath)
		{
			PropertySetter = SetProperty;
			InspectorPane = inspectorPane;
			Editable = Editable;
			Objects = objects;
			RootObjects = rootObjects;
			Type = type;
			PropertyName = propertyName;
			PropertyPath = propertyPath;
			TangerineAttribute = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(Type, PropertyName) ?? new TangerineKeyframeColorAttribute(0);
			Group = PropertyAttributes<TangerineGroupAttribute>.Get(Type, PropertyName)?.Name ?? String.Empty;
			PropertyInfo = Type.GetProperty(PropertyName);
			NumericEditBoxFactory = () => new ThemedNumericEditBox();
			DropDownListFactory = () => new ThemedDropDownList();
			EditBoxFactory = () => new ThemedEditBox();
		}

		public PropertyEditorParams(Widget inspectorPane, object obj, string propertyName, string propertyPath = null, string displayName = null)
			: this(inspectorPane, new [] { obj }, new [] { obj }, obj.GetType(), propertyName, propertyPath ?? propertyName)
		{
			DisplayName = displayName;
		}

		private void SetProperty(object obj, string propertyName, object value) => PropertyInfo.SetValue(obj, value);
	}

	public class PreferencesPropertyEditorParams : PropertyEditorParams
	{
		public PreferencesPropertyEditorParams(Widget inspectorPane, object obj, string propertyName, string propertyPath = null, string displayName = null) : base(inspectorPane, obj, propertyName, propertyPath, displayName)
		{
			LabelWidth = 200;
		}

		public PreferencesPropertyEditorParams(Widget inspectorPane, IEnumerable<object> objects, IEnumerable<object> rootObjects, Type type, string propertyName, string propertyPath) : base(inspectorPane, objects, rootObjects, type, propertyName, propertyPath)
		{
			LabelWidth = 200;
		}
	}
}

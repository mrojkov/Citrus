using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class PropertyEditorParams : IPropertyEditorParams
	{
		public bool ShowLabel { get; set; } = true;
		public Widget InspectorPane { get; set; }
		public List<object> Objects { get; set; }
		public Type Type { get; set; }
		public string PropertyName { get; set; }
		public string DisplayName { get; set; }
		public TangerineKeyframeColorAttribute TangerineAttribute { get; set; }
		public string Group { get; set; }
		public System.Reflection.PropertyInfo PropertyInfo { get; set; }
		public Func<NumericEditBox> NumericEditBoxFactory { get; set; }
		public Func<EditBox> EditBoxFactory { get; set; }
		public Func<DropDownList> DropDownListFactory { get; set; }
		public Func<object> DefaultValueGetter { get; set; }
		public PropertySetterDelegate PropertySetter { get; set; }
		public ITransactionalHistory History { get; set; }

		public PropertyEditorParams(Widget inspectorPane, List<object> objects, Type type, string propertyName)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			Type = type;
			PropertyName = propertyName;
			TangerineAttribute = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(Type, PropertyName) ?? new TangerineKeyframeColorAttribute(0);
			Group = PropertyAttributes<TangerineGroupAttribute>.Get(Type, PropertyName)?.Name ?? String.Empty;
			PropertyInfo = Type.GetProperty(PropertyName);
			PropertySetter = SetProperty;
			NumericEditBoxFactory = () => new ThemedNumericEditBox();
			DropDownListFactory = () => new ThemedDropDownList();
			EditBoxFactory = () => new ThemedEditBox();
		}

		public PropertyEditorParams(Widget inspectorPane, object obj, string propertyName, string displayName = null)
			: this(inspectorPane, new List<object> { obj }, obj.GetType(), propertyName)
		{
			DisplayName = displayName;
		}

		private void SetProperty(object obj, string propertyName, object value) => PropertyInfo.SetValue(obj, value);
	}
}

using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public delegate void PropertySetterDelegate(object obj, string propertyName, object value);

	public interface IPropertyEditorParams
	{
		Widget InspectorPane { get; set; }
		IEnumerable<object> Objects { get; set; }
		Type Type { get; set; }
		string PropertyName { get; set; }
		string DisplayName { get; set; }
		bool ShowLabel { get; set; }
		TangerineKeyframeColorAttribute TangerineAttribute { get; set; }
		System.Reflection.PropertyInfo PropertyInfo { get; set; }
		Func<NumericEditBox> NumericEditBoxFactory { get; set; }
		Func<DropDownList> DropDownListFactory { get; set; }
		Func<EditBox> EditBoxFactory { get; set; }
		Func<object> DefaultValueGetter { get; set; }
		PropertySetterDelegate PropertySetter { get; set; }
		ITransactionalHistory History { get; set; }
	}
}

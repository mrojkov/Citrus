using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AlignmentPropertyEditor : CommonPropertyEditor<Alignment>
	{
		private DropDownList selectorH { get; }
		private DropDownList selectorV { get; }

		public AlignmentPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(selectorH = editorParams.DropDownListFactory()),
					(selectorV = editorParams.DropDownListFactory())
				}
			});
			var items = new [] {
				(type: typeof(HAlignment), selector: selectorH),
				(type: typeof(VAlignment), selector: selectorV)
			};
			foreach (var (type, selector) in items) {
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
				var allowedFields = fields.Where(f => !Attribute.IsDefined((MemberInfo)f, typeof(TangerineIgnoreAttribute)));
				foreach (var field in allowedFields) {
					selector.Items.Add(new CommonDropDownList.Item(field.Name, field.GetValue(null)));
				}
				selector.Changed += a => {
					if (a.ChangedByUser) {
						SetComponent(editorParams, type);
					}
				};
			}
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			selectorH.AddChangeWatcher(currentX, v => selectorH.Value = v);
			selectorV.AddChangeWatcher(currentY, v => selectorV.Value = v);
		}

		void SetComponent(IPropertyEditorParams editorParams, Type t)
		{
			DoTransaction(() => {
				foreach (var obj in editorParams.Objects) {
					var current = new Property<Alignment>(obj, editorParams.PropertyName).Value;
					if (t == typeof(HAlignment)) {
						current.X = (HAlignment)selectorH.Value;
					} else if (t == typeof(VAlignment)) {
						current.Y = (VAlignment)selectorV.Value;
					}
					editorParams.PropertySetter(obj, editorParams.PropertyName, current);
				}
			});
		}

		public override void Submit()
		{
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			SetComponent(EditorParams, typeof(HAlignment));
			SetComponent(EditorParams, typeof(VAlignment));
		}
	}
}

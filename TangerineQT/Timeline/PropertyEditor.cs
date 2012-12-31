using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine
{
	public static class PropertyEditorRegistry
	{
		public static PropertyEditor CreateEditor(Lime.Node node, PropertyInfo property)
		{
			PropertyEditor e = null;
			if (property.PropertyType == typeof(Lime.Vector2)) {
				e = new PropertyEditorForVector2();
			} else {
				throw new NotImplementedException();
			}
			e.Node = node;
			e.PropertyInfo = property;
			return e;
		}
	}

	public abstract class PropertyEditor : QObject
	{
		public Lime.Node Node;
		public PropertyInfo PropertyInfo;

		public abstract void StartEditing();

		public abstract void CreateWidgets(QBoxLayout layout);

		public virtual void Destroy() { }
	}

	public class PropertyEditorForVector2 : PropertyEditor
	{
		QLabel label;
		InplaceTextEditor editor;

		public override void CreateWidgets(QBoxLayout layout)
		{
			label = new QLabel(GetValueFromProperty());
			label.MouseDoubleClick += label_MouseDoubleClick;
			layout.AddWidget(label);
			layout.AddSpacing(4);
		}

		public override void StartEditing()
		{
			editor = new InplaceTextEditor(label);
			editor.Finished += (text) => {
				text = text ?? "";
				var v = (Lime.Vector2)PropertyInfo.GetValue(Node, null);
				var vals = text.Split(';');
				if (vals.Length == 2) {
					float x, y;
					if (float.TryParse(vals[0], out x)) {
						v.X = x;
					}
					if (float.TryParse(vals[1], out y)) {
						v.Y = y;
					}
					PropertyInfo.SetValue(Node, v, null);
				}
				label.Text = GetValueFromProperty();
				label.Show();
			};
		}

		void label_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		{
			StartEditing();
		}

		private string GetValueFromProperty()
		{
			var v = (Lime.Vector2)PropertyInfo.GetValue(Node, null);
			return string.Format("{0}; {1}", v.X, v.Y);
		}
	}
}
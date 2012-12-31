using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	/// <summary>
	/// Строка представляющая одно свойство нода на таймлайне
	/// </summary>
	public class PropertyLine : AbstractLine
	{
		Lime.Node node;
		PropertyInfo property;
		PropertyEditor editor;

		public PropertyLine(Lime.Node node, PropertyInfo property)
		{
			this.node = node;
			this.property = property;
			editor = PropertyEditorRegistry.CreateEditor(node, property);
		}

		public override void HandleKey(Qt.Key key)
		{
			if (key == Key.Key_Return) {
				editor.StartEditing();
			}
		}

		public override void CreateWidgets()
		{
			base.CreateWidgets();

			layout.AddSpacing(30);
			var label = new QLabel(property.Name);
			label.SetFixedWidth(50);
			layout.AddWidget(label);

			var iconButton = CreateIconButton("Timeline/Interpolation/Spline");
			layout.AddWidget(iconButton, 0);

			editor.CreateWidgets(layout);
		}

		public override void PaintContent(QPainter ptr, int width)
		{
			int numCols = width / ColWidth + 1;
			var c = new KeyTransientCollector(property);
			var tp = new KeyTransientsPainter(ColWidth, Top);
			var transients = c.GetTransients(node);
			tp.DrawTransients(transients, 0, numCols, ptr);
		}
	}
}

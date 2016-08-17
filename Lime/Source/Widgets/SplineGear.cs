using Yuzu;

namespace Lime
{
	/// <summary>
	/// Хранит имя виджета и имя сплайна. Используется для задания траектории движения виджета по сплайну
	/// </summary>
	public class SplineGear : Node
	{
		/// <summary>
		/// Id виджета. Виджет ищется по всей сцене
		/// </summary>
		[YuzuMember]
		public string WidgetId { get; set; }

		/// <summary>
		/// Id сплайна. Сплайн ищется по всей сцене
		/// </summary>
		[YuzuMember]
		public string SplineId { get; set; }

		/// <summary>
		/// Положение виджета на сплайне. 0 - начало сплайна, 1 - конец
		/// </summary>
		[YuzuMember]
		public float SplineOffset { get; set; }

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			var spline = Parent.Nodes.TryFind(SplineId) as Spline;
			var widget = Parent.Nodes.TryFind(WidgetId) as Widget;
			if (spline != null && widget != null) {
				float length = spline.CalcLengthRough();
				Vector2 point = spline.CalcPoint(SplineOffset * length);
				widget.Position = spline.CalcLocalToParentTransform().TransformVector(point);
				widget.Update(0);
			}
		}
	}
}
using System;
using System.Text;
using Lime;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Точка DistortionMesh
	/// Position точки представлен как [0,0]-[1,1] (левый верхний и правый нижний угол DistortionMesh)
	/// </summary>
	[ProtoContract]
	public class DistortionMeshPoint : PointObject
	{
		/// <summary>
		/// Оттенок (при отрисовке цвет текстуры умножается на цвет точки)
		/// </summary>
		[ProtoMember(1)]
		public Color4 Color { get; set; }

		/// <summary>
		/// Текстурные координаты
		/// </summary>
		[ProtoMember(2)]
		public Vector2 UV { get; set; }

		/// <summary>
		/// Смещение точки относительно ее начальной позиции (в пикселях)
		/// </summary>
		[ProtoMember(3)]
		public Vector2 Offset { get; set; }

		public DistortionMeshPoint()
		{
			Color = Color4.White;
		}

		/// <summary>
		/// Возвращает позицию точки + позицию DistortionMesh (в пикселях)
		/// </summary>
		public Vector2 TransformedPosition {
			get {
				Vector2 result = Offset;
				if (Parent != null && Parent.AsWidget != null) {
					result = Parent.AsWidget.Size * Position + Offset;
				}
				if (SkinningWeights != null && Parent != null && Parent.Parent != null) {
					BoneArray a = Parent.Parent.AsWidget.BoneArray;
					Matrix32 m1 = Parent.AsWidget.CalcLocalToParentTransform();
					Matrix32 m2 = m1.CalcInversed();
					result = m2.TransformVector(a.ApplySkinningToVector(m1.TransformVector(result), SkinningWeights));
				}
				return result;
			}
		}
	}
}

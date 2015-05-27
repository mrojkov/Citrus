using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// “очка сплайна
	/// </summary>
	[ProtoContract]
	public class SplinePoint : PointObject
	{
		[ProtoMember(1)]
		public bool Straight { get; set; }

		/// <summary>
		/// ќт угла касательной зависит направление закруглений сплайна
		/// </summary>
		[ProtoMember(2)]
		public float TangentAngle { get; set; }

		/// <summary>
		/// ќт веса касательной зависит радиус закруглений сплайна
		/// </summary>
		[ProtoMember(3)]
		public float TangentWeight { get; set; }

		/// <summary>
		/// ѕоложение точки в контейнере сплайна. (0,0) - левый верхний угол, (1,1) - правый нижний
		/// </summary>
		[ProtoMember(4)]
		public Vector2 Anchor { get; set; }
	}
}
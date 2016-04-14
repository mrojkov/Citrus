using System;
using System.Collections.Generic;
using System.Text;
using Lime;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Точечный объект
	/// </summary>
	[ProtoContract]
	[ProtoInclude(101, typeof(SplinePoint))]
	[ProtoInclude(102, typeof(DistortionMeshPoint))]
	[ProtoInclude(103, typeof(EmitterShapePoint))]
	public class PointObject : Node
	{
		private Vector2 position;

		public PointObject()
		{
			// Just in sake of optimization set Presenter to null because all of PointObjects have empty Render() methods.
			Presenter = null;
		}

		/// <summary>
		/// Позиция объекта. Может быть представлена как в пикселях, так и в нормализованном значении (как именно, зависит от класса-наследника)
		/// </summary>
		[ProtoMember(1)]
		public Vector2 Position { get { return position; } set { position = value; } }

		/// <summary>
		/// Координата X (аналогично Position.X)
		/// </summary>
		public float X { get { return position.X; } set { position.X = value; } }

		/// <summary>
		/// Координата Y (аналогично Position.Y)
		/// </summary>
		public float Y { get { return position.Y; } set { position.Y = value; } }

		/// <summary>
		/// Веса костей (используется для скеленой анимации)
		/// </summary>
		[ProtoMember(2)]
		public SkinningWeights SkinningWeights { get; set; }
	}
}

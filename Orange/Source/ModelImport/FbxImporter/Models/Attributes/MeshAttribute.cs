using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orange.FbxImporter
{
	public class MeshAttribute : NodeAttribute
	{
		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CalcRelativeTransform(IntPtr node);

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetMeshAttribute(IntPtr node);

		#endregion

		public int[] Indices { get; private set; }
		public Mesh3D.Vertex[] Points { get; private set; }
		public Color4[] Colors { get; private set; }

		//TODO: Add Lime Vector3

		public MeshAttribute(IntPtr ptr) : base(ptr)
		{
			//TODO Fix this
			var mesh = GetMeshAttribute(NativePtr).To<MeshData>();
			Indices = mesh.vertices.ToIntArray(mesh.verticesCount);
			Points = new Mesh3D.Vertex[mesh.pointsCount];
			Colors = new Color4[mesh.colorsCount];
			var points = mesh.points.ToDoubleArray(mesh.pointsCount);
			for (int i = 0; i < points.Length * 4; i +=3 ) {
				try {
					Points[i / 3] = new Mesh3D.Vertex {
						Pos = new Vector3(
								(float)points[i],
								(float)points[i + 1],
								(float)points[i + 2])
					};
				} catch(System.Exception e) {
					var j = i;
				}
				
			}

			var colors = mesh.colors.ToDoubleArray(mesh.colorsCount);
			if (colors != null) {
				for (int i = 0; i < colors.Length * 2; i += 4) {
					Colors[i / 4] = Color4.FromFloats(
						(float)colors[i],
						(float)colors[i + 1],
						(float)colors[i + 2],
						(float)colors[i + 3]);
				}
			}
		}	

		//TODO: Add Lime matrix as return type
		public Matrix44 CalcGlobalTransform()
		{
			return CalcRelativeTransform(NativePtr).To<Mat4x4>().ToLime();
		}

		public override string ToString(int level)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Points: ".ToLevel(level + 1));
			if (Points == null) {
				builder.AppendLine("None");
			} else {
				builder.AppendLine("[".ToLevel(level + 2));
				for (int i =0; i < Points.Length; i+=3) {
					builder.AppendLine($"[{Points[i]}, {Points[i + 1]}, {Points[i + 2]}],".ToLevel(level + 3));
				}
				builder.AppendLine("[".ToLevel(level + 2));
			}
			return builder.ToString();
		}

		[StructLayout(LayoutKind.Sequential)]
		private class MeshData
		{
			public IntPtr vertices;

			[MarshalAs(UnmanagedType.I4)]
			public int verticesCount;

			public IntPtr points;

			[MarshalAs(UnmanagedType.I4)]
			public int pointsCount;

			public IntPtr colors;

			[MarshalAs(UnmanagedType.I4)]
			public int colorsCount;

			public IntPtr uvCoords;

			[MarshalAs(UnmanagedType.I4)]
			public int uvCount;

			~MeshData()
			{
				Utils.ReleaseNative(vertices);
				Utils.ReleaseNative(colors);
				Utils.ReleaseNative(uvCoords);
				Utils.ReleaseNative(points);
			}
		}
	}
}

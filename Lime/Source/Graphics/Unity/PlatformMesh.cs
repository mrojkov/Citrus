#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class PlatformMesh : IPlatformMesh
	{
		private Mesh mesh;
		private UnityEngine.Mesh unityMesh;
		private UnityEngine.Vector3[] vertices;
		private UnityEngine.Color32[] colors;
		private UnityEngine.Vector2[] uv1;
		private UnityEngine.Vector2[] uv2;
		private UnityEngine.Vector2[] uv3;
		private UnityEngine.Vector2[] uv4;

		public PlatformMesh(Mesh mesh)
		{
			this.mesh = mesh;
			unityMesh = new UnityEngine.Mesh();
			unityMesh.MarkDynamic();
		}

		public void Render(int startIndex, int count)
		{
			//unityMesh.Clear(true);
			UploadVertices();
			var indices = new int[count];
			Array.Copy(mesh.Indices, startIndex, indices, 0, count);
			unityMesh.triangles = indices;
			PlatformRenderer.SetViewportAndProject();
			UnityEngine.Graphics.DrawMeshNow(unityMesh, UnityEngine.Matrix4x4.identity);
		}

		private void UploadVertices()
		{
			var dm = mesh.DirtyAttributes;
			if (mesh.Vertices != null && (dm & Mesh.Attributes.Vertex) != 0) {
				Array.Resize(ref vertices, mesh.Vertices.Length);
				int i = 0;
				foreach (var v in mesh.Vertices) {
					vertices[i].x = v.X; 
					vertices[i++].y = v.Y;
				}                                         
				unityMesh.vertices = vertices;
			}
			if (mesh.Colors != null && (dm & Mesh.Attributes.Color) != 0) {
				Array.Resize(ref colors, mesh.Colors.Length);
				int i = 0;
				foreach (var v in mesh.Colors) {
					colors[i].r = v.R; 
					colors[i].g = v.G; 
					colors[i].b = v.B; 
					colors[i++].a = v.A; 
				}                                         
				unityMesh.colors32 = colors;
			}
			if (mesh.UV1 != null && (dm & Mesh.Attributes.UV1) != 0) {
				Array.Resize(ref uv1, mesh.UV1.Length);
				int i = 0;
				foreach (var v in mesh.UV1) {
					uv1[i].x = v.X; 
					uv1[i++].y = 1 - v.Y;
				}                                         
				unityMesh.uv = uv1;
			}
			if (mesh.UV2 != null && (dm & Mesh.Attributes.UV2) != 0) {
				Array.Resize(ref uv2, mesh.UV2.Length);
				int i = 0;
				foreach (var v in mesh.UV2) {
					uv2[i].x = v.X; 
					uv2[i++].y = 1 - v.Y;
				}                                         
				unityMesh.uv2 = uv2;
			}
			if (mesh.UV3 != null && (dm & Mesh.Attributes.UV3) != 0) {
				Array.Resize(ref uv3, mesh.UV3.Length);
				int i = 0;
				foreach (var v in mesh.UV3) {
					uv3[i].x = v.X; 
					uv3[i++].y = 1 - v.Y;
				} 
				// Only UNITY 5 supports it
				// unityMesh.uv3 = uv3;
			}
			if (mesh.UV4 != null && (dm & Mesh.Attributes.UV4) != 0) {
				Array.Resize(ref uv4, mesh.UV4.Length);
				int i = 0;
				foreach (var v in mesh.UV4) {
					uv4[i].x = v.X; 
					uv4[i++].y = 1 - v.Y;
				}  
				// Only UNITY 5 supports it
				// unityMesh.uv4 = uv4;
			}
			mesh.DirtyAttributes = Mesh.Attributes.None;
		}

		public void Dispose() {}
	}
}
#endif
#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class PlatformGeometryBuffer : IPlatformGeometryBuffer
	{
		private GeometryBuffer source;
		private UnityEngine.Mesh unityMesh;
		private UnityEngine.Vector3[] vertices;
		private UnityEngine.Color32[] colors;
		private UnityEngine.Vector2[] uv1;
		private UnityEngine.Vector2[] uv2;
		private UnityEngine.Vector2[] uv3;
		private UnityEngine.Vector2[] uv4;

		public PlatformGeometryBuffer(GeometryBuffer mesh)
		{
			this.source = mesh;
			unityMesh = new UnityEngine.Mesh();
			unityMesh.MarkDynamic();
		}

		public void Render(int startIndex, int count)
		{
			MaterialFactory.GetMaterial(
				PlatformRenderer.Blending, 
				PlatformRenderer.ZTest,
				PlatformRenderer.ZWrite,
				PlatformRenderer.Shader,
				PlatformRenderer.Textures[0],
				PlatformRenderer.Textures[1]
			).SetPass(0);
			// unityMesh.Clear(true);
			UploadVertices();
			var indices = new int[count];
			Array.Copy(source.Indices, startIndex, indices, 0, count);
			unityMesh.triangles = indices;
			PlatformRenderer.SetViewportAndProject();
			UnityEngine.Graphics.DrawMeshNow(unityMesh, UnityEngine.Matrix4x4.identity);
		}

		private void UploadVertices()
		{
			var dm = source.DirtyAttributes;
			if (source.Vertices != null && (dm & GeometryBuffer.Attributes.Vertex) != 0) {
				Array.Resize(ref vertices, source.Vertices.Length);
				int i = 0;
				foreach (var v in source.Vertices) {
					vertices[i].x = v.X; 
					vertices[i].y = v.Y;
					vertices[i++].z = v.Z;
				}
				unityMesh.vertices = vertices;
			}
			if (source.Colors != null && (dm & GeometryBuffer.Attributes.Color) != 0) {
				Array.Resize(ref colors, source.Colors.Length);
				int i = 0;
				foreach (var v in source.Colors) {
					colors[i].r = v.R; 
					colors[i].g = v.G; 
					colors[i].b = v.B; 
					colors[i++].a = v.A; 
				}                                         
				unityMesh.colors32 = colors;
			}
			if (source.UV1 != null && (dm & GeometryBuffer.Attributes.UV1) != 0) {
				Array.Resize(ref uv1, source.UV1.Length);
				int i = 0;
				foreach (var v in source.UV1) {
					uv1[i].x = v.X; 
					uv1[i++].y = 1 - v.Y;
				}                                         
				unityMesh.uv = uv1;
			}
			if (source.UV2 != null && (dm & GeometryBuffer.Attributes.UV2) != 0) {
				Array.Resize(ref uv2, source.UV2.Length);
				int i = 0;
				foreach (var v in source.UV2) {
					uv2[i].x = v.X; 
					uv2[i++].y = 1 - v.Y;
				}                                         
				unityMesh.uv2 = uv2;
			}
			if (source.UV3 != null && (dm & GeometryBuffer.Attributes.UV3) != 0) {
				Array.Resize(ref uv3, source.UV3.Length);
				int i = 0;
				foreach (var v in source.UV3) {
					uv3[i].x = v.X; 
					uv3[i++].y = 1 - v.Y;
				} 
				unityMesh.uv3 = uv3;
			}
			if (source.UV4 != null && (dm & GeometryBuffer.Attributes.UV4) != 0) {
				Array.Resize(ref uv4, source.UV4.Length);
				int i = 0;
				foreach (var v in source.UV4) {
					uv4[i].x = v.X; 
					uv4[i++].y = 1 - v.Y;
				}  
				unityMesh.uv4 = uv4;
			}
			source.DirtyAttributes = GeometryBuffer.Attributes.None;
		}

		public void Dispose() {}
	}
}
#endif
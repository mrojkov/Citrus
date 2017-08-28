using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Source.Profilers.NodeProfilerHelpers
{
	public static class ResultExporter
	{
		public static Node CreateCloneForSerialization(Node node, long totalRenderTicks, long totalUpdateTicks, Action<Node> calculateUsageSummary, Action<Node> customOperation = null)
		{
			var clone = node.Clone();
			calculateUsageSummary(clone);
			foreach (var n in clone.Descendants) {
				customOperation?.Invoke(n);
				AlterId(n, totalRenderTicks, totalUpdateTicks);
				RenameBonesInMesh3D(n);
				ResetFoldersAndAnimations(n);
				ReplaceRenderTexturesInNode(n);
			}
			ProcessExternalContent(clone);
			RemoveNodeComponents(clone);
			return clone;
		}

		private static void AlterId(Node node, long totalRenderTicks, long totalUpdateTicks)
		{
			// Add profiler result to the node Id
			var renderUsage = 0f;
			var updateUsage = 0f;
			var ud = node.Components.Get<UsageSummary>();
			if (ud != null) {
				renderUsage = (float)ud.RenderUsage / totalRenderTicks;
				updateUsage = (float)ud.UpdateUsage / totalUpdateTicks;
			}
			if (!(node is Camera3D)) {
				node.Id += $"({renderUsage:P} | {updateUsage:P})";
			}
		}

		private static void RemoveNodeComponents(Node node)
		{
			node.Components.Clear();
			foreach (var n in node.Descendants) {
				n.Components.Clear();
			}
		}

		private static void ProcessExternalContent(Node clone)
		{
			// Models should be external content, others should be inlined
			clone.ContentsPath = null;
			foreach (var n in clone.Descendants.Where(i => !string.IsNullOrEmpty(i.ContentsPath))) {
				if (n is Model3D) {
					n.Nodes.Clear();
					n.Markers.Clear();
				} else {
					n.ContentsPath = null;
				}
			}
		}

		private static void ResetFoldersAndAnimations(Node node)
		{
			node.AnimationFrame = 0;
			if (node.Folders != null && node.Folders.Count == 0) {
				node.Folders = null;
			}
		}

		private static void RenameBonesInMesh3D(Node node)
		{
			// BoneNames property in submeshes should reflect new bones Ids
			var m3d = node as Mesh3D;
			if (m3d != null) {
				foreach (var submesh in m3d.Submeshes) {
					submesh.BoneNames.Clear();
					submesh.Bones.ForEach(b => submesh.BoneNames.Add(b.Id));

					ReplaceRenderTexturesInSubmesh(submesh);
				}
			}
		}

		private static void ReplaceRenderTexturesInSubmesh(Submesh3D submesh)
		{
			// RenderTexture can't be serialized
			var cm = submesh.Material as CommonMaterial;
			if (cm?.DiffuseTexture is RenderTexture) {
				cm.DiffuseTexture = new SerializableTexture();
			}
		}

		private static void ReplaceRenderTexturesInNode(Node node)
		{
			// RenderTexture can't be serialized
			var w = node as Widget;
			if (w?.Texture is RenderTexture) {
				w.Texture = new SerializableTexture();
			}
		}
	}
}

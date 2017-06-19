#if PROFILE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class NodeProfiler
	{
		private sealed class UsageData : NodeComponent
		{
			public long RenderTicks;
			public long UpdateTicks;

			public void Clear()
			{
				RenderTicks = 0;
				UpdateTicks = 0;
			}
		}

		private sealed class UsageSummary : NodeComponent
		{
			public long RenderUsage;
			public long UpdateUsage;
		}

		private static long totalRenderTicks;
		private static long totalUpdateTicks;
		private static readonly List<UsageData> createdComponents = new List<UsageData>();

		internal static void RegisterRender(Node node, long ticks)
		{
			var ud = GetComponent(node);
			ud.RenderTicks += ticks;
			totalRenderTicks += ticks;
		}

		internal static void RegisterUpdate(Node node, long ticks)
		{
			var ud = GetComponent(node);
			ud.UpdateTicks += ticks;
			totalUpdateTicks += ticks;
		}

		private static UsageData GetComponent(Node node)
		{
			var ud = node.Components.Get<UsageData>();
			if (ud == null) {
				ud = new UsageData();
				node.Components.Add(ud);
				createdComponents.Add(ud);
			}
			return ud;
		}

		public static void Reset()
		{
			totalRenderTicks = 0;
			totalUpdateTicks = 0;
			createdComponents.ForEach(ud => ud.Clear());
		}

		public static void DumpWindowWithProfilingInfo(WindowWidget window, string fileName)
		{
			var frame = new Frame();
			foreach (var n in window.Nodes) {
				frame.AddNode(n.Clone());
			}
			DumpNodeWithProfilingInfo(frame, fileName);
		}

		public static void DumpNodeWithProfilingInfo(Node node, string fileName)
		{
			var ms = new MemoryStream();
			using (var clone = CreateCloneForSerialization(node)) {
				Serialization.WriteObject(fileName, ms, clone, Serialization.Format.JSON);
			}
			using (var fs = new FileStream(fileName, FileMode.Create)) {
				var a = ms.ToArray();
				fs.Write(a, 0, a.Length);
			}
		}

		private static Node CreateCloneForSerialization(Node node)
		{
			var clone = node.Clone();
			CalculateUsageSummary(clone);
			RemoveNodeComponents(clone);
			foreach (var n in clone.Descendants) {
				AlterId(n);
				RenameBonesInMesh3D(n);
				ResetFoldersAndAnimations(n);
				ReplaceRenderTexturesInNode(n);
			}
			ProcessExternalContent(clone);
			return clone;
		}

		private static void RemoveNodeComponents(Node node)
		{
			node.Components.Clear();
			foreach (var n in node.Descendants) {
				n.Components.Clear();
			}
		}

		private static Tuple<long, long> CalculateUsageSummary(Node node)
		{
			var ud = node.Components.Get<UsageData>();

			long renderTicks = ud?.RenderTicks ?? 0;
			long updateTicks = ud?.UpdateTicks ?? 0;

			foreach (var ticks in node.Nodes.Select(CalculateUsageSummary)) {
				renderTicks += ticks.Item1;
				updateTicks += ticks.Item2;
			}

			var us = node.Components.GetOrAdd<UsageSummary>();
			us.RenderUsage = renderTicks;
			us.UpdateUsage = updateTicks;

			return new Tuple<long, long>(renderTicks, updateTicks);
		}

		private static void ProcessExternalContent(Node clone)
		{
			// Models should be external content, others should be inlined
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

		private static void AlterId(Node node)
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
#endif
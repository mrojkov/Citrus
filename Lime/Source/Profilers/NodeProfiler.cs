#if PROFILE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime.Source.Profilers.NodeProfilerHelpers;

namespace Lime
{
	public static class NodeProfiler
	{
		public static long TotalRenderTicks { get; private set; }
		public static long TotalUpdateTicks { get; private set; }
		private static readonly List<UsageData> createdComponents = new List<UsageData>();

		internal static void RegisterRender(Node node, long ticks)
		{
			var ud = GetComponent(node);
			ud.RenderTicks += ticks;
			TotalRenderTicks += ticks;
		}

		internal static void RegisterUpdate(Node node, long ticks)
		{
			var ud = GetComponent(node);
			ud.UpdateTicks += ticks;
			TotalUpdateTicks += ticks;
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

		public static Tuple<long, long> CalculateUsageSummary(Node node)
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

		public static void Reset()
		{
			TotalRenderTicks = 0;
			TotalUpdateTicks = 0;
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

		public static void DumpNodeWithProfilingInfo(Node node, string fileName, Action<Node> customOperation = null)
		{
			var ms = new MemoryStream();
			using (
				var clone = ResultExporter.CreateCloneForSerialization(
					node,
					TotalRenderTicks,
					TotalUpdateTicks,
					(n) => {
						CalculateUsageSummary(n);
					},
					customOperation
				)
			) {
				Serialization.WriteObject(fileName, ms, clone, Serialization.Format.JSON);
			}
			using (var fs = new FileStream(fileName, FileMode.Create)) {
				var a = ms.ToArray();
				fs.Write(a, 0, a.Length);
			}
		}

	}
}
#endif
using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core
{
	public static class Utils
	{
		/// <summary>
		/// Reorder bones with topological sort to maintain correct update
		/// order of transformations
		/// </summary>
		public static List<Bone> SortBones(IEnumerable<Bone> bonesCollection, bool reverseOrder = false)
		{
			var bones = new Dictionary<int, Bone>();
			int maxIndex = 0;
			foreach (var bone in bonesCollection) {
				if (bones.ContainsKey(bone.Index)) {
					throw new InvalidOperationException("more than one bone with same index");
				}
				bones[bone.Index] = bone;
				if (bone.Index > maxIndex) {
					maxIndex = bone.Index;
				}
			}
			var visited = new Dictionary<int, bool>();
			var g = new Dictionary<int, List<int>>();
			foreach (var kv in bones) {
				var b = kv.Value;
				(g.ContainsKey(b.BaseIndex) ? g[b.BaseIndex] : g[b.BaseIndex] = new List<int>()).Add(b.Index);
				if (!g.ContainsKey(b.Index))
					g[b.Index] = new List<int>();
				visited.Add(b.Index, false);
				if (!visited.ContainsKey(b.BaseIndex))
					visited.Add(b.BaseIndex, false);
			}
			var orderedIndices = new List<int>();
			Action<int> visit = null;
			visit = (index) => {
				visited[index] = true;
				for (int i = 0; i < g[index].Count; i++) {
					if (visited[g[index][i]]) {
						throw new InvalidOperationException("found cycle in bones parent child relations");
					}
					visit(g[index][i]);
				}
				orderedIndices.Add(index);
			};
			foreach (var kv in g) {
				if (!visited[kv.Key]) {
					visit(kv.Key);
				}
			}
			if (reverseOrder) {
				orderedIndices.Reverse();
			}
			var res = new List<Bone>();
			foreach (var i in orderedIndices) {
				// holes in indices and zero index (implicit bone with Identity transformation)
				if (!bones.ContainsKey(i)) {
					continue;
				}
				res.Insert(0, bones[i]);
			}
			return res;
		}

		public static IEnumerable<Bone> FindBoneDescendats(Bone root, IEnumerable<Bone> bones)
		{
			foreach (var bone in bones.Where(b => b.BaseIndex == root.Index)) {
				yield return bone;
				foreach (var b in FindBoneDescendats(bone, bones)) {
					yield return b;
				}
			}
		}

		public static Bone GetBone(this IEnumerable<Node> nodes, int index)
		{
			foreach (var node in nodes) {
				if (node is Bone && ((Bone)node).Index == index) {
					return node as Bone;
				}
			}
			return null;
		}
	}
}

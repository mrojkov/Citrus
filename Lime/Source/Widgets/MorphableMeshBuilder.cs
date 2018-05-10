//using System;
//using System.Linq;
//using System.Collections.Generic;

//namespace Lime
//{
//	public class MorphableMeshBuilder
//	{
//		[Flags]
//		public enum Options
//		{
//			None = 0,
//			NoParticles = 1
//		}

//		public void BuildNodeContents(Node node, Options options)
//		{
//			var originalNodes = node.Nodes.ToList();
//			node.Nodes.Clear();
//			// A temporary container used for turning node list into a morphable mesh.
//			var container = new Widget();
//			// Fill the container with bones.
//			if (originalNodes.Any(i => i is Bone)) {
//				var bones = originalNodes.Where(i => i is Bone);
//				container.Nodes.AddRange(bones);
//				ReorderBones(container);
//			}
//			int numBones = container.Nodes.Count;
//			int skipConvertionCounter = 0;
//			foreach (var child in originalNodes) {
//				if ((options & Options.NoParticles) != 0 && (child is ParticleEmitter)) {
//					continue;
//				}
//				if (skipConvertionCounter == 0 && CanConvertNodeToMesh(child)) {
//					container.AddNode(child);
//				} else {
//					if (container.Nodes.Count > numBones) {
//						var mesh = NodeContentsToDiscreteMorphableMesh(container);
//						ClearContentsExceptBones(container);
//						node.AddNode(mesh);
//					}
//					if (skipConvertionCounter > 0) {
//						skipConvertionCounter--;
//					}
//					if (child is ImageCombiner) {
//						skipConvertionCounter = 2;
//						node.AddNode(child);
//					} else if (child is Bone) {
//						// Bones already have affected mesh animations. No need to add them to the container.
//					} else if (child is DiscreteMorphableMesh) {
//						node.AddNode(child);
//					} else if (child is DistortionMesh) {
//						var mesh = ConvertDistortionMeshWithTriggers((DistortionMesh)child);
//						node.AddNode(mesh);
//					} else {
//						node.AddNode(child);
//						BuildNodeContents(child, options);
//					}
//				}
//			}
//			if (container.Nodes.Count > numBones) {
//				var mesh = NodeContentsToDiscreteMorphableMesh(container);
//				node.AddNode(mesh);
//			}
//		}

//		private Node ConvertDistortionMeshWithTriggers(DistortionMesh mesh)
//		{
//			var result = new Frame {
//				Id = mesh.Id,
//				Position = mesh.Position,
//				Rotation = mesh.Rotation,
//				Pivot = mesh.Pivot,
//				Size = mesh.Size,
//				Scale = mesh.Scale,
//				Color = mesh.Color,
//			};
//			foreach (var a in mesh.Animators) {
//				result.Animators.Add(a.Clone());
//			}
//			mesh.Position = Vector2.Zero;
//			mesh.Scale = Vector2.One;
//			mesh.Pivot = Vector2.Zero;
//			mesh.Rotation = 0;
//			mesh.Color = Color4.White;
//			var mm = NodeContentsToDiscreteMorphableMesh(mesh);
//			result.AddNode(mm);
//			return result;
//		}

//		void ClearContentsExceptBones(Node container)
//		{
//			for (int i = container.Nodes.Count - 1; i >= 0; i--) {
//				var node = container.Nodes[i];
//				if (!(node is Bone)) {
//					container.Nodes.RemoveAt(i);
//				}
//			}
//		}

//		private DiscreteMorphableMesh NodeContentsToDiscreteMorphableMesh(Node node)
//		{
//			var discreteMesh = new DiscreteMorphableMesh { Id = "Mesh" };
//			foreach (var interval in GetMorphingIntervals(node)) {
//				var mesh = NodeContentsToMorphableMesh(node, interval);
//				discreteMesh.Meshes.Add(mesh);
//			}
//			discreteMesh.RefreshMeshBounds();
//			return discreteMesh;
//		}

//		private MorphableMesh NodeContentsToMorphableMesh(Node node, MorphingInterval interval)
//		{
//			var savedRenderList = Renderer.CurrentRenderList;
//			Widget.RenderTransparentWidgets = true;
//			try {
//				var renderList = new RenderList();
//				Renderer.CurrentRenderList = renderList;
//				var renderChain = new RenderChain();
//				var mesh = new MorphableMesh();
//				int vertexCount = 0;
//				int indexCount = 0;
//				bool isFirstTimeStamp = true;
//				foreach (var time in interval.Timestamps) {
//					node.AnimationTime = time;
//					node.Update(0);
//					node.RenderChainBuilder?.AddToRenderChain(renderChain);
//					renderChain.RenderAndClear();
//					int vc = 0;
//					int ic = 0;
//					foreach (var batch in renderList.Batches) {
//						vc += batch.LastVertex;
//						ic += batch.LastIndex;
//					}
//					if (isFirstTimeStamp) {
//						vertexCount = vc;
//						indexCount = ic;
//					} else {
//						if (vc != vertexCount || ic != indexCount) {
//							throw new InvalidOperationException("Inconsistent number of vertices or indices");
//						}
//					}
//					if (isFirstTimeStamp) {
//						mesh.UVBuffer = new VertexBuffer<Vector2> { Data = new Vector2[vertexCount] };
//						mesh.IndexBuffer = new IndexBuffer { Data = new ushort[indexCount] };
//					}
//					var morphTarget = new MorphableMesh.MorphTarget {
//						Timestamp = time,
//						PosColorBuffer = new VertexBuffer<MorphableMesh.PosColor> { Data = new MorphableMesh.PosColor[vertexCount] }
//					};
//					mesh.MorphTargets.Add(morphTarget);
//					int currentVertex = 0;
//					int currentIndex = 0;
//					foreach (var batch in renderList.Batches) {
//						CopyPosColorData(batch.VertexBuffer.Data, morphTarget.PosColorBuffer.Data, currentVertex, batch.LastVertex);
//						throw new NotImplementedException();
//						//if (isFirstTimeStamp) {
//						//	var mbatch = new MorphableMesh.RenderBatch {
//						//		Texture = batch.Texture1,
//						//		Blending = batch.Blending,
//						//		Shader = batch.Shader,
//						//		StartIndex = batch.StartIndex + currentIndex,
//						//		IndexCount = batch.LastIndex - batch.StartIndex,
//						//	};
//						//	mesh.Batches.Add(mbatch);
//						//	CopyUV1Data(batch.VertexBuffer.Data, mesh.UVBuffer.Data, currentVertex, batch.LastVertex);
//						//	CopyIndices(batch.IndexBuffer.Data, mesh.IndexBuffer.Data, currentIndex, batch.LastIndex, (ushort)currentVertex);
//						//}
//						currentVertex += batch.LastVertex;
//						currentIndex += batch.LastIndex;
//					}
//					renderList.Clear();
//					isFirstTimeStamp = false;
//				}
//				return mesh;
//			} finally {
//				Renderer.CurrentRenderList = savedRenderList;
//				Widget.RenderTransparentWidgets = false;
//			}
//		}

//		// Reorder widget bones with topological sort to maintain correct update
//		// order of transformations
//		private void ReorderBones(Widget widget)
//		{
//			var bones = new Dictionary<int, Bone>();
//			int maxIndex = 0;
//			for (int i = 0; i < widget.Nodes.Count; i++) {
//				var bone = widget.Nodes[i] as Bone;
//				if (bone != null) {
//					if (bones.ContainsKey(bone.Index)) {
//						throw new InvalidOperationException("More than one bone with same index");
//					}
//					bones[bone.Index] = bone;
//					if (bone.Index > maxIndex) {
//						maxIndex = bone.Index;
//					}
//				}
//			}
//			int n = maxIndex + 1;
//			var visited = new bool[n];
//			var g = new List<int>[n];
//			for (int i = 0; i < n; i++) {
//				g[i] = new List<int>();
//			}
//			foreach (var kv in bones) {
//				var b = kv.Value;
//				g[b.BaseIndex].Add(b.Index);
//			}
//			var orderedIndices = new List<int>();
//			Action<int> visit = null;
//			visit = (index) => {
//				visited[index] = true;
//				for (int i = 0; i < g[index].Count; i++) {
//					if (visited[g[index][i]]) {
//						throw new InvalidOperationException("Found cycle in bones parent child relations");
//					}
//					visit(g[index][i]);
//				}
//				orderedIndices.Add(index);
//			};
//			for (int i = 0; i < n; i++) {
//				if (!visited[i]) {
//					visit(i);
//				}
//			}
//			foreach (var i in orderedIndices) {
//				// holes in indices and zero index (implicit bone with Identity transformation)
//				if (!bones.ContainsKey(i)) {
//					continue;
//				}
//				bones[i].Unlink();
//				widget.Nodes.Insert(0, bones[i]);
//			}
//		}

//		private bool CanConvertNodeToMesh(Node node)
//		{
//			if (
//				node is DiscreteMorphableMesh ||
//				node is ImageCombiner ||
//				node is ParticleEmitter ||
//				node is ParticleModifier ||
//				node is ParticlesMagnet ||
//				node is Bone ||
//				node is Audio)
//			{
//				return false;
//			}
//			var id = node.Id;
//			if (id.Length > 0 && (id[0] == '>' || id[0] == '@')) {
//				// Make kumquat pleased.
//				return false;
//			}
//			IAnimator triggers;
//			if (node.Animators.TryFind("Trigger", out triggers) && triggers.ReadonlyKeys.Count > 0) {
//				return false;
//			}
//			foreach (var n in node.Nodes) {
//				if (!CanConvertNodeToMesh(n)) {
//					return false;
//				}
//			}
//			return true;
//		}

//		private void CopyUV1Data(Vertex[] vertices, Vector2[] destination, int destinationIndex, int length)
//		{
//			for (int i = 0; i < length; i++, destinationIndex++) {
//				destination[destinationIndex] = vertices[i].UV1;
//			}
//		}

//		private void CopyPosColorData(Vertex[] vertices, MorphableMesh.PosColor[] destination, int destinationIndex, int length)
//		{
//			for (int i = 0; i < length; i++, destinationIndex++) {
//				destination[destinationIndex].Position = vertices[i].Pos;
//				destination[destinationIndex].Color = vertices[i].Color;
//			}
//		}

//		private void CopyIndices(ushort[] source, ushort[] destination, int destinationIndex, int length, ushort offset)
//		{
//			for (int i = 0; i < length; i++, destinationIndex++) {
//				destination[destinationIndex] = (ushort)(source[i] + offset);
//			}
//		}

//		private void EnumerateKeyFrames(SortedSet<int> allFrames, SortedSet<int> leapFrames, Node node)
//		{
//			foreach (var a in node.Animators) {
//				var leap = a.TargetProperty == "Visible" || a.TargetProperty == "Texture";
//				foreach (var k in a.ReadonlyKeys) {
//					allFrames.Add(k.Frame);
//					if (leap) {
//						leapFrames.Add(k.Frame);
//					}
//				}
//			}
//			foreach (var n in node.Nodes) {
//				EnumerateKeyFrames(allFrames, leapFrames, n);
//			}
//		}

//		private List<MorphingInterval> GetMorphingIntervals(Node node)
//		{
//			var allFrames = new SortedSet<int>();
//			var leapFrames = new SortedSet<int>();
//			EnumerateKeyFrames(allFrames, leapFrames, node);
//			var result = new List<MorphingInterval>();
//			var interval = new MorphingInterval();
//			foreach (var frame in allFrames) {
//				var time = AnimationUtils.FramesToSeconds(frame);
//				if (leapFrames.Contains(frame)) {
//					if (interval.Timestamps.Count > 0) {
//						interval.Timestamps.Add(time - 1);
//						result.Add(interval);
//						interval = new MorphingInterval();
//					}
//				}
//				interval.Timestamps.Add(time);
//			}
//			if (interval.Timestamps.Count > 0) {
//				result.Add(interval);
//			}
//			if (result.Count == 0) {
//				result.Add(new MorphingInterval { Timestamps = { 0, 0 } });
//			}
//			return result;
//		}

//		class MorphingInterval
//		{
//			public readonly List<double> Timestamps = new List<double>();
//		}
//	}
//}

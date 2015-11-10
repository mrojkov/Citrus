using System.Collections.Generic;
using System.Linq;
using Lime.Tests.Mocks;
using NUnit.Framework;

namespace Lime.Tests
{
	[TestFixture]
	public class NodeTests
	{

		[Test]
		public void DisposeTest()
		{
			var root = new Node {Id = "Root"};
			var child = new Node {Id = "Child"};
			var grandChild = new Node {Id = "Grandchild"};
			root.AddNode(child);
			child.AddNode(grandChild);
			root.Dispose();
			Assert.That(root.Nodes, Is.Empty);
			Assert.That(child.Nodes, Is.Empty);
		}

		[Test]
		public void GetRootTest()
		{
			var root = new Node { Id = "Root" };
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			Assert.That(root.GetRoot(), Is.EqualTo(root));
			Assert.That(child.GetRoot(), Is.EqualTo(root));
			Assert.That(grandChild.GetRoot(), Is.EqualTo(root));
		}

		[Test]
		public void ChildOfTest()
		{
			var root = new Node { Id = "Root" };
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			Assert.That(grandChild.ChildOf(child), Is.True);
			Assert.That(grandChild.ChildOf(root), Is.True);
			Assert.That(child.ChildOf(grandChild), Is.False);
			Assert.That(root.ChildOf(grandChild), Is.False);
		}

		private const string AnimationName = "Animation";
		private const string MarkerName = "Marker";

		[Test]
		public void TryRunAnimationWithoutAnimationTest()
		{
			var node = new Node();
			Assert.That(node.TryRunAnimation(MarkerName), Is.False);
			Assert.That(node.TryRunAnimation(MarkerName, AnimationName), Is.False);
		}

		[Test]
		public void TryRunAnimationWithMarkerTest()
		{
			var node = new Node();
			var animation = new Animation();
			animation.Markers.AddPlayMarker(MarkerName, 0);
			node.Animations = new AnimationList(node) { animation };
			Assert.That(node.TryRunAnimation(MarkerName, AnimationName), Is.False);
			Assert.That(node.TryRunAnimation(MarkerName), Is.True);
			Assert.That(node.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		public void TryRunAnimationWithMarkerAndIdTest()
		{
			var node = new Node();
			var animation = new Animation {
				Id = AnimationName
			};
			animation.Markers.AddPlayMarker(MarkerName, 0);
			node.Animations = new AnimationList(node) { animation };
			Assert.That(node.TryRunAnimation(MarkerName, AnimationName), Is.True);
			Assert.That(node.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		public void RunAnimationWithoutAnimationTest()
		{
			var node = new Node();
			Assert.Throws<Exception>(() => node.RunAnimation(MarkerName));
			Assert.Throws<Exception>(() => node.RunAnimation(MarkerName, AnimationName));
		}

		[Test]
		public void RunAnimationWithMarkerTest()
		{
			var node = new Node();
			var animation = new Animation();	
			animation.Markers.AddPlayMarker(MarkerName, 0);
			node.Animations = new AnimationList(node) { animation };
			node.RunAnimation(MarkerName);
			Assert.That(node.CurrentAnimation, Is.EqualTo(MarkerName));
			Assert.Throws<Exception>(() => node.RunAnimation(MarkerName, AnimationName));
		}

		[Test]
		public void RunAnimationWithMarkerAndIdTest()
		{
			var node = new Node();
			var animation = new Animation {
				Id = AnimationName
			};
			animation.Markers.AddPlayMarker(MarkerName, 0);
			node.Animations = new AnimationList(node) { animation };
			node.RunAnimation(MarkerName, AnimationName);
			Assert.That(node.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		public void DeepCloneSafeTest()
		{
			Assert.Fail();
		}

		[Test]
		public void DeepCloneSafeTest1()
		{
			Assert.Fail();
		}

		[Test]
		public void DeepCloneFastTest()
		{
			Assert.Fail();
		}

		[Test]
		public void DeepCloneFastTest1()
		{
			Assert.Fail();
		}

		[Test]
		public void ToStringTest()
		{
			var root = new Node();
			var child = new Node { Id = "Child", Tag = "Special"};
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			Assert.That(root.ToString(), Is.EqualTo("Node, \"\", [Node]"));
			Assert.That(child.ToString(), Is.EqualTo("Node, \"Child\", [Node]/Child (Special)"));
			Assert.That(grandChild.ToString(), Is.EqualTo("Node, \"Grandchild\", [Node]/Child (Special)/Grandchild"));
		}

		[Test]
		public void UnlinkTest()
		{
			var parent = new Node { Id = "Parent" };
			var child = new Node { Id = "Child" };
			parent.AddNode(child);
			child.Unlink();
			Assert.That(parent.Nodes, Is.Empty);
		}

		[Test]
		public void UnlinkAndDisposeTest()
		{
			var root = new Node { Id = "Root" };
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			child.UnlinkAndDispose();
			Assert.That(root.Nodes, Is.Empty);
			Assert.That(child.Nodes, Is.Empty);
		}

		[Test]
		public void UpdateTest()
		{
			var root = new Node { Id = "Root" };
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			var nodes = new List<Node> {root, child, grandChild};
			foreach (var node in nodes) {
				var animation = new Animation();
				animation.Markers.AddPlayMarker("Start", 0);
				node.Animations = new AnimationList(node) { animation };
				node.RunAnimation("Start");
			}
			const float FrameDelta = (float)AnimationUtils.MsecsPerFrame / 1000;
			var animationFrames = nodes.Select(node => node.AnimationFrame);
			for (var i = 0; i < 10; i++) {
				root.Update(FrameDelta);
				Assert.That(animationFrames, Is.All.EqualTo(i));
			}
		}

		[Test]
		public void AddToRenderChainSingleNodeTest()
		{
			var renderChain = new RenderChain();
			var root = new Node { Id = "Root" };
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
		}

		[Test]
		public void AddToRenderChainMultipleNodesTest()
		{
			var renderChain = new RenderChain();
			var root = new Node { Id = "Root" };
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
		}

		[Test]
		public void AddToRenderChainNodeWithSideEffectsTest()
		{
			var renderChain = new RenderChain();
			var root = new Node { Id = "Root" };
			var childWithSideEffects = new NodeWithSideEffects();
			root.AddNode(childWithSideEffects);
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate().Contains(childWithSideEffects));
		}

		[Test]
		public void AddNodeTest()
		{
			var parent = new Node { Id = "Parent" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			parent.AddNode(child1);
			parent.AddNode(child2);
			Assert.That(parent.Nodes.Contains(child1));
			Assert.That(parent.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(parent.Nodes));
		}

		[Test]
		public void AddToNodeTest()
		{
			var parent = new Node { Id = "Parent" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			child1.AddToNode(parent);
			child2.AddToNode(parent);
			Assert.That(parent.Nodes.Contains(child1));
			Assert.That(parent.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(parent.Nodes));
		}

		[Test]
		public void PushNodeTest()
		{
			var parent = new Node { Id = "Parent" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			parent.PushNode(child2);
			parent.PushNode(child1);
			Assert.That(parent.Nodes.Contains(child1));
			Assert.That(parent.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(parent.Nodes));
		}

		[Test]
		public void PushToNodeTest()
		{
			var parent = new Node { Id = "Parent" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			child2.PushToNode(parent);
			child1.PushToNode(parent);
			Assert.That(parent.Nodes.Contains(child1));
			Assert.That(parent.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(parent.Nodes));
		}

		[Test]
		public void FindTest()
		{
			var root = new Node { Id = "Root" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child2.AddNode(grandChild);
			Assert.That(root.Find<Node>("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.Find<Node>("Child{0}", 1), Is.EqualTo(child1));
			Assert.That(root.Find<Node>("Child{0}/Grandchild", 2), Is.EqualTo(grandChild));
			var e = Assert.Throws<Exception>(() => grandChild.Find<Node>("Root"));
			Assert.That(e.Message, Is.EqualTo("'Root' of Node not found for 'Node, \"Grandchild\", Root/Child2/Grandchild'"));
		}

		[Test]
		public void TryFindTest()
		{
			var root = new Node { Id = "Root" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child2.AddNode(grandChild);
			Assert.That(root.TryFind<Node>("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.TryFind<Node>("Child{0}", 1), Is.EqualTo(child1));
			Node node;
			Assert.That(root.TryFind("Child1/Grandchild", out node), Is.False);
			Assert.That(root.TryFind("Child2/Grandchild", out node), Is.True);
			Assert.That(node, Is.EqualTo(grandChild));
			Assert.That(grandChild.TryFind<Node>("Root"), Is.Null);
		}

		[Test]
		public void FindNodeTest()
		{
			var root = new Node { Id = "Root" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child2.AddNode(grandChild);
			Assert.That(root.FindNode("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.FindNode("Child2/Grandchild"), Is.EqualTo(grandChild));
			var e = Assert.Throws<Exception>(() => grandChild.FindNode("Root"));
			Assert.That(e.Message, Is.EqualTo("'Root' not found for 'Node, \"Grandchild\", Root/Child2/Grandchild'"));
		}

		[Test]
		public void TryFindNodeTest()
		{
			var root = new Node { Id = "Root" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child2.AddNode(grandChild);
			Assert.That(root.TryFindNode("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.TryFindNode("Child2/Grandchild"), Is.EqualTo(grandChild));
			Assert.That(grandChild.TryFindNode("Root"), Is.Null);
		}

		[Test]
		public void DescendatsTest()
		{
			var root = new Node { Id = "Root" };
			var child1 = new Node { Id = "Child1" };
			var child2 = new Node { Id = "Child2" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child1.AddNode(grandChild);
			Assert.That(root.Descendants.Contains(child1));
			Assert.That(root.Descendants.Contains(child2));
			Assert.That(root.Descendants.Contains(grandChild));
			Assert.That(child1.Descendants.Contains(grandChild));
			Assert.That(grandChild.Descendants, Is.Empty);
		}

		[Test]
		public void StaticScaleTest()
		{
			Assert.Fail();
		}

		[Test]
		public void AdvanceAnimationTest()
		{
			var animation = new Animation();
			animation.Markers.AddPlayMarker("Start", 0);
			var node = new Node ();
			node.Animations = new AnimationList(node) {animation};
			node.RunAnimation("Start");
			const float FrameDelta = (float)(AnimationUtils.MsecsPerFrame) / 1000;
			for (var i = 0; i < 10; i++) {
				node.AdvanceAnimation(FrameDelta);
				Assert.That(node.AnimationFrame, Is.EqualTo(i));
			}
		}

		[Test]
		public void PreloadAssetsTest()
		{
			var node = new NodeWithAssets();
			node.PreloadAssets();
		}
	}
}
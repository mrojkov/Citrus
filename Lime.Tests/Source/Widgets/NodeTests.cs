using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Lime.Tests.Source.Widgets
{
	[TestFixture]
	public class NodeTests
	{
		private Node root;
		private Node child1;
		private Node child2;
		private Node grandChild;

		[SetUp]
		public void TestSetUp()
		{
			root = new Node {Id = "Root"};
			child1 = new Node {Id = "Child1"};
			child2 = new Node {Id = "Child2"};
			grandChild = new Node {Id = "Grandchild"};
			root.AddNode(child1);
			root.AddNode(child2);
			child1.AddNode(grandChild);
		}

		[Test]
		public void DisposeTest()
		{
			root.Dispose();
			Assert.That(root.Nodes, Is.Empty);
			Assert.That(child1.Nodes, Is.Empty);
		}

		[Test]
		public void GetRootTest()
		{
			Assert.That(root.GetRoot(), Is.EqualTo(root));
			Assert.That(child1.GetRoot(), Is.EqualTo(root));
			Assert.That(grandChild.GetRoot(), Is.EqualTo(root));
		}

		[Test]
		public void ChildOfTest()
		{
			Assert.That(grandChild.ChildOf(child1), Is.True);
			Assert.That(grandChild.ChildOf(root), Is.True);
			Assert.That(child1.ChildOf(grandChild), Is.False);
			Assert.That(root.ChildOf(grandChild), Is.False);
		}

		private const string AnimationName = "Animation";
		private const string MarkerName = "Marker";

		[Test]
		public void TryRunAnimationWithoutAnimationTest()
		{
			Assert.That(root.TryRunAnimation(MarkerName), Is.False);
			Assert.That(root.TryRunAnimation(MarkerName, AnimationName), Is.False);
		}

		[Test]
		public void TryRunAnimationWithMarkerTest()
		{
			var animation = new Animation();
			animation.Markers.AddPlayMarker(MarkerName, 0);
			root.Animations = new AnimationList(root) { animation };
			Assert.That(root.TryRunAnimation(MarkerName, AnimationName), Is.False);
			Assert.That(root.TryRunAnimation(MarkerName), Is.True);
			Assert.That(root.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		public void TryRunAnimationWithMarkerAndIdTest()
		{
			var animation = new Animation { Id = AnimationName };
			animation.Markers.AddPlayMarker(MarkerName, 0);
			root.Animations = new AnimationList(root) { animation };
			Assert.That(root.TryRunAnimation(MarkerName, AnimationName), Is.True);
			Assert.That(root.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		public void RunAnimationWithoutAnimationTest()
		{
			Assert.Throws<Exception>(() => root.RunAnimation(MarkerName));
			Assert.Throws<Exception>(() => root.RunAnimation(MarkerName, AnimationName));
		}

		[Test]
		public void RunAnimationWithMarkerTest()
		{
			var animation = new Animation();	
			animation.Markers.AddPlayMarker(MarkerName, 0);
			root.Animations = new AnimationList(root) { animation };
			root.RunAnimation(MarkerName);
			Assert.That(root.CurrentAnimation, Is.EqualTo(MarkerName));
			Assert.Throws<Exception>(() => root.RunAnimation(MarkerName, AnimationName));
		}

		[Test]
		public void RunAnimationWithMarkerAndIdTest()
		{
			var animation = new Animation {
				Id = AnimationName
			};
			animation.Markers.AddPlayMarker(MarkerName, 0);
			root.Animations = new AnimationList(root) { animation };
			root.RunAnimation(MarkerName, AnimationName);
			Assert.That(root.CurrentAnimation, Is.EqualTo(MarkerName));
		}

		[Test]
		[Ignore("Need to implement reliable way to check cloned objects.")]
		public void DeepCloneSafeTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Need to implement reliable way to check cloned objects.")]
		public void DeepCloneSafeTest1()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Need to implement reliable way to check cloned objects.")]
		public void DeepCloneFastTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Need to implement reliable way to check cloned objects.")]
		public void DeepCloneFastTest1()
		{
			Assert.Fail();
		}

		[Test]
		public void ToStringTest()
		{
			root.Id = "";
			child1.Tag = "Special";
			Assert.That(root.ToString(), Is.EqualTo("Node, \"\", [Node]"));
			Assert.That(child1.ToString(), Is.EqualTo("Node, \"Child1\", [Node]/Child1 (Special)"));
			Assert.That(grandChild.ToString(), Is.EqualTo("Node, \"Grandchild\", [Node]/Child1 (Special)/Grandchild"));
		}

		[Test]
		public void UnlinkTest()
		{
			child1.Unlink();
			Assert.That(root.Nodes, Is.Not.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
		}

		[Test]
		public void UnlinkAndDisposeTest()
		{
			child1.UnlinkAndDispose();
			Assert.That(root.Nodes, Is.Not.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
			Assert.That(child1.Nodes, Is.Empty);
		}

		[Test]
		public void UpdateTest()
		{
			var nodes = new List<Node> {root, child1, grandChild};
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
			root = new Node { Id = "Root" };
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
		}

		[Test]
		public void AddToRenderChainMultipleNodesTest()
		{
			var renderChain = new RenderChain();
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
		}

		[Test]
		public void AddToRenderChainNodeWithSideEffectsTest()
		{
			var renderChain = new RenderChain();
			root = new Node { Id = "Root" };
			var childWithSideEffects = new NodeWithSideEffects();
			root.AddNode(childWithSideEffects);
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate().Contains(childWithSideEffects));
		}

		[Test]
		public void AddNodeTest()
		{
			root = new Node { Id = "Parent" };
			child1 = new Node { Id = "Child1" };
			child2 = new Node { Id = "Child2" };
			root.AddNode(child1);
			root.AddNode(child2);
			Assert.That(root.Nodes.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(root.Nodes));
		}

		[Test]
		public void AddToNodeTest()
		{
			root = new Node { Id = "Parent" };
			child1 = new Node { Id = "Child1" };
			child2 = new Node { Id = "Child2" };
			child1.AddToNode(root);
			child2.AddToNode(root);
			Assert.That(root.Nodes.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(root.Nodes));
		}

		[Test]
		public void PushNodeTest()
		{
			root = new Node { Id = "Parent" };
			child1 = new Node { Id = "Child1" };
			child2 = new Node { Id = "Child2" };
			root.PushNode(child2);
			root.PushNode(child1);
			Assert.That(root.Nodes.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(root.Nodes));
		}

		[Test]
		public void PushToNodeTest()
		{
			root = new Node { Id = "Parent" };
			child1 = new Node { Id = "Child1" };
			child2 = new Node { Id = "Child2" };
			child2.PushToNode(root);
			child1.PushToNode(root);
			Assert.That(root.Nodes.Contains(child1));
			Assert.That(root.Nodes.Contains(child2));
			Assert.That(child1, Is.Before(child2).In(root.Nodes));
		}

		[Test]
		public void FindTest()
		{
			Assert.That(root.Find<Node>("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.Find<Node>("Child{0}", 2), Is.EqualTo(child2));
			Assert.That(root.Find<Node>("Child{0}/Grandchild", 1), Is.EqualTo(grandChild));
			var e = Assert.Throws<Exception>(() => grandChild.Find<Node>("Root"));
			Assert.That(e.Message, Is.EqualTo("'Root' of Node not found for 'Node, \"Grandchild\", Root/Child1/Grandchild'"));
		}

		[Test]
		public void TryFindTest()
		{
			Assert.That(root.TryFind<Node>("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.TryFind<Node>("Child{0}", 1), Is.EqualTo(child1));
			Node node;
			Assert.That(root.TryFind("Child2/Grandchild", out node), Is.False);
			Assert.That(node, Is.Null);
			Assert.That(root.TryFind("Child1/Grandchild", out node), Is.True);
			Assert.That(node, Is.EqualTo(grandChild));
			Assert.That(grandChild.TryFind<Node>("Root"), Is.Null);
		}

		[Test]
		public void FindNodeTest()
		{
			Assert.That(root.FindNode("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.FindNode("Child1/Grandchild"), Is.EqualTo(grandChild));
			var e = Assert.Throws<Exception>(() => grandChild.FindNode("Root"));
			Assert.That(e.Message, Is.EqualTo("'Root' not found for 'Node, \"Grandchild\", Root/Child1/Grandchild'"));
		}

		[Test]
		public void TryFindNodeTest()
		{
			Assert.That(root.TryFindNode("Grandchild"), Is.EqualTo(grandChild));
			Assert.That(root.TryFindNode("Child1/Grandchild"), Is.EqualTo(grandChild));
			Assert.That(grandChild.TryFindNode("Root"), Is.Null);
		}

		[Test]
		public void DescendatsTest()
		{
			Assert.That(root.Descendants.Contains(child1));
			Assert.That(root.Descendants.Contains(child2));
			Assert.That(root.Descendants.Contains(grandChild));
			Assert.That(child1.Descendants.Contains(grandChild));
			Assert.That(grandChild.Descendants, Is.Empty);
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
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

		private class NodeWithAssets : Node
		{
			public ITexture Texture
			{ get; set; }
			public SerializableFont Font
			{ get; set; }
		}

		private class NodeWithSideEffects : Node
		{
			public override void AddToRenderChain(RenderChain chain)
			{
				base.AddToRenderChain(chain);
				chain.Add(this, Layer);
			}
		}
	}
}
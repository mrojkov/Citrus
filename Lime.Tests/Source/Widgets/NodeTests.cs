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

		[Test]
		public void TryRunAnimationTest()
		{
			Assert.Fail();
		}

		[Test]
		public void RunAnimationTest()
		{
			Assert.Fail();
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
			Assert.Fail();
		}

		[Test]
		public void AddToRenderChainTest()
		{
			var renderChain = new RenderChain();
			var root = new Node { Id = "Root" };
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
			var child = new Node { Id = "Child" };
			var grandChild = new Node { Id = "Grandchild" };
			root.AddNode(child);
			child.AddNode(grandChild);
			root.AddToRenderChain(renderChain);
			Assert.That(renderChain.Enumerate(), Is.Empty);
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
			Assert.Fail();
		}

		[Test]
		public void PreloadAssetsTest()
		{
			Assert.Fail();
		}
	}
}
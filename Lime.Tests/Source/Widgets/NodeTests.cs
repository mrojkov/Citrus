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
			Assert.Fail();
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
		public void RenderTest()
		{
			Assert.Fail();
		}

		[Test]
		public void AddToRenderChainTest()
		{
			Assert.Fail();
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
			Assert.Fail();
		}

		[Test]
		public void FindTest1()
		{
			Assert.Fail();
		}

		[Test]
		public void TryFindTest()
		{
			Assert.Fail();
		}

		[Test]
		public void TryFindTest1()
		{
			Assert.Fail();
		}

		[Test]
		public void TryFindTest2()
		{
			Assert.Fail();
		}

		[Test]
		public void FindNodeTest()
		{
			Assert.Fail();
		}

		[Test]
		public void TryFindNodeTest()
		{
			Assert.Fail();
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
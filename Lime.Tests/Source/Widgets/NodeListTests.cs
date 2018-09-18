using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Lime.Tests.Source.Widgets
{
	[TestFixture]
	public class NodeListTests
	{
		[Test]
		public void IsReadOnlyTest()
		{
			Assert.That(new NodeList().IsReadOnly, Is.False);
		}

		[Test]
		public void IndexOfTest()
		{
			var list = new NodeList();
			var node = new Node();
			Assert.That(list.IndexOf(node), Is.EqualTo(-1));
			list.Add(node);
			Assert.That(list.IndexOf(node), Is.EqualTo(0));
		}

		[Test]
		public void CopyToEmptyTest()
		{
			var list = new NodeList();
			Node[] array = new Node[3];
			list.CopyTo(array, 0);
			Assert.That(array, Is.All.Null);
		}

		[Test]
		public void CopyToNotEmptyTest()
		{
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var list = new NodeList { node1, node2, node3 };
			Node[] array = new Node[3];
			list.CopyTo(array, 0);
			for (int i = 0; i < array.Length; i++) {
				Assert.That(array[i], Is.SameAs(list[i]));
			}
		}

		[Test]
		public void SortEmptyTest()
		{
			var list = new NodeList();
			list.Sort((first, second) => 0);
			Assert.That(list, Is.Empty);
		}

		[Test]
		public void SortNotEmptyTest()
		{
			var node1 = new Node { Layer = 3 };
			var node2 = new Node { Layer = 2 };
			var node3 = new Node { Layer = 1 };
			var list = new NodeList { node1, node2, node3 };
			Comparison<Node> comparison = (first, second) => first.Layer <= second.Layer ? 1 : 0;
			list.Sort(comparison);
			var expectedList = new NodeList { node3, node2, node1 };
			for (int i = 0; i < list.Count; i++) {
				Assert.That(list[i], Is.SameAs(expectedList[i]));
			}
		}

		[Test]
		public void ContainsTest()
		{
			var list = new NodeList();
			var node = new Node();
			Assert.That(list.Contains(node), Is.False);
			list.Add(node);
			Assert.That(list.Contains(node), Is.True);
		}

		[Test]
		public void EnumeratorEmptyTest()
		{
			var list = new NodeList();
			var enumerator = list.GetEnumerator();
			Assert.That(enumerator.Current, Is.Null);
			Assert.That(enumerator.MoveNext(), Is.False);
		}

		[Test]
		public void EnumeratorNotEmptyTest()
		{
			var node1 = new Node { Layer = 3 };
			var node2 = new Node { Layer = 2 };
			var node3 = new Node { Layer = 1 };
			var list = new NodeList { node1, node2, node3 };
			var enumerator = list.GetEnumerator();
			for (int i = 0; i < 10; i++) {
				Assert.That(enumerator.Current, Is.Null);
				foreach (var node in new List<Node> {node1, node2, node3}) {
					Assert.That(enumerator.MoveNext(), Is.True);
					Assert.That(enumerator.Current, Is.EqualTo(node));
				}
				Assert.That(enumerator.MoveNext(), Is.False);
				// BUG: This expression is not mandatory
				enumerator.Reset();
			}
		}

		[Test]
		public void PushTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var node1 = new Node { Id = "Node1" };
			list.Push(node1);
			Assert.That(list[0], Is.SameAs(node1));
			Assert.That(node1.Parent, Is.SameAs(owner));
			Assert.That(node1.NextSibling, Is.Null);
			var node2 = new Node { Id = "Node2" };
			list.Push(node2);
			Assert.That(list[0], Is.SameAs(node2));
			Assert.That(node2.NextSibling, Is.SameAs(node1));
			Assert.That(node1.NextSibling, Is.Null);
			Assert.That(node2.Parent, Is.SameAs(owner));
		}

		[Test]
		public void AddTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var node1 = new Node { Id = "Node1" };
			list.Add(node1);
			Assert.That(list.Last(), Is.SameAs(node1));
			Assert.That(node1.Parent, Is.SameAs(owner));
			Assert.That(node1.NextSibling, Is.Null);
			var node2 = new Node { Id = "Node2" };
			list.Add(node2);
			Assert.That(list.Last(), Is.SameAs(node2));
			Assert.That(node1.NextSibling, Is.SameAs(node2));
			Assert.That(node2.NextSibling, Is.Null);
			Assert.That(node2.Parent, Is.SameAs(owner));
		}

		[Test]
		public void AddAdoptedNodeTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var node = new Node();
			list.Add(node);
			var e = Assert.Throws<Exception>(() => list.Add(node));
			Assert.That(e.Message, Is.EqualTo("Can't adopt a node twice. Call node.Unlink() first"));
		}

		[Test]
		public void AddWidgetToNonWidgetOwnerTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var widget = new Widget();
			var e = Assert.Throws<Exception>(() => list.Add(widget));
			Assert.That(e.Message, Is.EqualTo("A widget can be adopted only by other widget"));
		}

		[Test]
		public void AddRangeTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var insertingList = new List<Node>{ node1, node2, node3 };
			list.AddRange(insertingList);
			for (int i = 0; i < list.Count; i++) {
				Assert.That(list[i], Is.SameAs(insertingList[i]));
				var nextSibling = i < list.Count - 1 ? list[i + 1] : null;
				Assert.That(list[i].NextSibling, Is.SameAs(nextSibling));
			}
			Assert.That(list.Select(node => node.Parent), Is.All.SameAs(owner));
		}

		[Test]
		public void FirstOrNullEmptyTest()
		{
			var list = new NodeList();
			Assert.That(list.FirstOrNull(), Is.Null);
		}

		[Test]
		public void FirstOrNullNotEmptyTest()
		{
			var node = new Node();
			var list = new NodeList {node};
			Assert.That(list.FirstOrNull(), Is.SameAs(node));
		}

		[Test]
		public void InsertTest()
		{
			var owner = new Node();
			var list = new NodeList(owner);
			var node1 = new Node {Id = "Node1"};
			list.Insert(0, node1);
			Assert.That(list[0], Is.SameAs(node1));
			Assert.That(node1.Parent, Is.SameAs(owner));
			Assert.That(node1.NextSibling, Is.Null);
			var node2 = new Node {Id = "Node2"};
			list.Insert(1, node2);
			Assert.That(list[1], Is.SameAs(node2));
			Assert.That(node1.NextSibling, Is.SameAs(node2));
			Assert.That(node2.NextSibling, Is.Null);
			Assert.That(node2.Parent, Is.SameAs(owner));
		}

		[Test]
		public void RemoveEmptyTest()
		{
			var list = new NodeList();
			Assert.That(list.Remove(null), Is.False);
		}

		[Test]
		public void RemoveNotEmptyTest()
		{
			var owner = new Node();
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var list = new NodeList(owner) { node1, node2, node3 };
			Assert.That(list.Remove(node2), Is.True);
			Assert.That(list.Contains(node1));
			Assert.That(list, Is.Not.Contains(node2));
			Assert.That(list.Contains(node3));
			Assert.That(node1.NextSibling, Is.SameAs(node3));
			Assert.That(node2.Parent, Is.Null);
			Assert.That(node2.NextSibling, Is.Null);
		}

		[Test]
		public void ClearEmptyTest()
		{
			var list = new NodeList();
			list.Clear();
			Assert.That(list, Is.Empty);
		}

		[Test]
		public void ClearNotEmptyTest()
		{
			var owner = new Node();
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var nodes = new List<Node> {node1, node2, node3};
			var list = new NodeList(owner) {node1, node2, node3};
			list.Clear();
			Assert.That(list, Is.Empty);
			foreach (var node in nodes) {
				Assert.That(node.Parent, Is.Null);
				Assert.That(node.NextSibling, Is.Null);
			}
		}

		[Test]
		public void TryFindTest()
		{
			var list = new NodeList();
			var node1 = new Node { Id = "Node1" };
			var node2 = new Node { Id = "Node2" };
			Assert.That(list.TryFind("Node1"), Is.Null);
			Assert.That(list.TryFind("Node2"), Is.Null);
			list.Add(node1);
			list.Add(node2);
			Assert.That(list.TryFind("Node1"), Is.SameAs(node1));
			Assert.That(list.TryFind("Node2"), Is.SameAs(node2));
		}

		[Test]
		public void RemoveAtEmptyTest()
		{
			var list = new NodeList();
			Assert.Throws<IndexOutOfRangeException>(() => list.RemoveAt(0));
		}

		[Test]
		public void RemoveAtNotEmptyTest()
		{
			var owner = new Node();
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var list = new NodeList(owner) { node1, node2, node3 };
			list.RemoveAt(1);
			Assert.That(list.Contains(node1));
			Assert.That(list, Is.Not.Contains(node2));
			Assert.That(list.Contains(node3));
			Assert.That(node1.NextSibling, Is.SameAs(node3));
			Assert.That(node2.Parent, Is.Null);
			Assert.That(node2.NextSibling, Is.Null);
		}

		[Test]
		public void IndexerGetEmptyTest()
		{
			var list = new NodeList();
			Node node;
			Assert.Throws<IndexOutOfRangeException>(() => node = list[0]);
		}

		[Test]
		public void IndexerGetNotEmptyTest()
		{
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var list = new NodeList { node1, node2, node3 };
			Node node;
			Assert.Throws<ArgumentOutOfRangeException>(() => node = list[-1]);
			Assert.Throws<ArgumentOutOfRangeException>(() => node = list[3]);
			Assert.That(list[1], Is.EqualTo(node2));
		}

		[Test]
		public void IndexerSetEmptyTest()
		{
			var list = new NodeList();
			Assert.Throws<ArgumentOutOfRangeException>(() => list[0] = new Node());
		}

		[Test]
		public void IndexerSetNotEmptyTest()
		{
			var owner = new Node();
			var node1 = new Node();
			var node2 = new Node();
			var node3 = new Node();
			var list = new NodeList(owner) { node1, node2, node3 };
			var newNode = new Node();
			list[1] = newNode;
			Assert.That(list.IndexOf(newNode), Is.EqualTo(1));
			Assert.That(list, Is.Not.Contains(node2));
			Assert.That(node1.NextSibling, Is.SameAs(newNode));
			Assert.That(newNode.NextSibling, Is.SameAs(node3));
			Assert.That(newNode.Parent, Is.SameAs(owner));
			Assert.That(node2.Parent, Is.Null);
			Assert.That(node2.NextSibling, Is.Null);
		}
	}
}
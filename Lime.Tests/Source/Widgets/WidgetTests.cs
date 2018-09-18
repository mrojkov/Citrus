using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lime.Tests.Source.Widgets
{
	[TestFixture]
	public class WidgetTests
	{
		private Widget root;
		private Widget child1;
		private Widget child2;
		private Widget grandChild;

		[SetUp]
		public void TestSetUp()
		{
			root = new Widget { Id = "Root" };
			child1 = new Widget { Id = "Child1" };
			child2 = new Widget { Id = "Child2" };
			grandChild = new Widget { Id = "Grandchild" };
			root.AddNode(child1);
			root.AddNode(child2);
			child1.AddNode(grandChild);
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void WasClickedTest()
		{
			Assert.Fail();
		}

		[Test]
		public void DisposeTest()
		{
			var widgets = new List<Widget> {root, child1, child2, grandChild};
			foreach (var widget in widgets) {
				widget.Tasks.Add(EmptyTask);
				widget.LateTasks.Add(EmptyTask);
			}
			root.Dispose();
			foreach (var widget in widgets) {
				Assert.That(widget.Tasks, Is.Empty);
				Assert.That(widget.LateTasks, Is.Empty);
			}
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void RefreshLayoutTest()
		{
			Assert.Fail();
		}

		[Test]
		public void CalcContentSizeTest()
		{
			root.Size = Vector2.Half;
			Assert.That(root.CalcContentSize(), Is.EqualTo(Vector2.Half));
			root.Size = Vector2.One;
			Assert.That(root.CalcContentSize(), Is.EqualTo(Vector2.One));
		}

		[Test]
		[Ignore("Need to implement reliable way to check cloned objects.")]
		public void DeepCloneFastTest()
		{
			Assert.Fail();
		}

		[Test]
		public void UpdateTest()
		{
			Assert.Fail();
		}

		[Test]
		public void RaiseUpdatingTest()
		{
			const float ExpectedDelta = 0.1f;
			var updatingRaised = false;
			var actualDelta = 0f;
			root.Updating += delta => {
				updatingRaised = true;
				actualDelta = delta;
			};
			root.RaiseUpdating(ExpectedDelta);
			Assert.That(updatingRaised);
			Assert.That(actualDelta, Is.EqualTo(ExpectedDelta));
		}

		[Test]
		public void RaiseUpdatedTest()
		{
			const float ExpectedDelta = 0.1f;
			var updatedRaised = false;
			var actualDelta = 0f;
			root.Updated += delta => {
				updatedRaised = true;
				actualDelta = delta;
			};
			root.RaiseUpdated(ExpectedDelta);
			Assert.That(updatedRaised);
			Assert.That(actualDelta, Is.EqualTo(ExpectedDelta));
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void CalcLocalToParentTransformTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void StaticScaleTest()
		{
			Assert.Fail();
		}

		[Test]
		public void AddToRenderChainTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void IsMouseOverTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void HitTestTest()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Wait until development on this function stops.")]
		public void GetEffectiveLayerTest()
		{
			Assert.Fail();
		}

		private IEnumerator<object> EmptyTask()
		{
			yield return null;
		}
	}
}
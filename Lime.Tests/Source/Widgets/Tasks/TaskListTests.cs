using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lime.Tests.Source.Widgets.Tasks
{
	[TestFixture]
	public class TaskListTests
	{
		[Test]
		public void StopTest()
		{
			var list = new TaskList();
			var task1 = list.Add(TwoFramesTask);
			var task2 = list.Add(TwoFramesTask);
			var task3 = list.Add(TwoFramesTask);
			list.Stop();
			Assert.That(list, Is.Empty);
			Assert.That(task1.Completed);
			Assert.That(task2.Completed);
			Assert.That(task3.Completed);
		}

		[Test]
		public void StopByPredicateTest()
		{
			var list = new TaskList();
			var task1 = list.Add(TwoFramesTask);
			var task2 = list.Add(TwoFramesTask);
			Action updating = () => { };
			task2.Updating = updating;
			var task3 = list.Add(TwoFramesTask);
			list.Stop(t => t.Updating == updating);
			Assert.That(list.Contains(task1));
			Assert.That(list, Is.Not.Contains(task2));
			Assert.That(list.Contains(task3));
			Assert.That(task2.Completed);
		}

		[Test]
		public void StopByTagTest()
		{
			var list = new TaskList();
			var task1 = list.Add(TwoFramesTask);
			var task2 = list.Add(TwoFramesTask);
			var tag = new object();
			task2.Tag = tag;
			var task3 = list.Add(TwoFramesTask);
			list.StopByTag(tag);
			Assert.That(list.Contains(task1));
			Assert.That(list, Is.Not.Contains(task2));
			Assert.That(list.Contains(task3));
			Assert.That(task2.Completed);
		}

		[Test]
		public void AddWithoutTagTest()
		{
			var list = new TaskList();
			var task1 = list.Add(TwoFramesTask());
			var task2 = list.Add(TwoFramesTask);
			Assert.That(list.Contains(task1));
			Assert.That(task1.Tag, Is.Null);
			Assert.That(list.Contains(task2));
			Assert.That(task2.Tag, Is.Null);
			Assert.That(task1, Is.Before(task2).In(list));
		}

		[Test]
		public void AddWithTagTest()
		{
			var list = new TaskList();
			var tag1 = new object();
			var tag2 = new object();
			var task1 = list.Add(TwoFramesTask(), tag1);
			var task2 = list.Add(TwoFramesTask, tag2);
			Assert.That(list.Contains(task1));
			Assert.That(task1.Tag, Is.EqualTo(tag1));
			Assert.That(list.Contains(task2));
			Assert.That(task2.Tag, Is.EqualTo(tag2));
			Assert.That(task1, Is.Before(task2).In(list));
		}

		[Test]
		public void UpdateNestedTest()
		{
			var list = new TaskList();
			 list.Add(UpdateNestedTestTask(list));
			list.Update(0);
			list.Update(0);
			Assert.Fail("Task haven't called Assert.Pass() on second Update(). Possible nested Update() call.");
		}

		[Test]
		public void UpdateTest()
		{
			const float UpdateDelta = 0.1f;
			var list = new TaskList();
			var task1 = list.Add(TwoFramesTask);
			var task2 = list.Add(ThreeFramesTask);
			list.Update(UpdateDelta);
			Assert.That(!task1.Completed);
			Assert.That(list.Contains(task1));
			Assert.That(!task2.Completed);
			Assert.That(list.Contains(task2));
			list.Update(UpdateDelta);
			Assert.That(task1.Completed);
			Assert.That(list.Contains(task1));
			Assert.That(!task2.Completed);
			Assert.That(list.Contains(task2));
			list.Update(UpdateDelta);
			Assert.That(list, Is.Not.Contains(task1));
			Assert.That(task2.Completed);
			Assert.That(list.Contains(task2));
			list.Update(UpdateDelta);
			Assert.That(list, Is.Not.Contains(task2));
		}

		private IEnumerator<object> UpdateNestedTestTask(TaskList list)
		{
			list.Update(0);
			yield return null;
			Assert.Pass();
		}

		private IEnumerator<object> TwoFramesTask()
		{
			yield return null;
		}
		private IEnumerator<object> ThreeFramesTask()
		{
			yield return null;
			yield return null;
		}
	}
}
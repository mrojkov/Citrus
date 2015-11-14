using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lime.Tests.Source.Widgets
{
	[TestFixture]
	public class TaskListTests
	{
		[Test]
		public void StopTest()
		{
			var list = new TaskList();
			var task1 = list.Add(OneFrameTask);
			var task2 = list.Add(OneFrameTask);
			var task3 = list.Add(OneFrameTask);
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
			var task1 = list.Add(OneFrameTask);
			var task2 = list.Add(OneFrameTask);
			Action watcher = () => { };
			task2.Watcher = watcher;
			var task3 = list.Add(OneFrameTask);
			list.Stop(t => t.Watcher == watcher);
			Assert.That(list.Contains(task1));
			Assert.That(list, Is.Not.Contains(task2));
			Assert.That(list.Contains(task3));
			Assert.That(task2.Completed);
		}

		[Test]
		public void StopByTagTest()
		{
			var list = new TaskList();
			var task1 = list.Add(OneFrameTask);
			var task2 = list.Add(OneFrameTask);
			var tag = new object();
			task2.Tag = tag;
			var task3 = list.Add(OneFrameTask);
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
			var task1 = list.Add(OneFrameTask());
			var task2 = list.Add(OneFrameTask);
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
			var task1 = list.Add(OneFrameTask(), tag1);
			var task2 = list.Add(OneFrameTask, tag2);
			Assert.That(list.Contains(task1));
			Assert.That(task1.Tag, Is.EqualTo(tag1));
			Assert.That(list.Contains(task2));
			Assert.That(task2.Tag, Is.EqualTo(tag2));
			Assert.That(task1, Is.Before(task2).In(list));
		}

		[Test]
		public void ReplaceTest()
		{
			Assert.Fail();
		}

		[Test]
		public void UpdateDuplicatingTest()
		{
			var list = new TaskList();
			 list.Add(UpdateDuplicateTestTask(list));
			list.Update(0);
			list.Update(0);
			Assert.Fail("Task haven't called Assert.Pass() on second Update(). Possible duplication of Update() call.");
		}

		[Test]
		public void UpdateTest()
		{
			const float UpdateDelta = 0.1f;
			var list = new TaskList();
			var task1 = list.Add(OneFrameTask);
			var task2 = list.Add(TwoFramesTask);
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

		private IEnumerator<object> UpdateDuplicateTestTask(TaskList list)
		{
			list.Update(0);
			yield return null;
			Assert.Pass();
		}

		private IEnumerator<object> OneFrameTask()
		{
			yield return null;
		}
		private IEnumerator<object> TwoFramesTask()
		{
			yield return null;
			yield return null;
		}
	}
}
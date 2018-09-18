using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Lime.Tests.Source.Widgets.Tasks
{
	[TestFixture]
	public class TaskTests
	{
		[Test]
		public void ToStringTest()
		{
			var nestingEnumerator = ToStringTask();
			var nestingToString = nestingEnumerator.GetType().ToString();
			nestingEnumerator.MoveNext();
			var nestedEnumerator = (IEnumerator<object>) nestingEnumerator.Current;
			var nestedToString = nestedEnumerator.GetType().ToString();
			var task = new Task(ToStringTask());
			Assert.That(task.ToString(), Is.EqualTo(nestingToString));
			task.Advance(0);
			Assert.That(task.ToString(), Is.EqualTo(nestedToString));
			task.Advance(0);
			Assert.That(task.ToString(), Is.EqualTo("Completed"));
		}

		private IEnumerator<object> ToStringTask()
		{
			yield return ToStringNestedTask();
		}

		private IEnumerator<object> ToStringNestedTask()
		{
			yield return null;
		}

		[Test]
		public void AdvanceDeltaTest()
		{
			var task = new Task(TwoFramesTask());
			task.Advance(0);
			Assert.That(task.Delta, Is.EqualTo(0));
			task.Advance(0.5f);
			Assert.That(task.Delta, Is.EqualTo(0.5f));
		}

		[Test]
		public void AdvanceCompletedTaskTest()
		{
			var task = new Task(TwoFramesTask());
			task.Advance(0);
			task.Advance(0);
			Assert.That(task.Completed);
			task.Advance(0);
			Assert.That(task.Completed);
		}

		[Test]
		public void AdvanceZeroYieldReturnNullTest()
		{
			var task = new Task(AdvanceYieldReturnNullTestTask());
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.True);
		}

		[Test]
		public void AdvanceFloatYieldReturnNullTest()
		{
			var task = new Task(AdvanceYieldReturnNullTestTask());
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnNullTestTask()
		{
			yield return null;
		}

		[Test]
		public void AdvanceZeroYieldReturnIntTest()
		{
			var task = new Task(AdvanceYieldReturnIntTestTask());
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
		}

		[Test]
		public void AdvanceFloatYieldReturnIntTest()
		{
			var task = new Task(AdvanceYieldReturnIntTestTask());
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnIntTestTask()
		{
			yield return 1;
		}

		[Test]
		public void AdvanceZeroYieldReturnFloatTest()
		{
			var task = new Task(AdvanceYieldReturnFloatTestTask());
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
		}

		[Test]
		public void AdvanceFloatYieldReturnFloatTest()
		{
			var task = new Task(AdvanceYieldReturnFloatTestTask());
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnFloatTestTask()
		{
			yield return 0.5f;
		}

		[Test]
		public void AdvanceYieldReturnIEnumeratorTest()
		{
			var task = new Task(AdvanceYieldReturnIEnumeratorTestTask());
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnIEnumeratorTestTask()
		{
			yield return AdvanceYieldReturnIEnumeratorTestNestedTask();
		}

		private IEnumerator<object> AdvanceYieldReturnIEnumeratorTestNestedTask()
		{
			yield return null;
		}

		[Test]
		public void AdvanceYieldReturnWaitPredicateTest()
		{
			var task = new Task(AdvanceYieldReturnWaitPredicateTestTask());
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.False);
			task.Advance(0.5f);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnWaitPredicateTestTask()
		{
			yield return Task.WaitWhile(totalTime => totalTime < 1);
		}

		[Test]
		public void AdvanceYieldReturnNodeTest()
		{
			var node = new Node();
			var animation = new Animation();
			animation.Markers.AddPlayMarker("Start", 0);
			animation.Markers.AddStopMarker("Stop", 1);
			node.Animations = new AnimationList(node) { animation };
			node.RunAnimation("Start");
			var task = new Task(AdvanceYieldReturnNodeTestTask(node));
			node.Update(1);
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			node.Update(1);
			task.Advance(0);
			Assert.That(task.Completed);
		}

		private IEnumerator<object> AdvanceYieldReturnNodeTestTask(Node node)
		{
			yield return node;
		}

		[Test]
		public void AdvanceYieldReturnIEnumerableTest()
		{
			Assert.Fail("Fix Task");
			var task = new Task(AdvanceYieldReturnIEnumerableTestTask());
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.True);
		}

		private IEnumerator<object> AdvanceYieldReturnIEnumerableTestTask()
		{
			yield return AdvanceYieldReturnIEnumerableTestNestedTask();
		}
		
		private IEnumerable<object> AdvanceYieldReturnIEnumerableTestNestedTask()
		{
			yield return null;
		}

		[Test]
		public void AdvanceYieldReturnOtherTest()
		{
			var task = new Task(AdvanceYieldReturnOtherTestTask());
			Assert.Catch(() => task.Advance(0));
		}

		private IEnumerator<object> AdvanceYieldReturnOtherTestTask()
		{
			yield return new object();
		}

		[Test]
		public void DisposeTest()
		{
			var task = new Task(DisposeNestingTask()) {Updating = () => { }};
			task.Advance(0);
			task.Dispose();
			Assert.That(task.Completed);
			Assert.That(task.Updating, Is.Null);
			task.Advance(0);
		}

		private IEnumerator<object> DisposeNestingTask()
		{
			yield return DisposeNestedTask();
		}

		private IEnumerator<object> DisposeNestedTask()
		{
			yield return null;
			Assert.Fail("Task wasn't disposed correctly.");
		}

		[Test]
		public void WaitWhileTest()
		{
			var shouldContinueTask = true;
			Func<bool> conditionProvider = () => shouldContinueTask;
			var task = new Task(WaitWhileTestTask(conditionProvider));
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			shouldContinueTask = false;
			task.Advance(0);
			Assert.That(task.Completed);
		}

		private IEnumerator<object> WaitWhileTestTask(Func<bool> shouldContinue)
		{
			yield return Task.WaitWhile(shouldContinue);
		}

		[Test]
		public void WaitWhileTest1()
		{
			var task = new Task(WaitWhileTest1Task());
			task.Advance(1);
			Assert.That(task.Completed, Is.False);
			task.Advance(1);
			Assert.That(task.Completed, Is.False);
			task.Advance(1);
			Assert.That(task.Completed);
		}

		private IEnumerator<object> WaitWhileTest1Task()
		{
			yield return Task.WaitWhile(totalTime => totalTime < 2);
		}

		[Test]
		public void WaitForAnimationTest()
		{
			var node = new Node();
			var animation = new Animation();
			animation.Markers.AddPlayMarker("Start", 0);
			animation.Markers.AddStopMarker("Stop", 1);
			node.Animations = new AnimationList(node) { animation };
			node.RunAnimation("Start");
			var task = new Task(WaitForAnimationTestTask(node));
			node.Update(1);
			task.Advance(0);
			Assert.That(task.Completed, Is.False);
			node.Update(1);
			task.Advance(0);
			Assert.That(task.Completed);
		}

		private IEnumerator<object> WaitForAnimationTestTask(Node node)
		{
			yield return Task.WaitForAnimation(node);
		}

		[Test]
		[Ignore("Test this method in other way")]
		public void ExecuteAsyncTest()
		{
			var sleepTime = TimeSpan.FromMilliseconds(100);
			var task = new Task(Task.ExecuteAsync(() => Thread.Sleep(sleepTime)));
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (!task.Completed) {
				task.Advance(0);
			}
			stopWatch.Stop();
			Assert.That(stopWatch.Elapsed, Is.EqualTo(sleepTime).Within(10).Milliseconds);
		}

		[Test]
		public void StopIfTest()
		{
			var shouldStopTask = false;
			Func<bool> conditionProvider = () => shouldStopTask;
			var task = new Task(StopIfTestTask(conditionProvider));
			task.Advance(0);
			shouldStopTask = true;
			task.Advance(0);
			Assert.That(task.Completed);
		}

		private IEnumerator<object> StopIfTestTask(Func<bool> shouldStop)
		{
			Task.StopIf(shouldStop);
			yield return null;
			Assert.Fail("Task wasn't stopped on provided condition.");
		}

		[Test]
		public void SinMotionTest()
		{
			var task = new Task(SinMotionTestTask());
			task.Advance(1);
			task.Advance(1f / 3f);
			task.Advance(1f / 6f);
			task.Advance(1f / 6f);
			task.Advance(1f / 3f);
			Assert.Fail("Task hasn't called Assert.Success.");
		}

		private IEnumerator<object> SinMotionTestTask()
		{
			var sequence = Task.SinMotion(1, 0, 1).GetEnumerator();
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(0));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(0.5f));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(Mathf.Sqrt(0.5f)).Within(Mathf.ZeroTolerance));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(Mathf.Sqrt(3) / 2).Within(Mathf.ZeroTolerance));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(1));
			Assert.Pass();
		}

		[Test]
		public void SqrtMotionTest()
		{
			var task = new Task(SqrtMotionTestTask());
			task.Advance(1);
			task.Advance(1);
			task.Advance(1);
			Assert.Fail("Task hasn't called Assert.Success.");
		}

		private IEnumerator<object> SqrtMotionTestTask()
		{
			var sequence = Task.SqrtMotion(2, 0, 1).GetEnumerator();
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(0));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(Mathf.Sqrt(0.5f)).Within(float.Epsilon));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(1));
			Assert.Pass();
		}

		[Test]
		public void LinearMotionTest()
		{
			var task = new Task(LinearMotionTestTask());
			task.Advance(1);
			task.Advance(1);
			task.Advance(1);
			Assert.Fail("Task hasn't called Assert.Success.");
		}

		private IEnumerator<object> LinearMotionTestTask()
		{
			var sequence = Task.LinearMotion(2, 0, 1).GetEnumerator();
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(0));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(0.5f));
			yield return null;
			sequence.MoveNext();
			Assert.That(sequence.Current, Is.EqualTo(1));
			Assert.Pass();
		}

		private IEnumerator<object> TwoFramesTask()
		{
			yield return null;
		}
	}
}
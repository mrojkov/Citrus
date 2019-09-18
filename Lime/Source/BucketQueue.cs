using System;
using System.Threading;

namespace Lime
{
	internal class BucketQueue<T>
	{
		private Bucket[] buckets;
		private int lastNearestBucketIndex;
		private int count;

		public int BucketCount => buckets.Length;
		public int Count => count;

		public BucketQueue(int bucketCount)
		{
			buckets = new Bucket[bucketCount];
			for (var i = 0; i < bucketCount; i++) {
				buckets[i] = new Bucket();
			}
			lastNearestBucketIndex = bucketCount;
		}

		public BucketQueueNode<T> Enqueue(int bucketIndex, T value)
		{
			var node = new BucketQueueNode<T>(value);
			Enqueue(bucketIndex, node);
			return node;
		}

		public void Enqueue(int bucketIndex, BucketQueueNode<T> node)
		{
			if (node.Queue != null) {
				throw new ArgumentException(nameof(node));
			}
			var bucket = buckets[bucketIndex];
			node.Queue = this;
			node.BucketIndex = bucketIndex;
			if (bucket.Last != null) {
				node.Prev = bucket.Last;
				bucket.Last.Next = node;
				bucket.Last = node;
			} else {
				bucket.First = node;
				bucket.Last = node;
			}
			if (lastNearestBucketIndex > bucketIndex) {
				lastNearestBucketIndex = bucketIndex;
			}
			count++;
		}

		public BucketQueueNode<T> Peek()
		{
			return PeekBucket().First;
		}

		public BucketQueueNode<T> Dequeue()
		{
			var b = PeekBucket();
			var node = b.First;
			b.First = node.Next;
			if (b.First != null) {
				b.First.Prev = null;
			} else {
				b.Last = null;
			}
			node.Queue = null;
			node.BucketIndex = -1;
			node.Prev = node.Next = null;
			count--;
			return node;
		}

		public bool Remove(BucketQueueNode<T> node)
		{
			if (node.Queue != this) {
				return false;
			}
			if (node.Prev != null) {
				node.Prev.Next = node.Next;
			} else {
				buckets[node.BucketIndex].First = node.Next;
			}
			if (node.Next != null) {
				node.Next.Prev = node.Prev;
			} else {
				buckets[node.BucketIndex].Last = node.Prev;
			}
			node.Queue = null;
			node.BucketIndex = -1;
			node.Prev = node.Next = null;
			count--;
			return true;
		}

		private Bucket PeekBucket()
		{
			while (lastNearestBucketIndex < buckets.Length && buckets[lastNearestBucketIndex].First == null) {
				lastNearestBucketIndex++;
			}
			return buckets[lastNearestBucketIndex];
		}

		public void Clear()
		{
			count = 0;
			lastNearestBucketIndex = buckets.Length;
			for (var i = 0; i < buckets.Length; i++) {
				ClearBucket(buckets[i]);
			}
		}

		private void ClearBucket(Bucket b)
		{
			for (var n = b.First; n != null; n = n.Next) {
				n.Queue = null;
				n.Prev = null;
				n.Next = null;
				n.BucketIndex = -1;
			}
			b.First = null;
			b.Last = null;
		}

		public static void Resize(ref BucketQueue<T> queue, int newBucketCount)
		{
			var newQueue = new BucketQueue<T>(newBucketCount);
			while (queue.Count > 0) {
				var bucketIndex = queue.Peek().BucketIndex;
				newQueue.Enqueue(bucketIndex, queue.Dequeue());
			}
			queue = newQueue;
		}

		private class Bucket
		{
			public BucketQueueNode<T> First;
			public BucketQueueNode<T> Last;
		}
	}

	internal class BucketQueueNode<T>
	{
		internal BucketQueueNode<T> Prev;
		internal BucketQueueNode<T> Next;

		public BucketQueue<T> Queue { get; internal set; }
		public int BucketIndex { get; internal set; } = -1;
		public T Value { get; set; }

		public BucketQueueNode() { }

		public BucketQueueNode(T value)
		{
			Value = value;
		}
	}
}

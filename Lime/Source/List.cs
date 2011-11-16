using System.Collections.Generic;
using System;
using System.Collections;

//[DebuggerDisplay ("Count={Count}")]
//[DebuggerTypeProxy (typeof(CollectionDebuggerView<>))]
public class Collection <T> : ICollection<T>
{
	T[] items;
	int size;
	int version;
	static readonly T[] EmptyArray = new T [0];
	const int DefaultCapacity = 4;

	public Collection ()
	{
		items = EmptyArray;
	}

	public void Add (T item)
	{
		// If we check to see if we need to grow before trying to grow
		// we can speed things up by 25%
		if (size == items.Length)
			GrowIfNeeded (1);
		items [size ++] = item;
		version++;
	}

	void GrowIfNeeded (int newCount)
	{
		int minimumSize = size + newCount;
		if (minimumSize > items.Length)
			Capacity = Math.Max (Math.Max (Capacity * 2, DefaultCapacity), minimumSize);
	}

	void CheckRange (int idx, int count)
	{
		if (idx < 0)
			throw new ArgumentOutOfRangeException ("index");

		if (count < 0)
			throw new ArgumentOutOfRangeException ("count");

		if ((uint)idx + (uint)count > (uint)size)
			throw new ArgumentException ("index and count exceed length of list");
	}

	public void Clear ()
	{
		Array.Clear (items, 0, items.Length);
		size = 0;
		version++;
	}

	public bool Contains (T item)
	{
		return Array.IndexOf<T> (items, item, 0, size) != -1;
	}

	public void CopyTo (T [] array, int arrayIndex)
	{
		Array.Copy (items, 0, array, arrayIndex, size);
	}

	public Enumerator GetEnumerator ()
	{
		return new Enumerator (this);
	}

	public int IndexOf (T item)
	{
		return Array.IndexOf<T> (items, item, 0, size);
	}

	void Shift (int start, int delta)
	{
		if (delta < 0)
			start -= delta;

		if (start < size)
			Array.Copy (items, start, items, start + delta, size - start);

		size += delta;

		if (delta < 0)
			Array.Clear (items, size, -delta);
	}

	void CheckIndex (int index)
	{
		if (index < 0 || (uint)index > (uint)size)
			throw new ArgumentOutOfRangeException ("index");
	}

	public void Insert (int index, T item)
	{
		CheckIndex (index);
		if (size == items.Length)
			GrowIfNeeded (1);
		Shift (index, 1);
		items [index] = item;
		version++;
	}

	public bool Remove (T item)
	{
		int loc = IndexOf (item);
		if (loc != -1)
			RemoveAt (loc);
		return loc != -1;
	}

	public void RemoveAt (int index)
	{
		if (index < 0 || (uint)index >= (uint)size)
			throw new ArgumentOutOfRangeException ("index");
		Shift (index, -1);
		Array.Clear (items, size, 1);
		version++;
	}

	public int Capacity {
		get { 
			return items.Length;
		}
		set {
			if ((uint)value < (uint)size)
				throw new ArgumentOutOfRangeException ();
				
			Array.Resize (ref items, value);
		}
	}

	public int Count {
		get { return size; }
	}

	public T this [int index] {
		get { return items [index]; } 
		set { items [index] = value; }
	}

	IEnumerator <T> IEnumerable <T>.GetEnumerator ()
	{
		return GetEnumerator ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}

	
	bool ICollection <T>.IsReadOnly {
		get { return false; }
	}

	public struct Enumerator : IEnumerator <T>, IDisposable
	{
		Collection <T> l;
		int next;
		int ver;
		T current;

		internal Enumerator (Collection <T> l) : this ()
		{
			this.l = l;
			ver = l.version;
		}
			
		public void Dispose ()
		{
			l = null;
		}

		void VerifyState ()
		{
			if (l == null)
				throw new ObjectDisposedException (GetType ().FullName);
			if (ver != l.version)
				throw new InvalidOperationException (
						"Collection was modified; enumeration operation may not execute.");
		}

		public bool MoveNext ()
		{
			VerifyState ();

			if (next < 0)
				return false;

			if (next < l.size) {
				current = l.items [next++];
				return true;
			}

			next = -1;
			return false;
		}

		public T Current {
			get { return current; }
		}

		void IEnumerator.Reset ()
		{
			VerifyState ();
			next = 0;
		}

		object IEnumerator.Current {
			get {
				VerifyState ();
				if (next <= 0)
					throw new InvalidOperationException ();
				return current;
			}
		}
	}
}

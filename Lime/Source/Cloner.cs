using System;

namespace Lime
{
	/// <summary>
	/// Utility class that contain methods for cloning objects.
	/// </summary>
	public static class Cloner
	{
		/// <summary>
		/// Clone the object. If source object implements ICloneable then the method will return object produced by ICloneable.Clone().
		/// Otherwise Serialization.Clone() will be used.
		/// </summary>
		/// <typeparam name="T">A type of object that need to be returned.</typeparam>
		/// <param name="obj">A source object to clone.</param>
		/// <returns></returns>
		public static T Clone<T>(T obj) => (T)Clone((object)obj);

		/// <summary>
		/// Clone the object. If source object implements ICloneable then the method will return object produced by ICloneable.Clone().
		/// Otherwise Serialization.Clone() will be used.
		/// </summary>
		/// <param name="obj">A source object to clone.</param>
		/// <returns></returns>
		public static object Clone(object obj)
		{
			return obj is ICloneable cloneable ? cloneable.Clone() : InternalPersistence.Instance.Clone(obj);
		}
	}
}

using NUnit.Framework.Constraints;
using System;
using System.Collections;
using NUnitIs = NUnit.Framework.Is;

namespace Lime.Tests
{
	// This wrapper is used to support creating custom Is methods.
	// Maybe NUnit 3.0 will bring better solution for this problem
	internal static partial class Is
	{
		/// <summary>
		/// Returns a constraint that tests two items for equality 
		/// </summary>
		public static EqualConstraint EqualTo(object expected)
		{
			return NUnitIs.EqualTo(expected);
		}

		/// <summary>
		/// Returns a constraint that tests that two references are the same object
		/// </summary>
		public static SameAsConstraint SameAs(object expected)
		{
			return NUnitIs.SameAs(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is greater than the suppled argument 
		/// </summary>
		public static GreaterThanConstraint GreaterThan(object expected)
		{
			return NUnitIs.GreaterThan(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is greater than or equal to the suppled argument 
		/// </summary>
		public static GreaterThanOrEqualConstraint GreaterThanOrEqualTo(object expected)
		{
			return NUnitIs.GreaterThanOrEqualTo(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is greater than or equal to the suppled argument 
		/// </summary>
		public static GreaterThanOrEqualConstraint AtLeast(object expected)
		{
			return NUnitIs.AtLeast(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is less than the suppled argument 
		/// </summary>
		public static LessThanConstraint LessThan(object expected)
		{
			return NUnitIs.LessThan(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is less than or equal to the suppled argument 
		/// </summary>
		public static LessThanOrEqualConstraint LessThanOrEqualTo(object expected)
		{
			return NUnitIs.LessThanOrEqualTo(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the
		/// actual value is less than or equal to the suppled argument 
		/// </summary>
		public static LessThanOrEqualConstraint AtMost(object expected)
		{
			return NUnitIs.AtMost(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual
		/// value is of the exact type supplied as an argument. 
		/// </summary>
		public static ExactTypeConstraint TypeOf(Type expectedType)
		{
			return NUnitIs.TypeOf(expectedType);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual
		/// value is of the exact type supplied as an argument. 
		/// </summary>
		public static ExactTypeConstraint TypeOf<T>()
		{
			return NUnitIs.TypeOf<T>();
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is of the type supplied as an argument or a derived type. 
		/// </summary>
		public static InstanceOfTypeConstraint InstanceOf(Type expectedType)
		{
			return NUnitIs.InstanceOf(expectedType);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is of the type supplied as an argument or a derived type. 
		/// </summary>
		public static InstanceOfTypeConstraint InstanceOf<T>()
		{
			return NUnitIs.InstanceOf<T>();
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is assignable from the type supplied as an argument. 
		/// </summary>
		public static AssignableFromConstraint AssignableFrom(Type expectedType)
		{
			return NUnitIs.AssignableFrom(expectedType);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is assignable from the type supplied as an argument. 
		/// </summary>
		public static AssignableFromConstraint AssignableFrom<T>()
		{
			return NUnitIs.AssignableFrom<T>();
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is assignable from the type supplied as an argument. 
		/// </summary>
		public static AssignableToConstraint AssignableTo(Type expectedType)
		{
			return NUnitIs.AssignableTo(expectedType);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is assignable from the type supplied as an argument. 
		/// </summary>
		public static AssignableToConstraint AssignableTo<T>()
		{
			return NUnitIs.AssignableTo<T>();
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is a collection containing the same elements as the
		/// collection supplied as an argument. 
		/// </summary>
		public static CollectionEquivalentConstraint EquivalentTo(IEnumerable expected)
		{
			return NUnitIs.EquivalentTo(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value
		/// is a subset of the collection supplied as an argument. 
		/// </summary>
		public static CollectionSubsetConstraint SubsetOf(IEnumerable expected)
		{
			return NUnitIs.SubsetOf(expected);
		}

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value contains the substring supplied as an argument. 
		/// </summary>
		public static SubstringConstraint StringContaining(string expected)
		{
			return NUnitIs.StringContaining(expected);
		}

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value starts with the substring supplied as an argument. 
		/// </summary>
		public static StartsWithConstraint StringStarting(string expected)
		{
			return NUnitIs.StringStarting(expected);
		}

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value ends with the substring supplied as an argument. 
		/// </summary>
		public static EndsWithConstraint StringEnding(string expected)
		{
			return NUnitIs.StringEnding(expected);
		}

		/// <summary>
		/// Returns a constraint that succeeds if the actual
		/// value matches the regular expression supplied as an argument. 
		/// </summary>
		public static RegexConstraint StringMatching(string pattern)
		{
			return NUnitIs.StringMatching(pattern);
		}

		/// <summary>
		/// Returns a constraint that tests whether the path provided
		/// is the same as an expected path after canonicalization. 
		/// </summary>
		public static SamePathConstraint SamePath(string expected)
		{
			return NUnitIs.SamePath(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the path provided
		/// is under an expected path after canonicalization. 
		/// </summary>
		public static SubPathConstraint SubPath(string expected)
		{
			return NUnitIs.SubPath(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the path provided
		/// is the same path or under an expected path after canonicalization. 
		/// </summary>
		public static SamePathOrUnderConstraint SamePathOrUnder(string expected)
		{
			return NUnitIs.SamePathOrUnder(expected);
		}

		/// <summary>
		/// Returns a constraint that tests whether the actual value falls
		/// within a specified range. 
		/// </summary>
		public static RangeConstraint<T> InRange<T>(T from, T to) where T : IComparable<T>
		{
			return NUnitIs.InRange(@from, to);
		}

		/// <summary>
		/// Returns a ConstraintExpression that negates any
		/// following constraint. 
		/// </summary>
		public static ConstraintExpression Not
		{
			get { return NUnitIs.Not; }
		}

		/// <summary>
		/// Returns a ConstraintExpression, which will apply
		/// the following constraint to all members of a collection,
		/// succeeding if all of them succeed. 
		/// </summary>
		public static ConstraintExpression All
		{
			get { return NUnitIs.All; }
		}

		/// <summary>
		/// Returns a constraint that tests for null
		/// 
		/// </summary>
		public static NullConstraint Null
		{
			get { return NUnitIs.Null; }
		}

		/// <summary>
		/// Returns a constraint that tests for True 
		/// </summary>
		public static TrueConstraint True
		{
			get { return NUnitIs.True; }
		}

		/// <summary>
		/// Returns a constraint that tests for False 
		/// </summary>
		public static FalseConstraint False
		{
			get { return NUnitIs.False; }
		}

		/// <summary>
		/// Returns a constraint that tests for a positive value 
		/// </summary>
		public static GreaterThanConstraint Positive
		{
			get { return NUnitIs.Positive; }
		}

		/// <summary>
		/// Returns a constraint that tests for a negative value 
		/// </summary>
		public static LessThanConstraint Negative
		{
			get { return NUnitIs.Negative; }
		}

		/// <summary>
		/// Returns a constraint that tests for NaN 
		/// </summary>
		public static NaNConstraint NaN
		{
			get { return NUnitIs.NaN; }
		}

		/// <summary>
		/// Returns a constraint that tests for empty
		/// 
		/// </summary>
		public static EmptyConstraint Empty
		{
			get { return NUnitIs.Empty; }
		}

		/// <summary>
		/// Returns a constraint that tests whether a collection
		///             contains all unique items.
		/// 
		/// </summary>
		public static UniqueItemsConstraint Unique
		{
			get { return NUnitIs.Unique; }
		}

		/// <summary>
		/// Returns a constraint that tests whether an object graph is serializable in binary format.
		/// 
		/// </summary>
		public static BinarySerializableConstraint BinarySerializable
		{
			get { return NUnitIs.BinarySerializable; }
		}

		/// <summary>
		/// Returns a constraint that tests whether an object graph is serializable in xml format.
		/// 
		/// </summary>
		public static XmlSerializableConstraint XmlSerializable
		{
			get { return NUnitIs.XmlSerializable; }
		}

		/// <summary>
		/// Returns a constraint that tests whether a collection is ordered
		/// 
		/// </summary>
		public static CollectionOrderedConstraint Ordered
		{
			get { return NUnitIs.Ordered; }
		}
	}
}